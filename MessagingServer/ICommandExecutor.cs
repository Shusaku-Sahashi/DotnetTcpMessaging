using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MessagingServer
{
    public interface ICommandExecutor
    {
        public Task IoLoopAsync(TcpClient client, CancellationToken cancellationToken);
    }
}