using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MessagingServer
{
    public class MessageExecutor : ICommandExecutor
    {
        private static readonly TimeSpan HeartbeatInterval = TimeSpan.FromMilliseconds(5000);
        private readonly ILogger<MessageExecutor> _logger;

        public MessageExecutor(ILogger<MessageExecutor> logger)
        {
            _logger = logger;
        }

        public async Task IoLoopAsync(TcpClient clientCon, CancellationToken cancellationToken)
        {
            var startSyncChan = Channel.CreateUnbounded<bool>();
            using var client = new Client(clientCon);
            
            var task = Task.Factory.StartNew(async () =>
                {
                    await StartMessagePumpAsync(client, startSyncChan, cancellationToken);
                },
                 cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();

            await startSyncChan.Reader.WaitToReadAsync(cancellationToken);

            while(true)
            {
                client.ReceiveTimeout = HeartbeatInterval.Milliseconds * 2;

                string? line;
                try
                {
                    line = await client.ReadMessageAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"failed read command");
                    break;
                }

                Console.WriteLine(line);
            }

            await task;
        }

        private static async Task StartMessagePumpAsync(Client client, ChannelWriter<bool> chan, CancellationToken cancellationToken)
        {
            var heartbeatTickerChan =
                ChannelExtension.CreateTickerChannel(HeartbeatInterval, () => new Tick(), cancellationToken);
            
            chan.Complete();

            var channels = new ChannelReader<ChannelExtension.IMessage>[] { heartbeatTickerChan, };
            while (!cancellationToken.IsCancellationRequested)
            {
                await foreach (var msg in ChannelExtension.Merge(channels, cancellationToken).ReadAllAsync(cancellationToken))
                {
                    switch (msg)
                    {
                        case Tick:
                            client.WriteMessage("heartbeat", HeartbeatInterval);
                            continue;
                        default:
                            throw new NotSupportedException();
                    }
                }
            }
        }

        private record Tick : ChannelExtension.IMessage;
    }
}