using Microcoin.Microcoin;
using Microcoin.Microcoin.ChainsIO;
using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin.Blockchain.Block;
using Tests.Generators;

namespace Tests
{
    public class ChainStreamTests
    {
        [Fact]
        public void StreamWriteReadTest()
        {
            Stream stream = new MemoryStream(1024 * 1024 * 1024);

            // Create some chain with fake blocks
            MutableChain chain = new MutableChain();
            List<Peer> peers = TransactionTheoriesGenerator.CreateTestPeers(10);
            for (int i = 0; i < 5; i++)
            {
                var transactionsTheories = TransactionTheoriesGenerator.GetValidTransactionsTheories(peers, 10);
                var block = new Block();
                foreach(var transactionTheory in transactionsTheories)
                    block.Transactions.Add(transactionTheory.Transaction);
                block.Hash = block.GetMiningBlockHash();
                chain.AddTailBlock(block);
            }

            ChainStreaming.WriteChainToStream(stream, chain, CancellationToken.None).Wait();
            stream.Seek(0, SeekOrigin.Begin);
            var readedChain = ChainStreaming.ReadChainFromStream(stream, chain.GetBlocksList().Count(), CancellationToken.None).Result;

            for (int i = 0; i < chain.GetBlocksList().Count; i++)
            {
                var first_block = chain.GetBlocksList()[i];
                var second_block = readedChain.GetBlocksList()[i];
                var first_hash = first_block.GetMiningBlockHash();
                var second_hash = second_block.GetMiningBlockHash();
                Assert.Equal(first_hash, second_hash);
            }
        }
    }
}
