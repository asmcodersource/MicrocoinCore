using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Blockchain.Chain
{
    /// <summary>
    /// The chain is a sequence of blocks for which the connection rule is executed. 
    /// A chain can be connected to the end of another chain. 
    /// A chain acting as a parent for another must be immutable
    /// </summary>
    internal interface IChain
    {
    }
}
