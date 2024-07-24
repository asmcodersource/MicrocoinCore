using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin.Blockchain.Chain;

namespace Microcoin.Microcoin.Network.ChainFethingNetwork.FetcherSession
{
    public record ChainDownloadingResult
    {
        public MutableChain DownloadedChain;
        public Block LastBlockFromSource;

        public ChainDownloadingResult(MutableChain mutableChain, Block lastBlockFromSource)
        {
            this.DownloadedChain = mutableChain;
            this.LastBlockFromSource = lastBlockFromSource;
        }
    }
}
