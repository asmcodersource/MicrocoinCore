using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microcoin.PipelineHandling;
using Microcoin.Transaction;
using Microcoin.TransactionPool;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microcoin.TransactionPool
{
    internal class TransactionsPool
    {
        public event Action<TransactionsPool> OnTransactionReceived;
        public List<ITransaction> Pool { get; protected set; } = new List<ITransaction>();
        public HashSet<ITransaction> PresentedTransactions { get; protected set; } = new HashSet<ITransaction>();
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

        public async Task HandleTransactionMessage(NodeNet.Message.MessageContext messageContext)
        {
            // Parsing transaction json to transaction object
            JObject jsonRequestObject = JObject.Parse(messageContext.Message.Data);
            JToken? jsonTransactionToken = jsonRequestObject["transaction"];
            if (jsonTransactionToken is null)
                return;
            string transactionJsonString = jsonTransactionToken.ToString();
            ITransaction? transaction = Transaction.Transaction.ParseTransactionFromJson(transactionJsonString);
            if (transaction == null)
                return;            
            // Handle transaction on verifing pipeline
            var handleResult = await Task.Run(() => HandlePipeline.Handle(transaction));
            if (handleResult.IsHandleSuccesful is not true)
                return;
            // If transaction succesfully pass pipeline, add it to pool
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
            lock (this)
            {
                if (PresentedTransactions.Contains(transaction))
                    return;
                Pool.Add(transaction);
                PresentedTransactions.Add(transaction);
                OnTransactionReceived?.Invoke(this);
            }
        }

        protected void RemoveTransaction(ITransaction transaction)
        {
            lock (this)
            {
                PresentedTransactions.Remove(transaction);
                Pool.Remove(transaction);
            }
        }
    }
}
