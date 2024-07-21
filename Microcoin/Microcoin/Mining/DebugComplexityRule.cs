using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin.Blockchain.Block;

namespace Microcoin.Microcoin.Mining
{
    // Used to reduce mining time in debug cases
    public class DebugComplexityRule : IComplexityRule
    {
        public int FixedComplexity { get; set; } = 17;

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
