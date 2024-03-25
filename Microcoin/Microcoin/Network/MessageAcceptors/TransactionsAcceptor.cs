using NodeNet.NodeNet.Message;
using Microcoin.Microcoin.Blockchain.Transaction;
using Newtonsoft.Json.Linq;
using Microcoin.Microcoin.Blockchain.Block;

namespace Microcoin.Microcoin.Network.MessageAcceptors
{
    public class TransactionsAcceptor : IAcceptor
    {
        public event Action<Transaction> TransactionReceived;

        public virtual async Task Handle(MessageContext messageContext)
        {
            JObject jsonRequestObject = JObject.Parse(messageContext.Message.Data);
            JToken? jsonTransactionToken = jsonRequestObject["transaction"];
            if (jsonTransactionToken is null)
                return;
            string transactionJsonString = jsonTransactionToken.ToString();
            Transaction? transaction = Transaction.ParseTransactionFromJson(transactionJsonString);
            if (transaction != null)
            {
                TransactionReceived?.Invoke(transaction);
                Serilog.Log.Verbose($"Microcoin peer | Transaction({transaction.GetHashCode()}) accepted from network");
            }
        }
    }
}
