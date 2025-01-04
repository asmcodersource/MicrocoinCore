namespace Microcoin.Microcoin.Blockchain.TransactionsPool
{
    public class TransactionsBag
    {
        public readonly int MaxTransactionsPerBag;
        private List<Transaction.Transaction> transactions = new List<Transaction.Transaction>();
        private readonly object lockObj = new object();
        private Timer? sendTimer;
        public Action<List<Transaction.Transaction>>? OnTransactionsBagReady;


        public TransactionsBag(int maxTransactionsPerBag)
        {
            MaxTransactionsPerBag = maxTransactionsPerBag;
        }

        public void AddTransaction(Transaction.Transaction transaction)
        {
            lock (lockObj)
            {
                transactions.Add(transaction);
                if (transactions.Count > MaxTransactionsPerBag)
                    SendBag();
                else if (sendTimer is null)
                    sendTimer = new Timer((_) => ReadyBag(), null, 15 * 1000, Timeout.Infinite);
            }
        }

        private void ReadyBag()
        {
            SendBag();
            transactions.Clear();
            sendTimer?.Dispose();
            sendTimer = null;
        }

        private void SendBag()
        {
            List<Transaction.Transaction> transactionsToSend;
            lock (lockObj)
            {
                transactionsToSend = new List<Transaction.Transaction>(transactions);
                transactions = new List<Transaction.Transaction>();
                transactions.Clear();
                sendTimer?.Dispose();
                sendTimer = null;
            }
            Task.Run(() => OnTransactionsBagReady?.Invoke(transactionsToSend));
        }
    }
}
