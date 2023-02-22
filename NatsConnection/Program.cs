﻿using System.Buffers;
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
            SubscribeData(natsConnection);
            _ = Task.Run(() => PingPeriodically(natsConnection, new CancellationToken()));
            _ = Task.Run(() => PublishContinously(natsConnection, new CancellationToken()));
            //foreach (var item in Enumerable.Range(0,10000))
            //{
            //    PublishData(natsConnection, new Person());
            //}
            

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

        public static async void PublishData(NatsConnection natsConnection,Person person)
        {
            Console.WriteLine("I am publishing the data");
            var publishCommand = JsonSerializer.Serialize(person);
            var encodedCommand = Encoding.UTF8.GetBytes(publishCommand);
            var pubCommand = Encoding.UTF8.GetBytes($"PUB FOO {encodedCommand.Length}\r\n{publishCommand}\r\n");
            natsConnection.SendAsync(pubCommand);
        }

        public static async void SubscribeData(NatsConnection natsConnection)
        {
            var pubCommand = Encoding.UTF8.GetBytes($"SUB bar 1\r\n");
            natsConnection.SendAsync(pubCommand);
        }

        public static async void UnSubscribe(NatsConnection natsConnection)
        {
            var pubCommand = Encoding.UTF8.GetBytes($"UNSUB 1\r\n");
            natsConnection.SendAsync(pubCommand);
        }

        
        public static async void PingPeriodically(NatsConnection natsConnection,CancellationToken cancellationToken)
        {
            var periodicTimer = new PeriodicTimer(TimeSpan.FromMinutes(2));
            while(!cancellationToken.IsCancellationRequested)
            {
                var pongJSON = Encoding.UTF8.GetBytes("PING\r\n");
                natsConnection.SendAsync(pongJSON);
                await periodicTimer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false);  
            }
        }

        public static async void PublishContinously(NatsConnection natsConnection,CancellationToken cancellationToken)
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