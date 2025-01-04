using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin.Mining;
using Microcoin.Microcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.NetworkTesting.TestNetworks;
using Tests.MicrocoinCoreTesting.Generators;
using Xunit;

namespace Tests.MicrocoinCoreTesting.PipelinesTesting
{
    public class BlocksPipelineTesting
    {
        [Fact]
        public void BlockPipeline_Hash_Test()
        {
            // Same block have to has same hash
            List<Peer> peers = TransactionTheoriesGenerator.CreateTestPeers(10);
            var transactionsTheories = TransactionTheoriesGenerator.GetValidTransactionsTheories(peers, 10);
            var block = new Block();
            foreach (var transactionTheory in transactionsTheories)
                block.Transactions.Add(transactionTheory.Transaction);
            block.Hash = block.GetMiningBlockHash();
            var firstHash = block.Hash;
            block.Hash = block.GetMiningBlockHash();
            block.Hash = block.GetMiningBlockHash();
            block.Hash = block.GetMiningBlockHash();
            block.Hash = block.GetMiningBlockHash();
            Assert.Equal(firstHash, block.Hash);
        }

        [Fact]
        public void ChainVerificator_ShouldReturnTrue()
        {
            var network = TwoNodesNetwork.Create();

            var chainGenerator = new MicrocoinTestChainsGenerator();
            var generatedChainTail = chainGenerator.CreateChain(2, 2, 2);
            var verificationFromId = Random.Shared.Next(1, generatedChainTail.EntireChainLength - 1);
            var verificationFromBlock = generatedChainTail.GetBlockFromHead(verificationFromId);
            var complexityRule = new DebugComplexityRule();
            var rewardRule = new RewardRule();
            var miningRules = new MiningRules(complexityRule, rewardRule);
            Miner miner = new Miner();
            miner.SetRules(miningRules);
            PeerBuilder peerBuilder = new PeerBuilder();
            peerBuilder.AddDebugMiner();
            peerBuilder.AddDefaultAcceptancePools();
            peerBuilder.AddDefaultRules();
            peerBuilder.AddBroadcastTransceiver(network.FirstNode);
            peerBuilder.AddBroadcastSessionManager(network.FirstNode);
            peerBuilder.AddChainsStorage("chains");
            peerBuilder.AddChainsFetcher();
            peerBuilder.AddChainBranchBlocksCount(10);
            peerBuilder.AddWalletKeys();
            var chainVerificator = new ChainVerificator(peerBuilder.ServicesContainer);
            bool isChainValid = chainVerificator.VerifyChain(generatedChainTail, verificationFromBlock).Result;
            Assert.True(isChainValid, "Chain wasn't verified as right chain");

        }
    }
}
