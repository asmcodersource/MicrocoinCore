using Microcoin.PipelineHandling;
using Microcoin.Microcoin.Blockchain.TransactionsPool;

namespace Microcoin.Microcoin.Blockchain.TransactionsPool
{
    public class TransactionsPool
    {
        public event Action<TransactionsPool>? OnTransactionReceived;
        public TransactionsBag TransactionsBag { get; protected set; } = new TransactionsBag();
        public List<Transaction.Transaction> Pool { get; protected set; } = new List<Transaction.Transaction>();
        public HashSet<Transaction.Transaction> PresentedTransactions { get; protected set; } = new HashSet<Transaction.Transaction>();
        public IHandlePipeline<Transaction.Transaction> HandlePipeline { get; set; } = new EmptyPipeline<Transaction.Transaction>();

        public TransactionsPool()
        {
            HandlePipeline = new HandlePipeline<Transaction.Transaction>();
            HandlePipeline.AddHandlerToPipeline(new VerifyTransactionSign());
            HandlePipeline.AddHandlerToPipeline(new VerifyTransactionFields());
            HandlePipeline.AddHandlerToPipeline(new VerifyTransactionDuplication());
        }

        public List<Transaction.Transaction> TakeTransactions(int countToTake)
        {
            var transactions = Pool.Take(countToTake).ToList();
            return transactions;
        }

        public void RemoveTransactions(List<Transaction.Transaction> removeTransactions)
        {
            // Some transaction can be expired after some events
            // ( for example being include in some block already )
            // Performed without lock, resposibility lies on controll code
            foreach (var transaction in removeTransactions)
                RemoveTransaction(transaction);
        }

        public bool HandleTransaction(Transaction.Transaction transaction)
        {
            // Handle transaction on verifing pipeline
            var handleResult = HandlePipeline.Handle(transaction);
            if (handleResult.IsHandleSuccesful is not true)
                return false;
            // If transaction succesfully pass pipeline, add it to pool
            AddTransaction(transaction);
            Serilog.Log.Verbose($"Microcoin peer | Transaction({transaction.GetHashCode()}) succesfully passed handle pipeline");
            return true;
        }

        public void HandleTransactions(List<Transaction.Transaction> transactions)
        {
            foreach(var transaction in transactions)
                HandleTransaction(transaction);
        }

        protected void AddTransaction(Transaction.Transaction transaction)
        {
            lock (this)
            {
                if (PresentedTransactions.Contains(transaction))
                    return;
                Pool.Add(transaction);
                PresentedTransactions.Add(transaction);
                TransactionsBag.AddTransaction(transaction);
            }
            OnTransactionReceived?.Invoke(this);
        }

        protected void RemoveTransaction(Transaction.Transaction transaction)
        {
            lock (this)
            {
                PresentedTransactions.Remove(transaction);
                Pool.Remove(transaction);
            }
        }
    }
}
