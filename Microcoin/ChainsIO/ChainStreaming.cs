using Microcoin.Blockchain.Chain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.ChainsIO
{
    public class ChainStreaming
    {
        public async Task<AbstractChain> ReadChainFromStream(Stream stream, CancellationToken cancellationToken)
        {
            return null;
        }

        public async Task<bool> WriteChainToStream(Stream stream, AbstractChain chain, CancellationToken cancellationToken) 
        {
            return false;
        }
    }
}
