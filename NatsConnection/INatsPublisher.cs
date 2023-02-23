using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NatsConnection
{
    public interface INatsPublisher
    {
        ValueTask<bool> PublishAsync<T>(string subject,T message);

        ValueTask<bool> PublishAsync<T>(string subject,T message,Dictionary<string,string> headers);
    }
}
