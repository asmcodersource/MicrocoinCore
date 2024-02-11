using Microcoin.CommunicationSessions;
using Microcoin.PipelineHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.TransactionPool
{
    internal class TransactionsPool
    {
        public event Action<TransactionsPool> OnTransactionReceived;
        public List<ITransaction> Pool { get; protected set; } = new List<ITransaction>();
        public IHandlePipeline<ITransaction> HandlePipeline { get; set; } = new EmptyPipeline<ITransaction>();

        public List<ITransaction> TakeTransactions()
        {
            var transactions = Pool.ToList();
            return transactions;
        }

        public void RemoveTransactions(List<ITransaction> removeTransactions)
        {
            // Some transaction can be expired after some events
            // ( for example being include in some block already )
            // Performed without lock, resposibility lies on controll code
            foreach (var transaction in removeTransactions)
                RemoveTransaction(transaction);
        }

        public async Task HandleTransactionMessage(IMessage message)
        {
            // TODO:
            // parse message to ITransaction object
            ITransaction transaction = null;
            var handleResult = await Task.Run(() => HandlePipeline.Handle(transaction));
            if (handleResult.IsHandleSuccesful is not true)
                return;
            AddTransaction(transaction);
        }

        protected void InitializeHandlerPipeline()
        {
            HandlePipeline = new HandlePipeline<ITransaction>();
            HandlePipeline.AddHandlerToPipeline(new VerifyTransactionSign());
            HandlePipeline.AddHandlerToPipeline(new VerifyTransactionFields());
            HandlePipeline.AddHandlerToPipeline(new VerifyTransactionDuplication());
        }

        protected void AddTransaction(ITransaction transaction)
        {
            Pool.Add(transaction);
            OnTransactionReceived?.Invoke(this);
        }

        protected void RemoveTransaction(ITransaction transaction)
        {
            Pool.Remove(transaction);
        }
    }
}
