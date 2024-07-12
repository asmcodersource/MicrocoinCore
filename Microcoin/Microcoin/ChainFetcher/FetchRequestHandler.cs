using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin.Network.ChainFethingNetwork.FetcherSession;
using NodeNet.NodeNet;
using NodeNet.NodeNet.Communication;
using NodeNet.NodeNetSession.Session;

namespace Microcoin.Microcoin.ChainFetcher
{
    public class FetchRequestHandler
    {
        public readonly FetchRequest Request;

        public FetchRequestHandler(FetchRequest fetchRequest) 
        {
            Request = fetchRequest;
        }

        public async Task StartHandling(Node communicationNode, AbstractChain sourceChain)
        {
            var connections = communicationNode.GetNodeConnections();
            var nonAnonymousConnections = connections.Where(node => node.OppositeSidePublicKey != null);
            // TODO: sort by chain providers rating
            foreach (var connection in nonAnonymousConnections)
            {
                var session = new Session(communicationNode);
                var connectionResult = await session.Connect(connection.OppositeSidePublicKey, "chain-fetching");
                if (connectionResult is ConnectionResult.Connected)
                {
                    var fetcherSession = new FetcherSession(session, sourceChain, Request);
                    await fetcherSession.StartDonwloadingProccess(CancellationToken.None);
                }
            }
            throw new NotImplementedException();
        }
    }
}
