using System;
using System.Collections.Generic;
using System.Threading;

namespace Microcoin.Microcoin.Blockchain.TransactionsPool
{
    public class TransactionsBag
    {
        public Action<List<Transaction.Transaction>>? OnTransactionsBagReady;

        private Timer? sendTimer;
        private List<Transaction.Transaction> transactions = new List<Transaction.Transaction>();
        private readonly object lockObj = new object();

        public void AddTransaction(Transaction.Transaction transaction)
        {
            lock (lockObj)
            {
                transactions.Add(transaction);
                if (sendTimer is null)
                {
                    sendTimer = new Timer((_) => ReadyBag(), null, 15 * 1000, Timeout.Infinite);
                }
            }
        }

        private void ReadyBag()
        {
            List<Transaction.Transaction> transactionsToSend;
            lock (lockObj)
            {
                transactionsToSend = new List<Transaction.Transaction>(transactions);
                transactions.Clear();
                sendTimer?.Dispose();
                sendTimer = null;
            }

            OnTransactionsBagReady?.Invoke(transactionsToSend);
        }
    }
}
