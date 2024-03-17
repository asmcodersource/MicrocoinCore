using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Microcoin.ChainStorage
{
    public class ChainIdentifier
    {
        public int TailBlockId { get; protected set; }
        public int TailBlockHash { get; protected set; }
        public int ChainComplexity { get; protected set; }

        public ChainIdentifier(int tailBlockId, int tailBlockHash, int chainComplexity)
        {
            TailBlockId = tailBlockId;
            TailBlockHash = tailBlockHash;
            ChainComplexity = chainComplexity;
        }

        public override bool Equals(object? obj)
        {
            if (obj is ChainIdentifier another)
                return (TailBlockId, TailBlockHash, ChainComplexity).Equals((another.TailBlockId, another.TailBlockHash, another.ChainComplexity));
            else
                return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TailBlockId, TailBlockHash, ChainComplexity);
        }
    }
}
