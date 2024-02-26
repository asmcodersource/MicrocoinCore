using Microcoin.Network.NodeNet;
using Microcoin.RSAEncryptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class NodeNetUnitTests
    {
        [Fact]
        public void NodeNet_Communications_Connection_Test()
        {
            Node first_node = Node.CreateRSAHttpNode(
                RSAEncryption.CreateSignOptions(),
                new Microcoin.Network.NodeNet.TcpCommunication.TcpListenerOptions(1333)
            );
            Node second_node = Node.CreateRSAHttpNode(
                RSAEncryption.CreateSignOptions(),
                new Microcoin.Network.NodeNet.TcpCommunication.TcpListenerOptions(1334)
            );

            var isConnectionSuccess = first_node.Connect("127.0.0.1:1334");
            Assert.True(isConnectionSuccess);

            first_node.Close();
            second_node.Close();
        }

        [Fact]
        public async void NodeNet_Communications_Messaging_Normal_Test()
        {
            Node first_node = Node.CreateRSAHttpNode(
                RSAEncryption.CreateSignOptions(),
                new Microcoin.Network.NodeNet.TcpCommunication.TcpListenerOptions(1335)
            );
            Node second_node = Node.CreateRSAHttpNode(
                RSAEncryption.CreateSignOptions(),
                new Microcoin.Network.NodeNet.TcpCommunication.TcpListenerOptions(1336)
            );

            var isConnectionSuccess = first_node.Connect("127.0.0.1:1336");
            Assert.True(isConnectionSuccess);
            await Task.Delay(5000);

            int received_summary = 0;
            int sending_summary = 0;
            second_node.MessageReceived += (msgcontext) => { lock (this) { received_summary += 1; }  };
            for (int i = 0; i < 1024; i++)
            {
                sending_summary++;
                first_node.SendMessage(i.ToString());
            }

            await Task.Delay(256);
            Assert.True(received_summary == sending_summary);

            first_node.Close();
            second_node.Close();
        }

        [Fact]
        public void NodeNet_Communications_Messaging_Flooding_Test()
        {
            Node first_node = Node.CreateRSAHttpNode(
                RSAEncryption.CreateSignOptions(),
                new Microcoin.Network.NodeNet.TcpCommunication.TcpListenerOptions(1337)
            );
            Node second_node = Node.CreateRSAHttpNode(
                RSAEncryption.CreateSignOptions(),
                new Microcoin.Network.NodeNet.TcpCommunication.TcpListenerOptions(1338)
            );

            var isConnectionSuccess = first_node.Connect("127.0.0.1:1338");
            Assert.True(isConnectionSuccess);

            first_node.Close();
            second_node.Close();
        }
    }
}
