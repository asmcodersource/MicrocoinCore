﻿using Microcoin.Microcoin.Network;

namespace Microcoin.Microcoin.ChainFetcher
{
    public interface IChainProvidersRating
    {
        public void ChainFetchSuccesful(ICommunicationEndPoint providerSession);
        public void ChainFetchFailed(ICommunicationEndPoint providerSession);
        public IEnumerable<ICommunicationEndPoint> GetRatingSortedProviders(IEnumerable<ICommunicationEndPoint> providerSessions);
    }
}
