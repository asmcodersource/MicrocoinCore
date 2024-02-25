using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microcoin.PipelineHandling;

namespace Microcoin.Blockchain.TransactionsPool
{
    internal class VerifyTransactionFields : IPipelineHandler<Transaction.Transaction>
    {
        public async Task<bool> Handle(Transaction.Transaction transaction)
        {
            if (transaction.TransferAmount <= 0) // Someone may try to transfer negative or zero count of coins ;) 
                return false;
            if (transaction.DateTime > DateTime.UtcNow )
                return false;
            if (transaction.SenderPublicKey == transaction.ReceiverPublicKey)  // Transactions between same wallet is invalid
                return false;
            return true;
        }
    }
}
