using Microcoin.Microcoin.Network;
using MockNetwork.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockNetwork.Entities
{
    public class MockBroadcastNodeEndpoint : CommunicationEndPoint
    {
        public MockBroadcastNode WrappedNode { get; init; }

        public MockBroadcastNodeEndpoint(MockBroadcastNode node)
            => WrappedNode = node;
    }
}
