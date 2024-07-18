using Microcoin.Microcoin.Blockchain.Chain;

namespace Microcoin.Microcoin.Mining
{
    public class Miner : IMiner
    {
        public MiningRules MiningRules { get; protected set; }

        public event Action<Blockchain.Block.Block, string> BlockMined;

        public void SetRules(MiningRules rules)
        {
            MiningRules = rules;
        }

        /// <summary>
        /// This is single-threaded, processor-based, not fast code.Since the project is educational, 
        /// this implementation will most likely be left, it shows a general example of how the blockchain works, 
        /// but provides an obvious way to perform a 50.1% network power attack.
        /// A person who implements a multi-threaded fast code will get a higher speed of hash generation and verification, 
        /// which will give him a huge increase in power against the background of other network members.
        /// Well, let's assume that everyone uses this version of the miner.
        /// </summary>
        public async Task<string> StartBlockMining(AbstractChain chain, Blockchain.Block.Block block, string minerWallet, CancellationToken cancellationToken)
        {
            Log.Debug($"Microcoin peer | Block({block.GetHashCode()}) mining started, block id = {block.MiningBlockInfo.BlockId}, complexity = {MiningRules.ComplexityRule.Calculate(chain, block)}");
            DateTime beginTime = DateTime.UtcNow;
            // Get chain complexity, used to calculate chain complexity for new tail block
            int chainComplexity = 0;
            Blockchain.Block.Block tailBlock = chain.GetLastBlock();
            if (tailBlock != null)
                chainComplexity = tailBlock.MiningBlockInfo.ChainComplexity;
            var immutableTransactionsBlock = new Blockchain.Block.ImmutableTransactionsBlock(block);
            bool isBlockAlreadyMined = false;
            int latest_complexity = -1;
            int hasheshCalculated = 0;
            try
            {
                // Prepare to mining
                block.MiningBlockInfo.MinerPublicKey = minerWallet;
                Random random = new Random();
                while (cancellationToken.IsCancellationRequested is not true && isBlockAlreadyMined is not true)
                {
                    // Calculate block values by rules
                    int miningComplexity = MiningRules.ComplexityRule.Calculate(chain, block);
                    block.MiningBlockInfo.Complexity = miningComplexity;
                    double miningReward = MiningRules.RewardRule.Calculate(chain, block);
                    block.MiningBlockInfo.MinerReward = miningReward;
                    block.MiningBlockInfo.ChainComplexity = miningComplexity + chainComplexity;
                    if( latest_complexity != miningComplexity)
                    {
                        Log.Verbose($"Microcoin peer | Block({block.GetHashCode()}) mining get complexity: {miningComplexity}, previous complexity: {latest_complexity}");
                        latest_complexity = miningComplexity;
                    }
                    // To reduce count of complexity and reward recalculations
                    for (int i = 0; i < 1024*16; i++)
                    {
                        hasheshCalculated++;
                        block.MiningBlockInfo.MinedValue = random.NextInt64();
                        immutableTransactionsBlock.ChangeMiningBlockInfo(block.MiningBlockInfo);
                        var hash = immutableTransactionsBlock.CalculateMiningBlockHash();
                        if (Blockchain.Block.Block.GetHashComplexity(hash) < block.MiningBlockInfo.Complexity)
                            continue;
                        lock (this)
                        {
                            if (isBlockAlreadyMined is true)
                                break;
                            isBlockAlreadyMined = true;
                            cancellationToken.ThrowIfCancellationRequested();
                            return hash;
                        }
                    }
                }
                cancellationToken.ThrowIfCancellationRequested();
                throw new Exception("Something wen't wrong");
            }
            finally
            {
                var finishTime = DateTime.UtcNow;
                if (isBlockAlreadyMined && cancellationToken.IsCancellationRequested is not true)
                {
                    Log.Debug($"Microcoin peer | Block({block.GetHashCode()})({block.GetMiningBlockHash()}) mining with complexity {block.MiningBlockInfo.Complexity} finished, after {(finishTime - beginTime).TotalSeconds} seconds");
                    BlockMined?.Invoke(block, block.Hash);
                }
                if (cancellationToken.IsCancellationRequested)
                    Log.Debug($"Microcoin peer | Block({block.GetHashCode()})({block.GetMiningBlockHash()}) mining canceled, after {(finishTime - beginTime).TotalSeconds} seconds");
            }
        }

        public bool VerifyBlockMining(AbstractChain chain, Blockchain.Block.Block block)
        {
            var isBlockRewardCorrect = MiningRules.RewardRule.Verify(chain, block);
            if (isBlockRewardCorrect is not true)
                return false;
            var isBlockComplexityCorrect = MiningRules.ComplexityRule.Verify(chain, block);
            if (isBlockComplexityCorrect is not true)
                return false;
            var computedHash = block.GetMiningBlockHash();
            if (computedHash != block.Hash)
                return false;
            if (Blockchain.Block.Block.GetHashComplexity(block.Hash) < block.MiningBlockInfo.Complexity)
                return false;
            return true;
        }
    }
}
