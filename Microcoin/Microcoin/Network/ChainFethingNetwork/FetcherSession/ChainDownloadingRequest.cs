using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin.Blockchain.Chain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Microcoin.Network.ChainFethingNetwork.FetcherSession
{
    public class ChainDownloadingRequest
    {
        public static async Task<AbstractChain> CreateRequestTask(FetcherSession fetcherSession, AbstractChain startingChain, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
