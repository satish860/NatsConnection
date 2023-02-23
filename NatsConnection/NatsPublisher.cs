using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NatsConnection
{
    public class NatsPublisher : INatsPublisher
    {
        private readonly INatsConnection natsConnection;

        public NatsPublisher(INatsConnection natsConnection)
        {
            this.natsConnection = natsConnection;
        }

        public ValueTask<bool> PublishAsync<T>(string subject, T message)
        {
            var publishCommand = JsonSerializer.Serialize(message);
            var encodedCommand = Encoding.UTF8.GetBytes(publishCommand);
            var pubCommand = Encoding.UTF8.GetBytes($"PUB {subject} {encodedCommand.Length}\r\n{publishCommand}\r\n");
            return natsConnection.SendAsync(1, pubCommand);
        }

        public ValueTask<bool> PublishAsync<T>(string subject, T message, Dictionary<string, string> headers)
        {
            var publishCommand = JsonSerializer.Serialize(message);
            var encodedCommand = Encoding.UTF8.GetBytes(publishCommand);
            StringBuilder builder = new StringBuilder();
            builder.Append("NATS/1.0\r\n");
            foreach (var item in headers)
            {
                builder.Append(item.Key);
                builder.Append(": ");
                builder.Append(item.Value);
                builder.Append("\r\n");
            }
            builder.Append("\r\n");
            var header = builder.ToString();
            var headerBytes = Encoding.UTF8.GetBytes(header);
            var totalbytes = headerBytes.Length + encodedCommand.Length;
            var command = $"HPUB {subject} {headerBytes.Length} {totalbytes}\r\n{header}{publishCommand}\r\n";
            var pubCommand = Encoding.UTF8.GetBytes(command);
            return natsConnection.SendAsync(1, pubCommand);
        }
    }
}
