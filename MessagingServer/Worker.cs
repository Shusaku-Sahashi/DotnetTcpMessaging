using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MessagingServer
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private static CommandHandler _handler;

        public Worker(ILogger<Worker> logger, CommandHandler commandHandler)
        {
            _logger = logger;
            _handler = commandHandler;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var server = new TcpListener(IPAddress.Parse("127.0.0.1"), 8080);
            server.Start();

            IList<Task> workers = new List<Task>();

            while (!stoppingToken.IsCancellationRequested)
            {
                var receivedChan = Channel.CreateUnbounded<bool>();
                var task = Task.Factory.StartNew(async () =>
                {
                    using var client = await server.AcceptTcpClientAsync();

                    receivedChan.Writer.Complete();

                    await _handler.HandleAsync(client, stoppingToken);
                }, stoppingToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                // complete されたらここに来る。
                await receivedChan.Reader.WaitToReadAsync(stoppingToken);

                workers.Add(task);
            }

            await Task.WhenAll(workers);
        }
    }
}