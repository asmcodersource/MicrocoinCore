using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin.Blockchain.Block;

namespace Microcoin.Microcoin.Mining
{
    /// <summary>
    /// This rule determines the difficulty of the next block. 
    /// The complexity of the block determines the number of calculations that the network must perform to mine the block. 
    /// The idea was taken from the Bitcoin network. Every 'avgWindow' blocks, a difficulty check is performed; 
    /// if mining lasts more than 'targetTime' minutes, the difficulty decreases; 
    /// if it takes less, it increases.
    /// </summary>
    public class ComplexityRule : IComplexityRule
    {
        protected Dictionary<ComplexityWindowIdentifier, int> complexityWindowCache = new Dictionary<ComplexityWindowIdentifier, int>();
        protected int defaultComplexity =  24;
        protected int targetTime = 10;
        protected int avgWindow = 20;

        public int Calculate(AbstractChain contextChain, Block block)
        {
            if( contextChain.EntireChainLength < avgWindow )
                return defaultComplexity;

            int windowBeginBlockId = (int)(contextChain.EntireChainLength / avgWindow) * avgWindow - avgWindow;
            Block windowFirstBlock = contextChain.GetBlockFromHead(windowBeginBlockId);
            Block windowLastBlock = contextChain.GetBlockFromHead(windowBeginBlockId + avgWindow - 1);
            var complexityWindowIdentifier = new ComplexityWindowIdentifier(windowFirstBlock, windowLastBlock);
            if (complexityWindowCache.ContainsKey(complexityWindowIdentifier))
                return complexityWindowCache[complexityWindowIdentifier];
            double averageHashRatePerSeconds = 0;
            for( int i = 0; i < avgWindow-1; i++ )
            {
                var windowCurrentBlock = contextChain.GetBlockFromHead(windowBeginBlockId + i);
                var windowNextBlock = contextChain.GetBlockFromHead(windowBeginBlockId + i + 1);
                var windowBlockAvgHashesToMine = Complexity.GetAverageIterationsToMine(windowCurrentBlock.MiningBlockInfo.Complexity);
                var windowBlockMiningTime = windowNextBlock.MiningBlockInfo.CreateTime - windowCurrentBlock.MiningBlockInfo.CreateTime;
                var windowBlockPredictedHashRate = (double)windowBlockAvgHashesToMine / Math.Abs(windowBlockMiningTime.TotalSeconds);
                averageHashRatePerSeconds += windowBlockPredictedHashRate;
            }
            averageHashRatePerSeconds = averageHashRatePerSeconds / (avgWindow - 1);
            var averageHashRatePerTargetTime = averageHashRatePerSeconds * 60 * targetTime;
            var complexity = Complexity.GetClosestComplexity(averageHashRatePerTargetTime);
            complexityWindowCache.Add(complexityWindowIdentifier, complexity);
            return complexity;
        }

        public bool Verify(AbstractChain contextChain, Block block)
        {
            var complexity = Calculate(contextChain, block);
            if (block.MiningBlockInfo.Complexity < complexity)
                return false;
            return true;
        }
    }

    public class ComplexityWindowIdentifier
    {
        public Block FirstBlock { get; protected set; }
        public Block LastBlock { get; protected set; }

        public ComplexityWindowIdentifier(Block firstBlock, Block lastBlock) 
        {
            this.FirstBlock = firstBlock;
            this.LastBlock = lastBlock;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FirstBlock, LastBlock);
        }

        public override bool Equals(object? obj)
        {
            if (base.Equals(obj) is true)
                return true;
            if( obj is ComplexityWindowIdentifier rangeIdentifier)
            {
                if (this.FirstBlock == rangeIdentifier.FirstBlock && this.LastBlock == rangeIdentifier.LastBlock)
                    return true;
            }
            return false;
        }
    }
}
