using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Blockchain.Chain
{
    [Serializable]
    public class ChainHeader
    {
        ///public Dictionary<string, decimal> WalletsCoins { get; protected set; }
        public Int32 BlocksCount { get; protected set; }

        public ChainHeader()
        { }

        public ChainHeader(AbstractChain chain) 
        {
            //WalletsCoins = new Dictionary<string, decimal>(chain.WalletsCoins);
            BlocksCount = chain.GetBlocksList().Count();
        }
    }
}
