using System;
using System.Threading;
using System.Threading.Channels;
using Timer = System.Timers.Timer;

namespace MessagingServer
{
    public sealed class TimerTicker : IDisposable
    {
        private readonly Timer _timer;

        public TimerTicker(TimeSpan interval)
        {
            _timer = new Timer(interval.TotalMilliseconds);
        }

        public ChannelReader<ChannelExtension.IMessage> GetChannel(Func<ChannelExtension.IMessage> creator,
            CancellationToken cancellationToken = default)
        {
            var output = Channel.CreateUnbounded<ChannelExtension.IMessage>();
            _timer.Elapsed += async (s, e) => await output.Writer.WriteAsync(creator(), cancellationToken);
            _timer.Start();

            return output;
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}