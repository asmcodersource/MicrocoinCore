using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Network.NodeNet.TcpCommunication
{
    public class TcpListenerOptions
    {
        public int Port { get; set; } = 8080;


        public TcpListenerOptions() { }

        public TcpListenerOptions(int port)
        {
            Port = port;
        }
    }
}
