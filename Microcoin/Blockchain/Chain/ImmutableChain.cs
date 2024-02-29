using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Blockchain.Chain
{
    public class ImmutableChain : IChain
    {
        public HashSet<Transaction.Transaction> TransactionsSet { get; }
        public Dictionary<string, decimal> WalletsCoins { get; }
        public ImmutableChain? PreviousChain { get; }
        public List<Block.Block> BlocksList { get; }
        public Dictionary<string, Block.Block> BlocksDictionary { get; }

        public ImmutableChain(Chain chain)
        {
            this.TransactionsSet = new HashSet<Transaction.Transaction>(chain.TransactionsSet);
            this.WalletsCoins = new Dictionary<string, decimal>(chain.WalletsCoins);
            this.PreviousChain = chain.PreviousChain;
            this.BlocksList = new List<Block.Block>(chain.BlocksList);
            this.BlocksDictionary = new Dictionary<string, Block.Block>(chain.BlocksDictionary);
        }
    }
}
