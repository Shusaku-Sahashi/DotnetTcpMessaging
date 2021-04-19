using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MessagingServer
{
    public class CommandHandler : IDisposable
    {
        private readonly ConcurrentDictionary<string, TcpClient> _cons = new ConcurrentDictionary<string, TcpClient>();

        public async Task HandleAsync(TcpClient clientConn, CancellationToken cancellationToken)
        {
            var ipAddress = IpAddress(clientConn);
            _cons.AddOrUpdate(ipAddress, _ => clientConn, (_, _) => clientConn);

            var executor = new MessageExecutor();

            try
            {
                await executor.IoLoopAsync(clientConn, cancellationToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            _cons.TryRemove(ipAddress, out _);
        }

        private static string IpAddress(TcpClient clientConn)
        {
            var endpoint = clientConn.Client.RemoteEndPoint as IPEndPoint;
            if (endpoint == null) throw new InvalidOperationException();

            return endpoint.Address.ToString();
        }

        public void Dispose()
        {
            foreach (var conn in _cons)
            {
                conn.Value.Close();
            }
        }
    }
}