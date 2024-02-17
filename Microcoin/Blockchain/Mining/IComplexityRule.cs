using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Blockchain.Mining
{
    internal interface IComplexityRule
    {
        public decimal CalculateComplexity(List<Blockchain.Block.Block> blocks, int blockId);
    }
}
