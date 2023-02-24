using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TcpClient = NetCoreServer.TcpClient;

namespace NatsConnection
{
    public class NatsConnection : TcpClient, INatsConnection
    {
        private bool _stop;
        Parser _parser;
        Dictionary<int, string> keyValuePairs;
        public NatsConnection(string address, int port) : base(address, port)
        {
            _parser = new Parser();
            keyValuePairs = new Dictionary<int, string>
            {
                {541545293,"MSG" },
                {1330007625,"INFO" },
                {1196312912,"PING" },
                {1196314448,"PONG" },
                {223039275,"OK" },
                {1381123373,"ERROR" }
            };
        }

        protected override void OnConnected()
        {
            Console.WriteLine($"NATS TCP client connected a new session with Id {Id}");
        }


        protected override void OnDisconnected()
        {
            Console.WriteLine($"NATS TCP client disconnected a session with Id {Id}");

            // Wait for a while...
            Thread.Sleep(1000);

            // Try to connect again
            if (!_stop)
                ConnectAsync();
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            Span<byte> buff = buffer.AsSpan().Slice((int)offset, (int)size);
            //Console.WriteLine(Encoding.UTF8.GetString(buffer, (int)offset, (int)size));
            Console.WriteLine(_parser.GetServerCode(buffer,keyValuePairs));
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"NATS TCP client caught an error with code {error}");
        }



        public ValueTask<bool> SendAsync(int id, byte[] bytes)
        {
            return ValueTask.FromResult(this.SendAsync(bytes));
        }


    }
}
