using AlterNats;
using System;

namespace Subscribe
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await using var conn = new NatsConnection();

            // for subscriber. await register to NATS server(not means await complete)
            var subscription = await conn.SubscribeAsync<Person>("FOO", x =>
            {
                Console.WriteLine($"Received {x}");
            });

            Console.WriteLine("Subscription started and ready for publish press enter");
            Console.ReadKey();
            foreach (var item in Enumerable.Range(1, 10))
            {
                Console.WriteLine(item);
                await conn.PublishAsync<string>("bar", $"{item}: Hello World!!");
            }


            Console.ReadKey();

        }
    }
}