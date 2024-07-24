using Microcoin.Microcoin.Network;

namespace Microcoin.Microcoin.ChainFetcher
{
    internal interface IChainProvidersRating
    {
        public void ChainFetchSuccesful(CommunicationEndPoint providerSession);
        public void ChainFetchFailed(CommunicationEndPoint providerSession);
        public IEnumerable<CommunicationEndPoint> GetRatingSortedProviders(IEnumerable<CommunicationEndPoint> providerSessions);
    }
}
