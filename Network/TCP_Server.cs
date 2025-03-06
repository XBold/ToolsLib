using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Concurrent;
using static Tools.Logger.Logger;
using Tools.ValidationsAndExeptions;
using static Tools.Network.NetworkConstants;
using Tools.Logger;

namespace Tools.Network
{
    public class TCP_Server
    {
        private TcpListener? _server;
        private CancellationTokenSource? _cancellationTokenSource;
        private readonly IPAddress _ip = IPAddress.Any;
        private readonly int _port;

        // A thread-safe collection for managing connected clients
        private readonly ConcurrentDictionary<TcpClient, NetworkStream> _connectedClients = new();

        /// <summary>
        /// Event to detect when a client is connected
        /// </summary>
        public event Func<TcpClient, Task>? OnClientConnected;
        /// <summary>
        /// Event to detect when a client is disconnected
        /// </summary>
        public event Func<TcpClient, Task>? OnClientDisconnected;
        /// <summary>
        /// Event when a new message is received
        /// </summary>
        public event Func<TcpClient, byte[], Task>? OnMessageReceived;

        /// <summary>Create a new TCP server with a specific IP</summary>
        /// <param name="ip"> IP address to bind the server to</param>
        /// <param name="validationResult">Give a validation result to check if there are some errors. <para/>Use <see cref="ValidationResult.GetFormattedErrors"/> to have a string with all the details</param>
        /// <param name="port">Specify a port to open the server (if not, the default will be <see cref="DefaultPort">1812</see>)</param>
        public static TCP_Server? Create(string ip, out ValidationResult validationResult, int port = DefaultPort)
        {
            validationResult = ValidateParameters(ip, port);
            if (!validationResult.IsValid)
            {
                return null;
            }

            return new TCP_Server(ip, port);
        }

        /// <summary>Create a new TCP server without setting a specific IP (it will be used the IP of the running machine)</summary>
        /// <param name="validationResult">Give a validation result to check if there are some errors. <para/>Use <see cref="ValidationResult.GetFormattedErrors"/> to have a string with all the details</param>
        /// <param name="port">Specify a port to open the server (if not, the default will be <see cref="DefaultPort">1812</see>)</param>
        public static TCP_Server? Create(out ValidationResult validationResult, int port = DefaultPort)
        {
            validationResult = ValidateParameters(port);
            if (!validationResult.IsValid)
            {
                return null;
            }

            return new TCP_Server(port);
        }

        private TCP_Server(int port)
        {
            _port = port;
        }

        private TCP_Server(string ip, int port)
        {
            _ip = IPAddress.Parse(ip);
            _port = port;
        }

        private static ValidationResult ValidateParameters(string ip, int port)
        {
            var result = new ValidationResult();

            if (!NetCheck.PortInRange(port))
            {
                result.AddError(PortName, $"Port must be between {MinPort} and {MaxPort}.");
            }

            if (!IPAddress.TryParse(ip, out _))
            {
                result.AddError(IPName, "Invalid IP address format.");
            }

            return result;
        }

        private static ValidationResult ValidateParameters(int port)
        {
            var result = new ValidationResult();

            if (!NetCheck.PortInRange(port))
            {
                result.AddError(PortName, $"Port must be between {MinPort} and {MaxPort}.");
            }

            return result;
        }

        /// <summary>
        /// Start the server and wait for clients to connect
        /// </summary>
        public async Task StartServerAsync()
        {
            _server = new TcpListener(_ip, _port);
            _server.Start();
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            Log("Server started...", Severity.INFO);
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var client = await _server.AcceptTcpClientAsync(cancellationToken);
                    Log("Client connected", Severity.INFO);

                    // Store client and its stream
                    if (!_connectedClients.TryAdd(client, client.GetStream()))
                    {
                        Log("Client already connected", Severity.INFO);
                        return;
                    }

                    // Trigger OnClientConnected
                    if (OnClientConnected != null)
                        await OnClientConnected.Invoke(client);            

                    // Start handling the client
                    _ = HandleClientAsync(client, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    Log("Server stop request succesfully received", Severity.INFO);
                }
                catch (Exception ex)
                {
                    Log($"Unexpected error: {ex.Message}", Severity.CRITICAL);
                }
            }
        }

        /// <summary>
        /// Stop the server and close all the connected clients
        /// </summary>
        public void StopServer()
        {
            _cancellationTokenSource?.Cancel();
            foreach (var client in _connectedClients.Keys)
            {
                client.Close();
            }
            _server?.Stop();
            Log("Server stopped.", Severity.INFO);
        }

        /// <summary>
        /// Send a message to a specific client
        /// </summary>
        /// <param name="client">Insert the <see cref="TcpClient">client</see> that need to receive the message</param>
        /// <param name="message">Insert the message that need to be sent</param>
        public async Task SendMessageAsync(TcpClient client, string message)
        {
            if (_connectedClients.TryGetValue(client, out var stream))
            {
                var data = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(data, 0, data.Length);
                Log($"Sent to client: {message}", Severity.INFO);
            }
        }

        /// <summary>
        /// Send a message to all connected clients
        /// </summary>
        /// <param name="message">Insert the message that need to be sent</param>
        public async Task BroadcastMessageAsync(string message)
        {
            var tasks = new Task[_connectedClients.Count];
            var index = 0;

            foreach (var stream in _connectedClients.Values)
            {
                var data = Encoding.UTF8.GetBytes(message);
                tasks[index++] = stream.WriteAsync(data, 0, data.Length);
            }

            await Task.WhenAll(tasks);
            Log($"Broadcasted: {message}", Severity.INFO);
        }

        /// <summary>
        /// Get a list of all connected clients
        /// </summary>
        public List<TcpClient>? GetTcpClients()
        {
            if (!_connectedClients.IsEmpty)
            {
                return [.. _connectedClients.Keys];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get the first connected client
        /// </summary>
        public TcpClient? GetFirstTcpClient()
        {
            return !_connectedClients.IsEmpty ? _connectedClients.Keys.FirstOrDefault() : null;
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            var stream = client.GetStream();
            var buffer = new byte[1024];

            while (!cancellationToken.IsCancellationRequested)
            {
                int bytesRead;
                try
                {
                    bytesRead = await stream.ReadAsync(buffer, cancellationToken);
                }
                catch
                {
                    Log("Connection closed by client.", Severity.INFO);
                    break;
                }

                if (bytesRead > 0)
                {
                    var data = new byte[bytesRead];
                    Array.Copy(buffer, data, bytesRead);

                    // Trigger OnMessageReceived action
                    if (OnMessageReceived != null)
                        await OnMessageReceived.Invoke(client, data);
                }
                else
                {
                    break;
                }
            }

            if(!_connectedClients.TryRemove(client, out _))
            {
                Log("Not possible to disconnet che client", Severity.FATAL_ERROR);
            };
            if (OnClientDisconnected != null)
                await OnClientDisconnected.Invoke(client);
            client.Close();
        }
    }
}
