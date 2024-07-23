using Microcoin.Microcoin;
using Microcoin.Microcoin.Blockchain.Transaction;
using Microcoin.Microcoin.Blockchain.TransactionsPool;
using System.Text;
using Tests.Generators;

namespace Tests
{
    public class TransactionsPipelineTests
    {
        readonly int theoriesListLenght = 1024;
        readonly int peersListLength = 12;
        protected List<Peer> peers = new List<Peer>();

        public TransactionsPipelineTests()
        {
            peers = TransactionTheoriesGenerator.CreateTestPeers(peersListLength);
        }

        private Microcoin.Microcoin.Blockchain.TransactionsPool.TransactionsPool CreateDefaultTransactionsPool()
        {
            // Create default transactions pool
            TransactionsPool transactionsPool = new TransactionsPool();
            return transactionsPool;
        }

        [Fact]
        public async Task TransactionsPipeline_ValidTrasnactions_Test()
        {
            var transactionsPool = CreateDefaultTransactionsPool();
            var transactionTheories = TransactionTheoriesGenerator.GetValidTransactionsTheories(peers, theoriesListLenght);
            foreach (var theorie in transactionTheories)
                Assert.Equal(theorie.IsTransactionValid, transactionsPool.HandleTransaction(theorie.Transaction));
        }

        [Fact]
        public void TransactionsPipeline_InvalidTrasnactions_Test()
        {
            var transactionsPool = CreateDefaultTransactionsPool();
            var transactionTheories = TransactionTheoriesGenerator.GetInvalidTransactionsTheories(peers, theoriesListLenght);
            foreach (var theorie in transactionTheories)
                Assert.Equal(theorie.IsTransactionValid, transactionsPool.HandleTransaction(theorie.Transaction));
        }

        [Fact]
        public async Task TransactionsPipeline_ValidAndInvalidTrasnactions_Test()
        {
            var transactionsPool = CreateDefaultTransactionsPool();
            var transactionInvalidTheories = TransactionTheoriesGenerator.GetInvalidTransactionsTheories(peers, theoriesListLenght);
            var transactionValidTheories = TransactionTheoriesGenerator.GetValidTransactionsTheories(peers, theoriesListLenght);
            List<TransactionTheory> transactionTheories = new List<TransactionTheory>(transactionValidTheories.Concat(transactionInvalidTheories));
            // random shuffle
            var rand = new Random();
            for (int i = 0; i < transactionTheories.Count(); i++)
            {
                int shuffle = rand.Next(transactionTheories.Count());
                var temp = transactionTheories[i];
                transactionTheories[i] = transactionTheories[shuffle];
                transactionTheories[shuffle] = temp;
            }
            foreach (var theorie in transactionTheories)
                Assert.Equal(theorie.IsTransactionValid, transactionsPool.HandleTransaction(theorie.Transaction));
        }
    }
}
