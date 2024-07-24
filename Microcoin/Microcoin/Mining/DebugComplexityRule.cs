using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin.Blockchain.Chain;

namespace Microcoin.Microcoin.Mining
{
    // Used to reduce mining time in debug cases
    public class DebugComplexityRule : IComplexityRule
    {
        public int FixedComplexity { get; set; } = 1;

        public int Calculate(AbstractChain contextChain, Block block)
        {
            return FixedComplexity;
        }

        public bool Verify(AbstractChain contextChain, Block block)
        {
            var complexity = Calculate(contextChain, block);
            if (block.MiningBlockInfo.Complexity < complexity)
                return false;
            return true;
        }
    }
}
