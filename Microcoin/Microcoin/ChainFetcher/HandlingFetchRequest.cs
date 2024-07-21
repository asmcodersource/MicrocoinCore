using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin.Network.ChainFethingNetwork.FetcherSession;
using NodeNet.NodeNet;
using NodeNet.NodeNet.Communication;
using NodeNet.NodeNetSession.Session;
using Serilog;
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
                var bestProviders = GetBestProviders(connections);

                foreach (var provider in bestProviders)
                {
                    if (await TryFetchChainFromProvider(communicationNode, sourceChain, provider, cancellationToken))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unhandled exception during chain fetching.");
            }

            ChainIsntFetched?.Invoke();
            return false;
        }

        private ICollection<string> GetBestProviders(IEnumerable<INodeConnection>? connections)
        {
            return ChainProvidersRating.GetRatingSortedProviders(
                (connections ?? new List<INodeConnection>())
                .Where(c => c.OppositeSidePublicKey != null)
                .Select(c => c.OppositeSidePublicKey!)
                .ToList()
            );
        }

        private async Task<bool> TryFetchChainFromProvider(Node communicationNode, AbstractChain sourceChain, string provider, CancellationToken cancellationToken)
        {
            try
            {
                using var session = new Session(communicationNode);
                var connectionResult = await session.Connect(provider, "chain-fetching");

                if (connectionResult == ConnectionResult.Connected)
                {
                    var fetcherSession = new FetcherSession(session, sourceChain, Request, ChainBranchBlocksCount);
                    var fetchResult = await fetcherSession.StartDonwloadingProccess(cancellationToken);

                    Log.Debug("Chain fetched.");
                    ChainFetched?.Invoke(fetchResult);
                    SessionFinishedSuccesful?.Invoke(provider);
                    return true;
                }
            }
            catch (ChainDownloadingException ex)
            {
                //Log.Error(ex, $"Error fetching chain from provider {provider}: {ex.Message}");
                SessionFinishedFaulty?.Invoke(provider);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Unhandled exception fetching chain from provider {provider}: {ex.Message}");
                SessionFinishedFaulty?.Invoke(provider);
            }
            return false;
        }
    }
}
