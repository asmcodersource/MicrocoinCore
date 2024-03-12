using Microcoin.Blockchain.Block;
using Microcoin.Blockchain.Chain;
using Microcoin.ChainsIO;

namespace Tests
{
    public class ChainStreamTests
    {
        [Fact]
        public void StreamWriteReadTest()
        {
            Stream stream = new MemoryStream(1024 * 1024 * 1024);

            // Create some chain with fake blocks
            Chain chain = new Chain();
            List<Peer> peers = TransactionTheory.CreateTestPeers(10);
            for (int i = 0; i < 10; i++)
            {
                var transactionsTheories = TransactionTheory.GetValidTransactionsTheories(peers, 10);
                var block = new Block();
                foreach(var transactionTheory in transactionsTheories)
                    block.Transactions.Add(transactionTheory.Transaction);
                block.Hash = block.GetMiningBlockHash();
                chain.AddTailBlock(block);
            }

            ChainStreaming.WriteChainToStream(stream, chain, CancellationToken.None).Wait();
            stream.Seek(0, SeekOrigin.Begin);
            var readedChain = ChainStreaming.ReadChainFromStream(stream, CancellationToken.None).Result;

        }
    }
}
