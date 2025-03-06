using System.Net.Sockets;
using System.Text;
using Tools.ValidationsAndExeptions;
using static Tools.Logger.Logger;
using static Tools.Network.NetworkConstants;
using Tools.Logger;

namespace Tools.Network
{
    public class TCP_Client
    {
        private TcpClient? _client;
        private NetworkStream? _stream;
        private CancellationTokenSource? _cancellationTokenSource;

        /// <summary>
        /// Event when a new message is received
        /// </summary>
        public event Func<byte[], Task>? OnMessageReceived;
        /// <summary>
        /// Event when the client is disconnected
        /// </summary>
        public event Func<Task>? OnDisconnection;
        /// <summary>
        /// Event when the connection state changes
        /// </summary>
        public event Func<bool, Task>? OnConnectionStateChanged;

        /// <summary>
        /// Create a new TCP client
        /// <para/>
        /// Throw an exepction <see cref="ParameterValidationException"/> if parmaters are not valid
        /// </summary>
        /// <param name="serverIp">Specify the server IP</param>
        /// <param name="serverPort">Specify the server port</param>
        /// <exception cref="ParameterValidationException"/>
        public async Task ConnectAsync(string serverIp, int serverPort = DefaultPort)
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

        private static ValidationResult ValidateParameters(string ip, int port)
        {
            var result = new ValidationResult();

            if (!NetCheck.PortInRange(port))
            {
                result.AddError(PortName, $"Port must be between {MinPort} and {MaxPort}.");
            }
            
            if (!NetCheck.IsStandardIPAddress(ip))
            {
                result.AddError(IPName, "Invalid IP address format.");
            }

            return result;
        }

        /// <summary>
        /// Disconnect from the server
        /// </summary>
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

        /// <summary>
        /// Send a message to the server
        /// </summary>
        /// <param name="message">Insert the message that has to be sent</param>
        /// <returns></returns>
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
