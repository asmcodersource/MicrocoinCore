using Microcoin.Microcoin.Blockchain.Chain;

namespace Microcoin.Microcoin.ChainStorage
{
    class ChainStorage
    {
        public string WorkingDirectory { get; protected set; } = "ChainsStorage";


        /// <summary>Suitable for restoring state after peer shutdown</summary>
        /// <returns>Tail chain object with linked pre-chains</returns>
        public ChainContext LoadMostComprehensiveChain()
        {
            throw new NotImplementedException();
        }

        public ChainContext LoadChain(ChainIdentifier chainIdentifier)
        {
            throw new NotImplementedException();
        }

        public void StoreChain(AbstractChain chain)
        {
            throw new NotImplementedException();
        }
    }
}
