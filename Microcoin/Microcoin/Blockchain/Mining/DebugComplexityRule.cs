using Microcoin.Microcoin.Blockchain.Chain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Microcoin.Blockchain.Mining
{
    // Used to reduce mining time in debug cases
    public class DebugComplexityRule : IComplexityRule
    {
        protected int FixedComplexity { get; set; } = 1;

        public int Calculate(AbstractChain contextChain, Block.Block block)
        {
            return FixedComplexity;
        }

        public bool Verify(AbstractChain contextChain, Block.Block block)
        {
            var complexity = Calculate(contextChain, block);
            if (block.MiningBlockInfo.Complexity < complexity)
                return false;
            return true;
        }
    }
}
