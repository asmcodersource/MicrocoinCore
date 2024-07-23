using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin.Blockchain.Chain;

namespace Microcoin.Microcoin.Blockchain.ChainController
{
    public class FetchableChainRule : IFetchableChainRule
    {
        public bool IsPossibleChainUpgrade(AbstractChain chain, Microcoin.Blockchain.Block.Block block)
        {
            // Chain length is bad reson for swithing to another chain
            //    The most expected example of an attack in such a case is the creation of a large number of blocks
            //    with minimal complexity (improper setting of the correct block mining time).
            // 
            var currentTailBlock = chain.GetLastBlock();
            if (currentTailBlock == null)
                throw new Exception("Chain shouldn't be empty");
            int minimumComplexityToSwitch = Convert.ToInt32(currentTailBlock.MiningBlockInfo.ChainComplexity * 1.10);
            if (Math.Abs(minimumComplexityToSwitch - block.MiningBlockInfo.ChainComplexity) < 50)
                return false;
            if (minimumComplexityToSwitch < block.MiningBlockInfo.ChainComplexity)
                return true;
            else
                return false;
        }
    }
}
