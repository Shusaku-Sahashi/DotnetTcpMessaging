using System;
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
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            var startSyncChan = Channel.CreateUnbounded<bool>();
            using var client = new Client(clientCon);
            
            var task = Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        await StartMessagePumpAsync(client, startSyncChan, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Something wrong");

                        // errorが発生した場合は、IoLoopAsyncを止める。
                        cts.Cancel();
                    }
                },
                 cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();

            await startSyncChan.Reader.WaitToReadAsync(cts.Token);

            while(!cts.Token.IsCancellationRequested)
            {
                // TODO: 多分この処理だとReadがTimeoutにならないようなので確認する。
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

        private static async Task StartMessagePumpAsync(Client client, ChannelWriter<bool> startSyncChannel, CancellationToken cancellationToken)
        {
            using var heartbeatTickerTicker = new TimerTicker(HeartbeatInterval);
            var heartbeatChannel = heartbeatTickerTicker.GetChannel(() => new Tick(), cancellationToken);

            startSyncChannel.Complete();

            var channels = new[] { heartbeatChannel, };
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