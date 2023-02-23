using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Reflection.Metadata;
using System.IO;
using System.Data;
using System.Reflection.PortableExecutable;
using System.Threading;

namespace NatsConnection
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.ReadKey();
            NatsConnection natsConnection = new NatsConnection("127.0.0.1", 4222);
            natsConnection.ConnectAsync();
            var optionsJson = JsonSerializer.Serialize(ConnectOptions.Default);
            var connectCommand = $"CONNECT {optionsJson}\r\n";
            var encodedCommand = Encoding.UTF8.GetBytes(connectCommand);
            natsConnection.SendAsync(encodedCommand);
            PingPeriodically(natsConnection, new CancellationToken());
            INatsPublisher natsPublisher = new NatsPublisher(natsConnection);
            Dictionary<string,string> keyValuePairs= new Dictionary<string,string>();
            keyValuePairs.Add("Hello", "World");
            await natsPublisher.PublishAsync<Person>("cli.demo", new Person { }, keyValuePairs);
            //PublishContinously(natsConnection, new CancellationToken());
            Console.ReadKey();

            UnSubscribe(natsConnection);

            Console.ReadKey();
        }


        public static async Task SendAsyncCommand(PipeWriter writer)
        {
            var optionsJson = JsonSerializer.Serialize(ConnectOptions.Default);
            var connectCommand = $"S {optionsJson}\r\n";
            var encodedCommand = Encoding.UTF8.GetBytes(connectCommand);
            await writer.WriteAsync(encodedCommand);
            await writer.FlushAsync();
        }

        public static async void PublishData(NatsConnection natsConnection, Person person)
        {
            var publishCommand = JsonSerializer.Serialize(person);
            var encodedCommand = Encoding.UTF8.GetBytes(publishCommand);
            var pubCommand = Encoding.UTF8.GetBytes($"PUB FOO {encodedCommand.Length}\r\n{publishCommand}\r\n");
            natsConnection.SendAsync(pubCommand);
        }

        public static async Task SubscribeData(NatsConnection natsConnection)
        {
            var pubCommand = Encoding.UTF8.GetBytes($"SUB bar 1\r\n");
            natsConnection.SendAsync(pubCommand);
        }

        public static async void PublishDataWithHeader(NatsConnection natsConnection, Person person)
        {
            Console.WriteLine("I am publishing the data");
            var publishCommand = JsonSerializer.Serialize(person);
            var encodedCommand = Encoding.UTF8.GetBytes("Hello NATS!");
            StringBuilder builder = new StringBuilder();
            builder.Append("NATS/1.0\r\n");
            builder.Append("Bar: Baz\r\n\r\n");
            var header = builder.ToString();
            var headerBytes = Encoding.UTF8.GetBytes(header);
            var totalbytes = headerBytes.Length+encodedCommand.Length;
            var command = $"HPUB FOO {headerBytes.Length} {totalbytes}\r\n{header}{"Hello Nats!"}\r\n";
            var pubCommand = Encoding.UTF8.GetBytes(command);
            natsConnection.SendAsync(pubCommand);
        }



        public static async void UnSubscribe(NatsConnection natsConnection)
        {
            var pubCommand = Encoding.UTF8.GetBytes($"UNSUB 1\r\n");
            natsConnection.SendAsync(pubCommand);
        }


        public static async void PingPeriodically(NatsConnection natsConnection, CancellationToken cancellationToken)
        {
            var periodicTimer = new PeriodicTimer(TimeSpan.FromMinutes(2));
            while (!cancellationToken.IsCancellationRequested)
            {
                var pongJSON = Encoding.UTF8.GetBytes("PING\r\n");
                natsConnection.SendAsync(pongJSON);
                await periodicTimer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public static async void PublishContinously(NatsConnection natsConnection, CancellationToken cancellationToken)
        {
            var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            while (!cancellationToken.IsCancellationRequested)
            {
                PublishData(natsConnection, new Person());
                await periodicTimer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false);
            }
        }








    }
}