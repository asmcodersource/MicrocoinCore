using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Microcoin.ChainFetcher
{
    /// <summary>
    /// Since some chain providers can make mistakes too often, it will be correct to keep track of the rating and turn to the best ones.
    /// This class represents a repository of ratings of providers.
    /// </summary>
    public class ChainProvidersRating
    {
        Dictionary<string, double> Ratings { get; set; } = new Dictionary<string, double>();

        public void ChainFetchSuccesful(string providerPublicKey)
        {
            lock (this)
            {
                Ratings[providerPublicKey]++;
            }
        }

        public void ChainFetchFailed(string providerPublicKey)
        {
            lock (this)
            {
                Ratings[providerPublicKey]--;
            }
        }

        public ICollection<string> GetRatingSortedProviders(ICollection<string> providers)
        {
            lock (this)
            {
                foreach (var provider in providers)
                    Ratings.TryAdd(provider, 0);
                return Ratings
                    .Where(p => providers.Contains(p.Key))
                    .OrderByDescending(p => p.Value)
                    .Select(p => p.Key)
                    .ToList();
            }
        }
    }
}
