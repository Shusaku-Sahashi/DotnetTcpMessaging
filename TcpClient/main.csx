#!/usr/bin/env dotnet-script
using System.Threading;
using System.Net;
using System.Net.Sockets;

var tasks = new List<Task>();
var rand = new Random();
foreach (var _ in Enumerable.Range(0, 1))
{
    var client = new TcpClient("127.0.0.1", 8080);
    var task = Task.Factory.StartNew(async () =>
       {
           var ns = client.GetStream();
           var rs = new StreamReader(ns);
           var ws = new StreamWriter(ns);

           await ws.WriteAsync("hoge\n");
           ws.Flush();
           await rs.ReadLineAsync();

           System.Console.WriteLine("Start client");

           while (true)
           {
               var message = await rs.ReadLineAsync();

               await Task.Delay(1000);

               Console.WriteLine(message);
           }
       }, CancellationToken.None).Unwrap();

    tasks.Add(task);
}

Task.WaitAll(tasks.ToArray());