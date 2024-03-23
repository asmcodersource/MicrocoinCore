using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin.Blockchain.Block;

namespace Microcoin.Microcoin.Mining
{
    public interface IRewardRule
    {
        public bool Verify(AbstractChain contextChain, Block block);
        public double Calculate(AbstractChain contextChain, Block block);
    }
}
