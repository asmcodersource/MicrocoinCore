using Microcoin.Microcoin.Network;
using System.Collections.Concurrent;

namespace Microcoin.Microcoin.ChainFetcher
{
    /// <summary>
    /// Since some chain providers can make mistakes too often, it will be correct to keep track of the rating and turn to the best ones.
    /// This class represents a repository of ratings of providers.
    /// </summary>
    public class ChainProvidersRating : IChainProvidersRating
    {
        ConcurrentDictionary<CommunicationEndPoint, double> Ratings { get; set; } = new();

        public void ChainFetchSuccesful(CommunicationEndPoint providerSession)
        {
            Ratings[providerSession]++;
        }

        public void ChainFetchFailed(CommunicationEndPoint providerSession)
        {
            Ratings[providerSession]--;
        }

        public IEnumerable<CommunicationEndPoint> GetRatingSortedProviders(IEnumerable<CommunicationEndPoint> providerSessions)
        {
            foreach (var provider in providerSessions)
                Ratings.TryAdd(provider, 0);
            return Ratings
                .Where(p => providerSessions.Contains(p.Key))
                .OrderByDescending(p => p.Value)
                .Select(p => p.Key)
                .ToList();
        }
    }
}
