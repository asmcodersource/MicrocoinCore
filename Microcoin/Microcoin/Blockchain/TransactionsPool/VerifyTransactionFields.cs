﻿using Microcoin.Microcoin.PipelineHandling;

namespace Microcoin.Microcoin.Blockchain.TransactionsPool
{
    public class VerifyTransactionFields : IPipelineHandler<Transaction.Transaction>
    {
        public bool Handle(Transaction.Transaction transaction)
        {
            if (transaction.TransferAmount <= 0) // Someone may try to transfer negative or zero count of coins ;) 
                return false;
            if (transaction.DateTime > DateTime.UtcNow)
                return false;
            if (transaction.SenderPublicKey == transaction.ReceiverPublicKey)  // Transactions between same wallet is invalid
                return false;
            return true;
        }
    }
}
