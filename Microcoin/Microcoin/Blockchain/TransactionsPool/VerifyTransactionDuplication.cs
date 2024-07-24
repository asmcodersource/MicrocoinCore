﻿using Microcoin.Microcoin.PipelineHandling;

namespace Microcoin.Microcoin.Blockchain.TransactionsPool
{
    // Remember: I don't need this verify when receiving trasaction from network
    // because I have condition check before adding new value to list
    // but I need this because something can change from receiving one or multiple block from network
    public class VerifyTransactionDuplication : IPipelineHandler<Transaction.Transaction>
    {
        public bool Handle(Transaction.Transaction transaction)
        {
            return true;
        }
    }
}
