using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MessagingServer
{
    public class Client : IDisposable
    {
        private TcpClient client;
        private StreamWriter writer;
        private StreamReader reader;

        public int ReceiveTimeout
        {
            get => client.ReceiveTimeout;
            set => client.ReceiveTimeout = value;
        }

        public Client(TcpClient tcpClient)
        {
            client = tcpClient;
            writer = new StreamWriter(client.GetStream());
            reader = new StreamReader(client.GetStream());
        }

        public async ValueTask<string?> ReadMessageAsync()
        {
            return await reader.ReadLineAsync();
        }

        public void WriteMessage(string message, TimeSpan sentTimeout)
        {
            lock (writer)
            {
                client.SendTimeout = sentTimeout.Milliseconds;
                writer.WriteLine(message);
                writer.Flush();
            }
        }

        public void Dispose()
        {
            writer.Dispose();
            reader.Dispose();
        }
    }
}