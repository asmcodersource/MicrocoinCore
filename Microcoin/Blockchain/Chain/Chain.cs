using Microcoin.Blockchain.Block;

namespace Microcoin.Blockchain.Chain
{
    internal class Chain : IChain
    {
        public HashSet<Transaction.Transaction> TransactionsSet { get; protected set; } = new HashSet<Transaction.Transaction>();
        public Dictionary<string, decimal> WalletsCoins { get; protected set; } = new Dictionary<string, decimal>();
        public ImmutableChain? PreviousChain { get; protected set; } = null;
        public List<Block.Block> BlocksList { get; protected set; } = new List<Block.Block>();
        public Dictionary<string, Block.Block> BlocksDictionary { get; protected set; } = new Dictionary<string, Block.Block>();

        public void AddTailBlock(Block.Block block)
        {
            lock (this)
            {
                BlocksList.Add(block);
                BlocksDictionary.Add(block.Hash, block);
                CountTransactionsTransfers(block.Transactions);
            }
        }

        protected void CountTransactionsTransfers(List<Transaction.Transaction> transactions)
        {
            foreach(Transaction.Transaction transaction in transactions)
            {
                // CAUTION, its not only count transaction transfers
                // I am also add transaction to HashSet
                TransactionsSet.Add(transaction);
                // and then count?... Should I separate this logic? Nah
                if ( WalletsCoins.TryAdd(transaction.SenderPublicKey, -transaction.TransferAmount) is not true )
                    WalletsCoins[transaction.SenderPublicKey] -= transaction.TransferAmount;
                if (WalletsCoins.TryAdd(transaction.ReceiverPublicKey, transaction.TransferAmount) is not true )
                    WalletsCoins[transaction.ReceiverPublicKey] += transaction.TransferAmount;
            }
        }
    }
}
