using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin.Blockchain.Chain;

namespace Microcoin.Microcoin.Mining
{
    public class RewardRule : IRewardRule
    {
        public double Calculate(AbstractChain contextChain, Block block)
        {
            return CalculateRewardOfBlock(block);
        }

        public bool Verify(AbstractChain contextChain, Block block)
        {
            double reward = CalculateRewardOfBlock(block);
            if (block.MiningBlockInfo.MinerReward != reward)
                return false;
            return true;
        }

        protected double CalculateRewardOfBlock(Block block)
        {
            return block.MiningBlockInfo.Complexity * (1.0 / (block.MiningBlockInfo.BlockId + 1));
        }
    }
}
