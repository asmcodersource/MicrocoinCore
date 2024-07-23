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
            Blockchain.Block.Block tailBlock = chain.GetLastBlock();
            int chainComplexity = tailBlock is not null ? tailBlock.MiningBlockInfo.ChainComplexity : 0;
            var immutableTransactionsBlock = new Blockchain.Block.ImmutableTransactionsBlock(block);
            block.MiningBlockInfo.Complexity = MiningRules.ComplexityRule.Calculate(chain, block);
            block.MiningBlockInfo.MinerReward = MiningRules.RewardRule.Calculate(chain, block); ;
            block.MiningBlockInfo.ChainComplexity = block.MiningBlockInfo.Complexity + chainComplexity;
            block.MiningBlockInfo.MinerPublicKey = minerWallet;
            Serilog.Log.Debug($"Microcoin peer | Mining started block={block.GetHashCode()} id={block.MiningBlockInfo.BlockId} complexity={block.MiningBlockInfo.Complexity}");
            Random random = new Random();
            string hash = null;
            do
            {
                block.MiningBlockInfo.MinedValue = random.NextInt64();
                immutableTransactionsBlock.ChangeMiningBlockInfo(block.MiningBlockInfo);
                hash = immutableTransactionsBlock.CalculateMiningBlockHash();
            } while (Blockchain.Block.Block.GetHashComplexity(hash) < block.MiningBlockInfo.Complexity && cancellationToken.IsCancellationRequested is not true);
            return hash;
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
