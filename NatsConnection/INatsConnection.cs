using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NatsConnection
{
    public interface INatsConnection
    {
        ValueTask<bool> SendAsync(int id,byte[] bytes);
    }
}
