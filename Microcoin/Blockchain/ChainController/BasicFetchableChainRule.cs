using Microcoin.Blockchain.Chain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Blockchain.ChainController
{
    internal class BasicFetchableChainRule : IFetchableChainRule
    {
        public bool IsPossibleChainUpgrade(IChain chain, Block.Block block)
        {
            // The simplest implementation without additional checks.
            // If a chain from the network is N blocks longer than the local one, then it is a candidate for replacing the current chain.
            const int N = 10;
            if( (chain.GetLastBlock().MiningBlockInfo.BlockId + N) >= block.MiningBlockInfo.BlockId )
                return false;
            return true;
        }
    }
}
