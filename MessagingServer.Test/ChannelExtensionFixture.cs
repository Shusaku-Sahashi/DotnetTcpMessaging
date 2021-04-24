using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace MessagingServer.Test
{
    [TestFixture]
    public class ChannelExtensionFixture
    {
        [Test]
        public async Task CreateTickerChannel()
        {
            var heartbeatTicker = new TimerTicker(TimeSpan.FromSeconds(1));
            var channel = heartbeatTicker.GetChannel(() => new Tick());

            var res = await channel.WaitToReadAsync();
            
            Assert.True(res);
        }

        public record Tick : ChannelExtension.IMessage;
    }
}