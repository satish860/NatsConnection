using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NatsConnection
{
    internal class Parser
    {
         
        public Parser() 
        {

        }

        public string GetServerCode(ReadOnlySpan<byte> data,Dictionary<int,string> keyValuePairs)
        {
            var span = data.Slice(0, 4);
            var code = Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference<byte>(span));
            return keyValuePairs[code];
        }
    }


    internal static class ServerCode
    {
        public const int Info = 1330007625;  // Encoding.ASCII.GetBytes("INFO") |> MemoryMarshal.Read<int>
        public const int Msg = 541545293;    // Encoding.ASCII.GetBytes("MSG ") |> MemoryMarshal.Read<int>
        public const int Ping = 1196312912;  // Encoding.ASCII.GetBytes("PING") |> MemoryMarshal.Read<int>
        public const int Pong = 1196314448;  // Encoding.ASCII.GetBytes("PONG") |> MemoryMarshal.Read<int>
        public const int Ok = 223039275;     // Encoding.ASCII.GetBytes("+OK\r") |> MemoryMarshal.Read<int>
        public const int Error = 1381123373; // Encoding.ASCII.GetBytes("-ERR") |> MemoryMarshal.Read<int>
    }
}
