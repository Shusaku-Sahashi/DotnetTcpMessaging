using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MessagingServer
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var server = new TcpListener(IPAddress.Parse("127.0.0.1"), 8080);
            server.Start();

            IList<Task> workers = new List<Task>();

            using (var handler = new CommandHandler())
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var client = await server.AcceptTcpClientAsync();

                    var task = Task.Factory.StartNew(async arg =>
                    {
                        Debug.Assert(arg is TcpClient, "arg is TcpClient");
                        var tcpClient = (TcpClient) arg;

                        try
                        {
                            await handler.HandleAsync(tcpClient, stoppingToken);
                        }
                        finally
                        {
                            tcpClient.Dispose();
                        }
                    }, client, stoppingToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                    workers.Add(task);
                }
            }

            await Task.WhenAll(workers);
        }
    }
}