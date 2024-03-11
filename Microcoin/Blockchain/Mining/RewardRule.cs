using Microcoin.Blockchain.Chain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Blockchain.Mining
{
    public class RewardRule : IRewardRule
    {
        public decimal Calculate(AbstractChain contextChain, Block.Block block)
        {
            return CalculateRewardOfBlock(block);
        }

        public bool Verify(AbstractChain contextChain, Block.Block block)
        {
            decimal reward = CalculateRewardOfBlock(block);
            if (block.MiningBlockInfo.MinerReward != reward)
                return false;
            return true;
        }

        protected decimal CalculateRewardOfBlock(Block.Block block)
        {
            return Convert.ToDecimal(block.MiningBlockInfo.Complexity * (1.0 / Convert.ToDouble(block.MiningBlockInfo.BlockId + 1)));
        }
    }
}
