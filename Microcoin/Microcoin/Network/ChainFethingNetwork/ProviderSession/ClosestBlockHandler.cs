using Microcoin.Microcoin.Blockchain.Chain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Microcoin.Network.ChainFethingNetwork.ProviderSession
{
    public class ClosestBlockHandler
    {
        /// <summary>
        /// Creates a task that will communicate through a session, processing a request to find the nearest block serving as a common point from which to start loading the continuation of the chain.
        /// </summary>
        public static async Task CreateHandleTask(ProviderSession providerSession, AbstractChain actualChain, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
