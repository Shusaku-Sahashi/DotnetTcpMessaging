using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MessagingServer
{
    public class CommandHandler : IDisposable
    {
        private readonly ConcurrentDictionary<string, TcpClient> _cons = new ConcurrentDictionary<string, TcpClient>();
        private static ILogger<CommandHandler> _logger;
        private static ICommandExecutor _executor;

        public CommandHandler(ILogger<CommandHandler> logger, ICommandExecutor executor)
        {
            _executor = executor;
            _logger = logger;
        }

        public async Task HandleAsync(TcpClient clientConn, CancellationToken cancellationToken)
        {
            var ipAddress = IpAddress(clientConn);
            _cons.AddOrUpdate(ipAddress, _ => clientConn, (_, _) => clientConn);

            try
            {
                await _executor.IoLoopAsync(clientConn, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"client({ipAddress})");
            }

            _cons.TryRemove(ipAddress, out _);
        }

        private static string IpAddress(TcpClient clientConn)
        {
            var endpoint = clientConn.Client.RemoteEndPoint as IPEndPoint;
            if (endpoint == null) throw new InvalidOperationException();

            return endpoint.Address.ToString();
        }

        public void CloseAll()
        {
            foreach (var conn in _cons)
            {
                conn.Value.Close();
            }

            _cons.Clear();
        }

        public void Dispose()
        {
            CloseAll();
        }
    }
}