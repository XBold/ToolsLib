using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Concurrent;

namespace Tools.Network
{
    public class TCPServer
    {
        private TcpListener? _server;
        private CancellationTokenSource? _cancellationTokenSource;
        private const int _defaultPort = 1812;
        private const int _minPortOk = 1020;
        private const int _maxPortOk = 65535;
        private readonly IPAddress _ip = IPAddress.Any;
        private readonly int _port;

        // A thread-safe collection for managing connected clients
        private readonly ConcurrentDictionary<TcpClient, NetworkStream> _connectedClients = new();

        public event Func<TcpClient, Task>? OnClientConnected;
        public event Func<TcpClient, byte[], Task>? OnMessageReceived;

        public static TCPServer? Create(string ip, out ValidationResult validationResult, int port = NetworkConstants.DefaultPort)
        {
            validationResult = ValidateParameters(ip, port);
            if (!validationResult.IsValid)
            {
                return null;
            }

            return new TCPServer(ip, port);
        }

        public static TCPServer? Create(out ValidationResult validationResult, int port = NetworkConstants.DefaultPort)
        {
            validationResult = ValidateParameters(port);
            if (!validationResult.IsValid)
            {
                return null;
            }

            return new TCPServer(port);
        }

        private TCPServer(int port)
        {
            _port = port;
        }

        private TCPServer(string ip, int port)
        {
            _ip = IPAddress.Parse(ip);
            _port = port;
        }

        public static ValidationResult ValidateParameters(string ip, int port)
        {
            var result = new ValidationResult();

            if (!Math.IsInrange(port, NetworkConstants.MinPort, NetworkConstants.MaxPort))
            {
                result.AddError(NetworkConstants.PortName, $"Port must be between {_minPortOk} and {_maxPortOk}.");
            }

            if (!IPAddress.TryParse(ip, out _))
            {
                result.AddError(NetworkConstants.IPName, "Invalid IP address format.");
            }

            return result;
        }

        public static ValidationResult ValidateParameters(int port)
        {
            var result = new ValidationResult();

            if (!Math.IsInrange(port, _minPortOk, _maxPortOk))
            {
                result.AddError(NetworkConstants.PortName, $"Port must be between {_minPortOk} and {_maxPortOk}.");
            }

            return result;
        }

        public async Task StartServerAsync()
        {
            _server = new TcpListener(_ip, _port);
            _server.Start();
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            Logger.Log("Server started...", 0);
            while (!cancellationToken.IsCancellationRequested)
            {
                var client = await _server.AcceptTcpClientAsync();
                Logger.Log("Client connected.", 0);

                // Store client and its stream
                _connectedClients.TryAdd(client, client.GetStream());

                // Trigger OnClientConnected
                if (OnClientConnected != null)
                    await OnClientConnected.Invoke(client);

                // Start handling the client
                _ = HandleClientAsync(client, cancellationToken);
            }
        }

        public void StopServer()
        {
            _cancellationTokenSource?.Cancel();
            foreach (var client in _connectedClients.Keys)
            {
                client.Close();
            }
            _server?.Stop();
            Logger.Log("Server stopped.", 0);
        }

        public async Task SendMessageAsync(TcpClient client, string message)
        {
            if (_connectedClients.TryGetValue(client, out var stream))
            {
                var data = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(data, 0, data.Length);
                Logger.Log($"Sent to client: {message}", 0);
            }
        }

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
            Logger.Log($"Broadcasted: {message}", 0);
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
                    Logger.Log("Connection closed by client.", 0);
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
                    Logger.Log("Client disconnected.", 0);
                    break;
                }
            }

            if(!_connectedClients.TryRemove(client, out _))
            {
                Logger.Log("Not possible to disconnet che client", 3);
            };
            client.Close();
        }
    }
}
