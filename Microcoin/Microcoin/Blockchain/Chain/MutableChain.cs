using Microcoin.Microcoin.Blockchain.Block;

namespace Microcoin.Microcoin.Blockchain.Chain
{
    [Serializable]
    public class MutableChain : AbstractChain, ICloneable
    {
        new public Dictionary<string, double> WalletsCoins { get; protected set; }
        new public HashSet<Transaction.Transaction> TransactionsSet { get; protected set; }
        new public Dictionary<string, Microcoin.Blockchain.Block.Block> BlocksDictionary { get; protected set; }
        new public List<Microcoin.Blockchain.Block.Block> BlocksList { get; protected set; }

        public MutableChain()
        {
            BlocksList = new List<Microcoin.Blockchain.Block.Block>();
            BlocksDictionary = new Dictionary<string, Microcoin.Blockchain.Block.Block>();
            TransactionsSet = new HashSet<Transaction.Transaction>();
            WalletsCoins = new Dictionary<string, double>();
            // I also initialize the interface of the abstract chain with references to mutable instances
            base.BlocksList = BlocksList;
            base.WalletsCoins = WalletsCoins;
            base.TransactionsSet = TransactionsSet;
            base.BlocksDictionary = BlocksDictionary;
        }

        public MutableChain(ImmutableChain immutableChain)
        {
            PreviousChain = immutableChain.PreviousChain;
            BlocksList = new List<Microcoin.Blockchain.Block.Block>(immutableChain.BlocksList);
            BlocksDictionary = new Dictionary<string, Microcoin.Blockchain.Block.Block>(immutableChain.BlocksDictionary);
            TransactionsSet = new HashSet<Transaction.Transaction>(immutableChain.TransactionsSet);
            WalletsCoins = new Dictionary<string, double>(immutableChain.WalletsCoins);
            // I also initialize the interface of the abstract chain with references to mutable instances
            base.BlocksList = BlocksList;
            base.WalletsCoins = WalletsCoins;
            base.TransactionsSet = TransactionsSet;
            base.BlocksDictionary = BlocksDictionary;
        }

        private MutableChain(Dictionary<string, double> walletsCoins, HashSet<Transaction.Transaction> transactionsSet, Dictionary<string, Block.Block> blocksDictionary, List<Block.Block> blocksList, ImmutableChain? previousChain)
        {
            PreviousChain = previousChain;
            BlocksList = new List<Microcoin.Blockchain.Block.Block>(blocksList);
            BlocksDictionary = new Dictionary<string, Microcoin.Blockchain.Block.Block>(blocksDictionary);
            TransactionsSet = new HashSet<Transaction.Transaction>(transactionsSet);
            WalletsCoins = new Dictionary<string, double>(walletsCoins);
            // I also initialize the interface of the abstract chain with references to mutable instances
            base.BlocksList = BlocksList;
            base.WalletsCoins = WalletsCoins;
            base.TransactionsSet = TransactionsSet;
            base.BlocksDictionary = BlocksDictionary;
        }

        public void AddTailBlock(Microcoin.Blockchain.Block.Block block)
        {
            lock (this)
            {
                BlocksList.Add(block);
                BlocksDictionary.Add(block.Hash, block);
                CountTransactionsTransfers(block.Transactions);
                CountMinerReward(block);
                EntireChainLength = EntireChainLength + 1;
            }
        }

        public void LinkPreviousChain(AbstractChain previousChain)
        {
            PreviousChain = new ImmutableChain(previousChain);
            EntireChainLength = BlocksList.Count() + (PreviousChain is not null ? PreviousChain.EntireChainLength : 0);
        }

        /// <summary> Use it careful, because it don't update initiale state of object </summary>
        public void SetBlockList(List<Microcoin.Blockchain.Block.Block> blockList)
        {
            BlocksList = blockList;
            EntireChainLength = BlocksList.Count() + (PreviousChain is not null ? PreviousChain.EntireChainLength : 0);
        }

        public object Clone()
        {
            return new MutableChain(WalletsCoins, TransactionsSet, BlocksDictionary, BlocksList, PreviousChain);
        }

        protected void CountMinerReward(Microcoin.Blockchain.Block.Block block)
        {
            if (WalletsCoins.TryAdd(block.MiningBlockInfo.MinerPublicKey, block.MiningBlockInfo.MinerReward) is not true)
                WalletsCoins[block.MiningBlockInfo.MinerPublicKey] += block.MiningBlockInfo.MinerReward;
        }

        protected void CountTransactionsTransfers(List<Transaction.Transaction> transactions)
        {
            foreach (Transaction.Transaction transaction in transactions)
            {
                // CAUTION, its not only count transaction transfers
                // I am also add transaction to HashSet
                TransactionsSet.Add(transaction);
                // and then count?... Should I separate this logic? Nah
                if (WalletsCoins.TryAdd(transaction.SenderPublicKey, -transaction.TransferAmount) is not true)
                    WalletsCoins[transaction.SenderPublicKey] -= transaction.TransferAmount;
                if (WalletsCoins.TryAdd(transaction.ReceiverPublicKey, transaction.TransferAmount) is not true)
                    WalletsCoins[transaction.ReceiverPublicKey] += transaction.TransferAmount;
            }
        }
    }
}
