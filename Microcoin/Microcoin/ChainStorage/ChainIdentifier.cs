using Microcoin.Microcoin.Blockchain.Chain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Microcoin.ChainStorage
{
    public class ChainIdentifier
    {
        public int TailChainBlockCount { get; protected set; }
        public int TailBlockId { get; protected set; }
        public string TailBlockHash { get; protected set; }
        public int ChainComplexity { get; protected set; }

        public ChainIdentifier( AbstractChain chain ) 
        {
            var tailBlock = chain.GetLastBlock();
            if (tailBlock == null)
                throw new Exception("Empty chain can't be used to create ChainIdentifier");
            TailChainBlockCount = chain.GetBlocksList().Count();
            TailBlockId = tailBlock.MiningBlockInfo.BlockId;
            TailBlockHash = tailBlock.Hash;
            ChainComplexity = tailBlock.MiningBlockInfo.ChainComplexity;
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
