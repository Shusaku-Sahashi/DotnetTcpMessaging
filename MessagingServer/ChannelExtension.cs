using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace MessagingServer
{
    public class ChannelExtension
    {
        public static ChannelReader<IMessage>
            Merge(IEnumerable<ChannelReader<IMessage>> channels, CancellationToken cancellationToken = default)
        {
            var output = Channel.CreateUnbounded<IMessage>();

            foreach (var chan in channels)
            {
                Task.Factory.StartNew(async arg =>
                {
                    var chanReader = arg as ChannelReader<IMessage>;
                    await foreach (var item in chanReader!.ReadAllAsync(cancellationToken))
                        await output.Writer.WriteAsync(item, cancellationToken);
                }, chan, cancellationToken);
            }

            return output;
        }

        public static ChannelReader<IMessage> CreateTickerChannel(TimeSpan interval, Func<IMessage> creator,
            CancellationToken cancellationToken = default)
        {
            var output = Channel.CreateUnbounded<IMessage>();
            var timer = new Timer(interval.TotalMilliseconds);
            timer.Elapsed += async (s, e) => await output.Writer.WriteAsync(creator(), cancellationToken);
            timer.Start();

            return output;
        }


        public interface IMessage
        {
        }
    }
}