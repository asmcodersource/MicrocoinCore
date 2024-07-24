using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin.Blockchain.Chain;

namespace Microcoin.Microcoin.Mining
{
    public interface IRewardRule
    {
        public bool Verify(AbstractChain contextChain, Block block);
        public double Calculate(AbstractChain contextChain, Block block);
    }
}
