using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Blockchain.ChainController
{
    internal class BasicFetchableChainRule : IFetchableChainRule
    {
        bool IFetchableChainRule.IsPossibleChainUpgrade(Block.Block block)
        {
            throw new NotImplementedException();
        }
    }
}
