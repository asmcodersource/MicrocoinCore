using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Blockchain.Chain
{
    internal class NextBlockRule : INextBlockRule
    {
        public bool IsBlockNextToChain(Block.Block block, IChain chain)
        {
            throw new NotImplementedException();
        }
    }
}
