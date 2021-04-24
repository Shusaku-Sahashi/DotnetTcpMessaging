using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;

namespace MessagingServer.Test
{
    [TestFixture]
    public class TcpServer
    {
        private Task _runTask;
        private IHost _host;

        [OneTimeSetUp]
        public void SetUp()
        {
            _host = Program.CreateHostBuilder(new string[] { }).Build();
            _runTask = _host.RunAsync();
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await _host.StopAsync();
            await _runTask;
        }

        [Test]
        public async Task ClientTest()
        {
            using var client = new TcpClient("127.0.0.1", 8080);
            await using var ns = client.GetStream();
            using var rs = new StreamReader(ns);
            await using var ws = new StreamWriter(ns);
            
            var task = Task.Factory.StartNew(async () =>
            {
                await ws.WriteLineAsync("started?");
                await ws.FlushAsync();

                for (var i = 0; i < 3; i++)
                {
                    var message = await rs.ReadLineAsync();
                    Assert.That(message, Is.EqualTo("heartbeat"));
                }
            }, CancellationToken.None).Unwrap();
            
            await task;
        }
    }
}