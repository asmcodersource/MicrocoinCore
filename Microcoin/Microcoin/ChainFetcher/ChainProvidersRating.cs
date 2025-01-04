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
        ConcurrentDictionary<ICommunicationEndPoint, double> Ratings { get; set; } = new();

        public void ChainFetchSuccesful(ICommunicationEndPoint providerSession)
        {
            Ratings[providerSession]++;
        }

        public void ChainFetchFailed(ICommunicationEndPoint providerSession)
        {
            Ratings[providerSession]--;
        }

        public IEnumerable<ICommunicationEndPoint> GetRatingSortedProviders(IEnumerable<ICommunicationEndPoint> providerSessions)
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
