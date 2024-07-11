using Microcoin.Microcoin.Blockchain.Block;
using System.Collections.Immutable;

namespace Microcoin.Microcoin.Blockchain.Chain
{
    /// <summary>
    /// The chain is a sequence of blocks for which the connection rule is executed. 
    /// A chain can be connected to the end of another chain. 
    /// A chain acting as a parent for another must be immutable
    /// </summary>
    [Serializable]
    public abstract class AbstractChain
    {
        public ImmutableChain? PreviousChain { get; protected set; }
        public IReadOnlyDictionary<string, double> WalletsCoins { get; protected set; }
        public IReadOnlyCollection<Transaction.Transaction> TransactionsSet { get; protected set; }
        public IReadOnlyDictionary<string, Microcoin.Blockchain.Block.Block> BlocksDictionary { get; protected set; }
        public IReadOnlyList<Microcoin.Blockchain.Block.Block> BlocksList { get; protected set; }
        public int EntireChainLength { get; protected set; } = 0;


        public IReadOnlyList<Microcoin.Blockchain.Block.Block> GetBlocksList()
            => BlocksList;

        public bool IsChainHasTransaction(Transaction.Transaction transaction)
            => TransactionsSet.Contains(transaction);

        public double GetWalletCoins(string walletPublicKey)
            => WalletsCoins.ContainsKey(walletPublicKey) ? WalletsCoins[walletPublicKey] : 0;

        public Microcoin.Blockchain.Block.Block? GetLastBlock()
            => GetBlockFromTail(0);

        public Microcoin.Blockchain.Block.Block? GetBlockFromTail(int blockIdFromTail)
        {
            if (blockIdFromTail < BlocksList.Count)
                return BlocksList[(BlocksList.Count - blockIdFromTail) - 1];
            else if (PreviousChain is not null)
                return PreviousChain.GetBlockFromTail(blockIdFromTail - BlocksList.Count);
            return null;
        }

        public Microcoin.Blockchain.Block.Block? GetBlockFromHead(int blockIdFromHead)
        {
            if( blockIdFromHead < EntireChainLength && blockIdFromHead >= EntireChainLength - BlocksList.Count())
                return BlocksList[blockIdFromHead - (EntireChainLength - BlocksList.Count())];
            else if ( PreviousChain is not null )
                return PreviousChain.GetBlockFromHead(blockIdFromHead);
            else
                return null;
        }
    }
}
