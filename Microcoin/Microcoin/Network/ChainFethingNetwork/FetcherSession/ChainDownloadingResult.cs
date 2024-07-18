using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin.Blockchain.Chain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
