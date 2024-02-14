using Microcoin.Transaction;
using Newtonsoft.Json.Linq;
using NodeNet.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.MessageAcceptors
{
    internal class TransactionsAcceptor : IAcceptor
    {
        public event Action<Transaction.Transaction> TransactionReceived;

        public async Task Handle(MessageContext messageContext)
        {
            JObject jsonRequestObject = JObject.Parse(messageContext.Message.Data);
            JToken? jsonTransactionToken = jsonRequestObject["transaction"];
            if (jsonTransactionToken is null)
                return;
            string transactionJsonString = jsonTransactionToken.ToString();
            Transaction.Transaction? transaction = Transaction.Transaction.ParseTransactionFromJson(transactionJsonString);
            if( transaction != null ) 
                TransactionReceived?.Invoke(transaction);
        }
    }
}
