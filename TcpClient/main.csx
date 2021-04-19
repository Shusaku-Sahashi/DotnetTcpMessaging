#!/usr/bin/env dotnet-script
using System.Threading;
using System.Net;
using System.Net.Sockets;

var tasks = new List<Task>();
var rand = new Random();
foreach (var _ in Enumerable.Range(0, 1))
{
    var task = Task.Factory.StartNew(async () =>
       {
           var client = new TcpClient("127.0.0.1", 8080);

           var ns = client.GetStream();
           await ns.WriteAsync(Encoding.UTF8.GetBytes("Hello\n"), CancellationToken.None);

           var buf = new byte[256];
           await ns.ReadAsync(buf, 0, buf.Length);

           System.Console.WriteLine("end");

           System.Console.WriteLine(buf.ToString());
       }, CancellationToken.None).Unwrap();

    tasks.Add(task);
}

Task.WaitAll(tasks.ToArray());