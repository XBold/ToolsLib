using System.Net;
using System.Net.Sockets;
using System.Text;
using Tools.ValidationsAndExeptions;
using static Tools.Logger.Logger;
using Tools.Logger;

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

                    Log($"Connected to server {serverIp}:{serverPort}", Severity.INFO);

                    await UpdateConnectionStateAsync(true);

                    // Start listening for incoming messages
                    _ = ReceiveMessagesAsync(_cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    Log("Connection terminated succesfully after requesting disconnection", Severity.INFO);
                    await UpdateConnectionStateAsync(false);
                }
                catch (SocketException socketEx)
                {
                    if (socketEx.Message == "Connection refused")
                    {
                        Log("Connection refused", Severity.INFO);
                    }
                    else
                    {
                        Log($"Socket exception: {socketEx.Message}", Severity.CRITICAL);
                    }
                    await UpdateConnectionStateAsync(false);
                }
                catch (Exception ex)
                {
                    Log($"Error connecting to server: {ex.Message}", Severity.CRITICAL);
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
                Log("Disconnected from server.", Severity.INFO);

                _ = UpdateConnectionStateAsync(false);

                if (OnDisconnection != null)
                    OnDisconnection.Invoke();
            }
            catch (Exception ex)
            {
                Log($"Error during disconnection: {ex.Message}", Severity.CRITICAL);
            }
        }

        public async Task SendMessageAsync(string message)
        {
            if (_stream == null || !_client!.Connected)
            {
                Log("Cannot send message. Not connected to the server.", Severity.CRITICAL);
                return;
            }

            try
            {
                var data = Encoding.UTF8.GetBytes(message);
                await _stream.WriteAsync(data, 0, data.Length);
                Log($"Sent to server: {message}", Severity.INFO);
            }
            catch (Exception ex)
            {
                Log($"Error sending message: {ex.Message}", Severity.CRITICAL);
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
                        Log("Server disconnected.", Severity.WARNING);
                        Disconnect();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Error receiving messages: {ex.Message}", Severity.CRITICAL);
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
