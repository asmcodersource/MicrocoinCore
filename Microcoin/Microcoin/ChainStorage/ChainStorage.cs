using Chain;

namespace Microcoin.ChainStorage
{
    class ChainStorage
    {
        public string WorkingDirectory { get; protected set; } = "ChainsStorage";

        /// <summary>Suitable for restoring state after peer shutdown</summary>
        /// <returns>Tail chain object with linked pre-chains</returns>
        /// <exception cref="NotImplementedException"></exception>
        public Chain.Chain LoadMostComprehensiveChain()
        {
            throw new NotImplementedException();
        }

        public Chain.Chain LoadChain()
        {
            throw new NotImplementedException();
        }

        public void StoreChain(AbstractChain chain)
        {
            throw new NotImplementedException();
        }
    }
}
