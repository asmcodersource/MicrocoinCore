using Microcoin.PipelineHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Blockchain.Block
{
    internal class VerifyBlockTransactions : IPipelineHandler<Block>
    {
        public Task<bool> Handle(Block block)
        {
            throw new NotImplementedException();
        }
    }
}
