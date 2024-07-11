using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin.Blockchain.Chain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Microcoin.Network.ChainFethingNetwork.FetcherSession
{
    public class ChainBlockPresentRequest
    {
        public static async Task<bool> CreateRequestTask(FetcherSession fetcherSession, AbstractChain abstractChain, Block firstBlock, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
