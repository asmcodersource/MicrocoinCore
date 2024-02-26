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
    public interface IChain
    {
        public Dictionary<string, decimal> WalletsCoins { get; }
        public ImmutableChain? PreviousChain { get; }
        public List<Block.Block> BlocksList { get; }
        public HashSet<Transaction.Transaction> TransactionsSet {  get; }
        public Dictionary<string, Block.Block> BlocksDictionary { get; }

        public bool IsChainHasTransaction(Transaction.Transaction transaction)
            => TransactionsSet.Contains(transaction);

        public decimal GetWalletCoins(string walletPublicKey)
            => WalletsCoins.ContainsKey(walletPublicKey) ? WalletsCoins[walletPublicKey] : 0;

        public Block.Block GetLastBlock() 
            => BlocksList.Last();

    }
}
