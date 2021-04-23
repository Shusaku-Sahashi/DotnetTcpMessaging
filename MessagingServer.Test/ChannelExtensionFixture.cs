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
            var heartbeatTickerChan =
                ChannelExtension.CreateTickerChannel(TimeSpan.FromSeconds(1), () => new Tick());

            var res = await heartbeatTickerChan.WaitToReadAsync();
            
            Assert.True(res);
        }

        public record Tick : ChannelExtension.IMessage;
    }
}