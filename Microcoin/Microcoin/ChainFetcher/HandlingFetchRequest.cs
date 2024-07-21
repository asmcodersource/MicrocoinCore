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
    public class HandlingFetchRequest
    {
        public readonly int ChainBranchBlocksCount;
        public readonly FetchRequest Request;
        public readonly ChainProvidersRating ChainProvidersRating;
        public event Action<ChainDownloadingResult>? ChainFetched;
        public event Action? ChainIsntFetched;
        public event Action<string>? SessionFinishedSuccesful;
        public event Action<string>? SessionFinishedFaulty;

        public HandlingFetchRequest(FetchRequest fetchRequest, int chainBranchBlocksCount, ChainProvidersRating chainProvidersRating) 
        {
            Request = fetchRequest;
            ChainBranchBlocksCount = chainBranchBlocksCount;
            ChainProvidersRating = chainProvidersRating;
        }

        public async Task<bool> StartHandling(Node communicationNode, AbstractChain sourceChain, CancellationToken cancellationToken)
        {
            try
            {
                var connections = communicationNode.GetNodeConnections();
                var bestProviders = ChainProvidersRating.GetRatingSortedProviders(
                    (connections ?? new List<INodeConnection>())
                        .Where(c => c.OppositeSidePublicKey is not null)
                        .Select(c => c.OppositeSidePublicKey!)
                        .ToList()
                );
                // TODO: sort by chain providers rating
                foreach (var provider in bestProviders)
                {
                    try
                    {
                        var session = new Session(communicationNode);
                        var connectionResult = await session.Connect(provider, "chain-fetching");
                        if (connectionResult is ConnectionResult.Connected)
                        {
                            var fetcherSession = new FetcherSession(session, sourceChain, Request, ChainBranchBlocksCount);
                            var fetchResult = await fetcherSession.StartDonwloadingProccess(cancellationToken);
                            Serilog.Log.Debug("Some chain fetched!");
                            ChainFetched?.Invoke(fetchResult);
                            SessionFinishedSuccesful?.Invoke(provider);
                            return true;
                        }
                    }
                    catch (ChainDownloadingException ex)
                    {
                        Serilog.Log.Error(ex.Message);
                        SessionFinishedFaulty?.Invoke(provider);
                    }
                }
            }
            catch (Exception ex )
            {
                Serilog.Log.Error(ex.Message);
            }
            ChainIsntFetched?.Invoke();
            return false;
        }
    }
}
