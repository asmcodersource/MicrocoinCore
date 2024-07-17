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
using static Microcoin.Microcoin.Network.ChainFethingNetwork.FetcherSession.FetcherSession;

namespace Microcoin.Microcoin.ChainFetcher
{
    public class FetchRequestHandler
    {
        public readonly int ChainBranchBlocksCount;
        public readonly FetchRequest Request;
        public event Action<MutableChain>? ChainFetched;
        public event Action? ChainIsntFetched;
        public event Action<INodeConnection>? SessionFinishedSuccesful;
        public event Action<INodeConnection>? SessionFinishedFaulty;
        public MutableChain? DownloadedChain { get; private set; }

        public FetchRequestHandler(FetchRequest fetchRequest, int chainBranchBlocksCount) 
        {
            Request = fetchRequest;
            ChainBranchBlocksCount = chainBranchBlocksCount;
        }

        public async Task<bool> StartHandling(Node communicationNode, AbstractChain sourceChain, CancellationToken cancellationToken)
        {
            try
            {
                var connections = communicationNode.GetNodeConnections();
                var nonAnonymousConnections = connections.Where(node => node.OppositeSidePublicKey != null);
                // TODO: sort by chain providers rating
                foreach (var connection in nonAnonymousConnections)
                {
                    try
                    {
                        var session = new Session(communicationNode);
                        var connectionResult = await session.Connect(connection.OppositeSidePublicKey, "chain-fetching");
                        if (connectionResult is ConnectionResult.Connected)
                        {
                            var fetcherSession = new FetcherSession(session, sourceChain, Request, ChainBranchBlocksCount);
                            DownloadedChain = await fetcherSession.StartDonwloadingProccess(cancellationToken);
                            ChainFetched?.Invoke(DownloadedChain);
                            SessionFinishedSuccesful?.Invoke(connection);
                            return true;
                        }
                    }
                    catch (ChainDownloadingException ex)
                    {
                        SessionFinishedFaulty?.Invoke(connection);
                    }
                }
            }
            catch( OperationCanceledException) { }
            finally
            {
                ChainIsntFetched?.Invoke();
            }
            return false;
        }
    }
}
