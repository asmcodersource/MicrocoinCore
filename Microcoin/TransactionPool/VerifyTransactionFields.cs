using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microcoin.PipelineHandling;
using Microcoin.Transaction;

namespace Microcoin.TransactionPool
{
    internal class VerifyTransactionFields : IPipelineHandler<ITransaction>
    {
        public async Task<bool> Handle(ITransaction transaction)
        {
            if (transaction.TransferAmount <= 0) // Someone may try to transfer negative or zero count of coins ;) 
                return false;
            if( transaction.SenderPublicKey == transaction.ReceiverPublicKey)  // Transactions between same wallet is invalid
                return false;
            return true;
        }
    }
}
