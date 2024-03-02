using Microcoin.Blockchain.Chain;


namespace Microcoin.Blockchain.Mining
{
    public class Miner : IMiner
    {
        public MiningRules MiningRules { get; protected set; }

        public event Action<Block.Block> BlockMined;

        public async Task StartBlockMining(IChain chain, Block.Block block, string minerWallet, CancellationToken cancellationToken)
        {
            bool isBlockAlreadyMined = false;
            block.MiningBlockInfo.MinerPublicKey = minerWallet;
            while (cancellationToken.IsCancellationRequested is not true && isBlockAlreadyMined is not true )
            {
                decimal miningReward = MiningRules.RewardRule.Calculate(chain, block);
                int miningComplexiy = MiningRules.ComplexityRule.Calculate(chain, block);
                block.MiningBlockInfo.MinerReward = miningReward;
                block.MiningBlockInfo.Complexity = miningComplexiy;
                // To reduce count of complexity and reward recalculations
                for (int i = 0; i < 1024 * 16; i++)
                {
                    block.MiningBlockInfo.MinedValue = Random.Shared.NextInt64();
                    var hash = block.GetMiningBlockHash();
                    var isHashCorrect = MiningRules.ComplexityRule.Verify(chain, block);
                    if (isHashCorrect is not true)
                        continue;
                    lock (this)
                    {
                        if (isBlockAlreadyMined is true)
                            break;
                        isBlockAlreadyMined = true;
                        cancellationToken.ThrowIfCancellationRequested();
                        block.Hash = hash;
                        BlockMined.Invoke(block);
                    }
                }
            }
        }

        public bool VerifyBlockMining(IChain chain, Block.Block block)
        {
            var isBlockRewardCorrect = MiningRules.RewardRule.Verify(chain, block);
            if (isBlockRewardCorrect is not true)
                return false;
            var isBlockComplexityCorrect = MiningRules.ComplexityRule.Verify(chain, block);
            if (isBlockComplexityCorrect is not true)
                return false;
            var computedHash = block.GetMiningBlockHash();
            if( computedHash != block.Hash)
                return false;
            return true;
        }
    }
}
