using System;
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
            // StartAsyncを使用する場合、 using で囲まないとゾンビプロセスになるので RunAsync を使用する。
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

            client.ReceiveTimeout = (int) TimeSpan.FromSeconds(6).TotalMilliseconds;
            var task = Task.Factory.StartNew(async () =>
            {
                await ws.WriteLineAsync("started?");
                await ws.FlushAsync();

                for (var i = 0; i < 3; i++)
                {
                    var message = await rs.ReadLineAsync();
                    // 通信が切れている場合、0になる。
                    // ref: https://qiita.com/kurasho/items/275612d408d32923eabd
                    Assert.NotZero(message.Length);
                    Assert.Equals(message, "heartbeat");
                }
                
            }, CancellationToken.None).Unwrap();
            
            await task;
        }
    }
}