﻿using Microcoin.Microcoin.Blockchain.Block;

namespace Microcoin.Microcoin.Blockchain.Chain
{
    [Serializable]
    public class Chain : AbstractChain
    {
        public Chain()
        {
            blocksList = new List<Microcoin.Blockchain.Block.Block>();
            BlocksDictionary = new Dictionary<string, Microcoin.Blockchain.Block.Block>();
            TransactionsSet = new HashSet<Transaction.Transaction>();
            WalletsCoins = new Dictionary<string, double>();
        }

        public void AddTailBlock(Microcoin.Blockchain.Block.Block block)
        {
            lock (this)
            {
                blocksList.Add(block);
                BlocksDictionary.Add(block.Hash, block);
                CountTransactionsTransfers(block.Transactions);
                CountMinerReward(block);
                ChainLength = ChainLength + 1;
            }
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
