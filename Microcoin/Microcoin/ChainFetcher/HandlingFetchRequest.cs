using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin.Network;
using Microcoin.Microcoin.Network.ChainFethingNetwork.FetcherSession;
using SimpleInjector;

namespace Microcoin.Microcoin.ChainFetcher
{
    public class HandlingFetchRequest
    {
        public readonly FetchRequest Request;
        private readonly ISessionManager sessionManager;
        private readonly IEndPointCollectionProvider endPointCollectionProvider;
        private readonly IChainProvidersRating chainProvidersRating;
        private readonly Container servicesContainer;
        private readonly int chainBranchBlocksCount;

        public event Action<ChainDownloadingResult>? ChainFetched;
        public event Action? ChainIsntFetched;

        public HandlingFetchRequest(FetchRequest fetchRequest, Container servicesContainer)
        {
            Request = fetchRequest;
            this.servicesContainer = servicesContainer;
            this.chainBranchBlocksCount = servicesContainer.GetInstance<ChainBranchBlocksCount>().Value;
            this.chainProvidersRating = servicesContainer.GetInstance<IChainProvidersRating>();
            this.endPointCollectionProvider = servicesContainer.GetInstance<IEndPointCollectionProvider>();
            this.sessionManager = servicesContainer.GetInstance<ISessionManager>();
        }

        public async Task<bool> StartHandling(AbstractChain sourceChain, CancellationToken cancellationToken)
        {
            var endPoints = endPointCollectionProvider.GetEndPoints();
            var bestProviders = chainProvidersRating.GetRatingSortedProviders(endPoints);
            foreach (var providerSession in bestProviders)
            {
                if (await TryFetchChainFromProvider(providerSession, sourceChain, cancellationToken))
                {
                    return true;
                }
            }
            ChainIsntFetched?.Invoke();
            return false;
        }


        private async Task<bool> TryFetchChainFromProvider(CommunicationEndPoint endPoint, AbstractChain sourceChain, CancellationToken cancellationToken)
        {
            try
            {
                var sessionConnection = await sessionManager.Connect(endPoint, "chains-providing");
                var fetcherSession = new FetcherSession(sessionConnection, sourceChain, Request, chainBranchBlocksCount);
                var fetchResult = await fetcherSession.StartDonwloadingProccess(cancellationToken);
                chainProvidersRating.ChainFetchSuccesful(endPoint);
                ChainFetched?.Invoke(fetchResult);
                return true;
            }
            catch (ChainDownloadingException ex)
            {
                chainProvidersRating.ChainFetchSuccesful(endPoint);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Unhandled exception fetching chain: {ex.Message}");
            }
            return false;
        }
    }
}
