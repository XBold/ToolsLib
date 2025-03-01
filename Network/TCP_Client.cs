using System.Net;
using System.Net.Sockets;
using System.Text;
using Tools.ValidationsAndExeptions;

namespace Tools.Network
{
    public class TCP_Client
    {
        private TcpClient? _client;
        private NetworkStream? _stream;
        private CancellationTokenSource? _cancellationTokenSource;
        private readonly IPAddress _ip = IPAddress.Any;
        private readonly int _port;

        public event Func<byte[], Task>? OnMessageReceived;
        public event Func<Task>? OnDisconnection;
        public event Func<bool, Task>? OnConnectionStateChanged;

        public async Task ConnectAsync(string serverIp, int serverPort = NetworkConstants.DefaultPort)
        {
            var validationResult = ValidateParameters(serverIp, serverPort);
            if (!validationResult.IsValid)
            {
                await UpdateConnectionStateAsync(false);
                throw new ParameterValidationException("Paramteres not ok", validationResult);
            }
            else
            {
                _client = new TcpClient();
                _cancellationTokenSource = new CancellationTokenSource();

                try
                {
                    await _client.ConnectAsync(serverIp, serverPort, _cancellationTokenSource.Token);
                    _stream = _client.GetStream();

                    Logger.Log($"Connected to server {serverIp}:{serverPort}", Logger.Severity.INFO);

                    await UpdateConnectionStateAsync(true);

                    // Start listening for incoming messages
                    _ = ReceiveMessagesAsync(_cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    Logger.Log("Connection terminated succesfully after requesting disconnection", Logger.Severity.INFO);
                    await UpdateConnectionStateAsync(false);
                }
                catch (SocketException socketEx)
                {
                    if (socketEx.Message == "Connection refused")
                    {
                        Logger.Log("Connection refused", Logger.Severity.INFO);
                    }
                    else
                    {
                        Logger.Log($"Socket exception: {socketEx.Message}", Logger.Severity.CRITICAL);
                    }
                    await UpdateConnectionStateAsync(false);
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error connecting to server: {ex.Message}", Logger.Severity.CRITICAL);
                    await UpdateConnectionStateAsync(false);
                    throw;
                }
            }
        }

        public static ValidationResult ValidateParameters(string ip, int port)
        {
            var result = new ValidationResult();

            if (!NetCheck.PortInRange(port))
            {
                result.AddError(NetworkConstants.PortName, $"Port must be between {NetworkConstants.MinPort} and {NetworkConstants.MaxPort}.");
            }
            
            if (!NetCheck.IsStandardIPAddress(ip))
            {
                result.AddError(NetworkConstants.IPName, "Invalid IP address format.");
            }

            return result;
        }

        public void Disconnect()
        {
            try
            {
                _cancellationTokenSource?.Cancel();
                _client?.Close();
                _client = null;
                Logger.Log("Disconnected from server.", Logger.Severity.INFO);

                _ = UpdateConnectionStateAsync(false);

                if (OnDisconnection != null)
                    OnDisconnection.Invoke();
            }
            catch (Exception ex)
            {
                Logger.Log($"Error during disconnection: {ex.Message}", Logger.Severity.CRITICAL);
            }
        }

        public async Task SendMessageAsync(string message)
        {
            if (_stream == null || !_client!.Connected)
            {
                Logger.Log("Cannot send message. Not connected to the server.", Logger.Severity.CRITICAL);
                return;
            }

            try
            {
                var data = Encoding.UTF8.GetBytes(message);
                await _stream.WriteAsync(data, 0, data.Length);
                Logger.Log($"Sent to server: {message}", 0);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error sending message: {ex.Message}", Logger.Severity.CRITICAL);
            }
        }

        private async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
        {
            if (_stream == null)
                return;

            var buffer = new byte[1024];

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    int bytesRead = await _stream.ReadAsync(buffer, cancellationToken);
                    if (bytesRead > 0)
                    {
                        var data = new byte[bytesRead];
                        Array.Copy(buffer, data, bytesRead);

                        // Trigger OnMessageReceived event
                        if (OnMessageReceived != null)
                            await OnMessageReceived.Invoke(data);
                    }
                    else
                    {
                        Logger.Log("Server disconnected.", Logger.Severity.WARNING);
                        Disconnect();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error receiving messages: {ex.Message}", Logger.Severity.CRITICAL);
                Disconnect();
            }
        }

        private async Task UpdateConnectionStateAsync(bool isConnected)
        {
            if (OnConnectionStateChanged != null)
                await OnConnectionStateChanged.Invoke(isConnected);
        }

    }
}
