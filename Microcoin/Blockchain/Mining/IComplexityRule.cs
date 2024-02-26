using Microcoin.Blockchain.Chain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Blockchain.Mining
{
    public interface IComplexityRule
    {
        public Task<bool> Verify(IChain contextChain, Block.Block block);
        public decimal Calculate(IChain contextChain, Block.Block block);
    }
}
