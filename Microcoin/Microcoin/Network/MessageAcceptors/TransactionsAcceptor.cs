using NodeNet.NodeNet.Message;
using Microcoin.Microcoin.Blockchain.Transaction;
using Newtonsoft.Json.Linq;
using Microcoin.Microcoin.Blockchain.Block;
using System.Text.Json;

namespace Microcoin.Microcoin.Network.MessageAcceptors
{
    public class TransactionsAcceptor : IAcceptor
    {
        public event Action<List<Transaction>> TransactionsReceived;

        public virtual async Task Handle(MessageContext messageContext)
        {
            JObject jsonRequestObject = JObject.Parse(messageContext.Message.Data);
            JToken? jsonTransactionToken = jsonRequestObject["transaction"];
            if (jsonTransactionToken is null)
                return;
            string transactionJsonString = jsonTransactionToken.ToString();
            List<Transaction> transaction = JsonSerializer.Deserialize<List<Transaction>>(transactionJsonString);
            if (transaction != null)
            {
                TransactionsReceived?.Invoke(transaction);
                Serilog.Log.Verbose($"Microcoin peer | Transaction({transaction.GetHashCode()}) accepted from network");
            }
        }
    }
}
