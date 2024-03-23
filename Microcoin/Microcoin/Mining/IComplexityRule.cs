using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin.Blockchain.Block;

namespace Microcoin.Microcoin.Mining
{
    public interface IComplexityRule
    {
        public bool Verify(AbstractChain contextChain, Block block);
        public int Calculate(AbstractChain contextChain, Block block);
    }
}
