using Microcoin.Blockchain.Chain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Blockchain.Mining
{
    public class Miner : IMiner
    {
        public MiningRules MiningRules { get; protected set; }

        public event Action<Block.Block> BlockMined;

        public async Task StartBlockMining(IChain chain, Block.Block block, CancellationToken cancellationToken)
        {
            decimal miningReward = MiningRules.RewardRule.Calculate(chain, block);
            double miningComplexiy = MiningRules.ComplexityRule.Calculate(chain, block);

        }

        public async Task<bool> VerifyBlockMining(IChain chain, Block.Block block)
        {
            var isBlockRewardCorrect = await MiningRules.RewardRule.Verify(chain, block);
            if (isBlockRewardCorrect is not true)
                return false;
            var isBlockComplexityCorrect = await MiningRules.ComplexityRule.Verify(chain, block);
            if (isBlockComplexityCorrect is not true)
                return false;

            decimal miningReward = block.MiningBlockInfo.MinerReward;
            double miningComplexiy = block.MiningBlockInfo.Complexity;
            return true;
        }

        protected async Task<string> MineBlockHash()
        {
            // TODO: just work...
        }

        protected bool VerifyBlockHash()
        {
            return true;
        }
    }
}
