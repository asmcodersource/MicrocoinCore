using Microcoin.Blockchain.Chain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Blockchain.Mining
{
    public class ComplexityRule : IComplexityRule
    {
        public double Calculate(IChain contextChain, Block.Block block)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Verify(IChain contextChain, Block.Block block)
        {
            throw new NotImplementedException();
        }
    }
}
