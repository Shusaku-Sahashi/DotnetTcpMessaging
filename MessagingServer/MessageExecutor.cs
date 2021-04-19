using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MessagingServer
{
    public class MessageExecutor : ICommandExecutor
    {
        private const int HeartbeatInterval = 5000;

        public async Task IoLoopAsync(TcpClient client, CancellationToken cancellationToken)
        {
            await using var stream = client.GetStream();
            using var sr = new StreamReader(stream);
            await using var sw = new StreamWriter(stream);

            while(true)
            {
                client.ReceiveTimeout = HeartbeatInterval * 2;

                string? line;
                try
                {
                    line = await sr.ReadLineAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"failed read command");
                    break;
                }

                Console.WriteLine(line);

                await sw.WriteLineAsync("Hello\n");
                await sw.FlushAsync();
            }
        }
    }
}