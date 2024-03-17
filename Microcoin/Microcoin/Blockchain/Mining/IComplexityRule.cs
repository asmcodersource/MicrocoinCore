using Microcoin.Microcoin.Blockchain.Chain;

namespace Microcoin.Microcoin.Blockchain.Mining
{
    public interface IComplexityRule
    {
        public bool Verify(AbstractChain contextChain, Block.Block block);
        public int Calculate(AbstractChain contextChain, Block.Block block);
    }
}
