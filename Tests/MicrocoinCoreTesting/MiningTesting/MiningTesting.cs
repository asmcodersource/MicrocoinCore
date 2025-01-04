using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin.Mining;
using Microcoin.Microcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Tests.NetworkTesting.TestNetworks;
using System.Reflection.PortableExecutable;

namespace Tests.MicrocoinCoreTesting.MiningTesting
{
    public class MiningTesting
    {
        [Fact]
        public void OneBlockMiningTest()
        {
            // Create peer for transactions in chain 
            var network = TwoNodesNetwork.Create();
            var peerBuilder = new PeerBuilder();
            peerBuilder.AddDebugServices();
            peerBuilder.AddBroadcastSessionManager(network.FirstNode);
            peerBuilder.AddBroadcastTransceiver(network.FirstNode);
            var peer = peerBuilder.Build();

            // Create first block with some transactions
            Block block = new Block();
            block.Hash = "none";
            block.MiningBlockInfo = new MiningBlockInfo();
            block.MiningBlockInfo.BlockId = 0;
            block.MiningBlockInfo.MinerReward = 1000;
            block.MiningBlockInfo.PreviousBlockHash = "none";
            block.Transactions.Add(peer.CreateTransaction(peer.WalletPublicKey, 0));


            Microcoin.Microcoin.Blockchain.Chain.MutableChain chain = new MutableChain();
            Miner miner = new Miner();
            miner.SetRules(new MiningRules(new DebugComplexityRule(), new RewardRule()));
            miner.StartBlockMining(chain, block, peer.WalletPublicKey, CancellationToken.None).Wait();
        }

        [Fact]
        public void TwoBlockMiningTest()
        {
            // Create peers for transactions in chain 
            var network = TwoNodesNetwork.Create();
            var firstPeerBuilder = new PeerBuilder();
            firstPeerBuilder.AddDebugServices();
            firstPeerBuilder.AddBroadcastSessionManager(network.FirstNode);
            firstPeerBuilder.AddBroadcastTransceiver(network.FirstNode);
            Peer first_peer = firstPeerBuilder.Build();
            var secondPeerBuilder = new PeerBuilder();
            secondPeerBuilder.AddDebugServices();
            secondPeerBuilder.AddBroadcastSessionManager(network.SecondNode);
            secondPeerBuilder.AddBroadcastTransceiver(network.SecondNode);
            Peer second_peer = secondPeerBuilder.Build();

            // Create first block with some transactions
            Block first_block = new Block();
            first_block.Hash = "none";
            first_block.MiningBlockInfo = new MiningBlockInfo();
            first_block.MiningBlockInfo.BlockId = 0;
            first_block.MiningBlockInfo.MinerReward = 1000;
            first_block.MiningBlockInfo.PreviousBlockHash = "none";
            first_block.Transactions.Add(first_peer.CreateTransaction(first_peer.WalletPublicKey, 0));

            // add this first block to chain
            MutableChain chain = new MutableChain();
            Miner miner = new Miner();
            miner.SetRules(new MiningRules(new DebugComplexityRule(), new RewardRule()));
            var first_hash = miner.StartBlockMining(chain, first_block, first_peer.WalletPublicKey, CancellationToken.None).Result;
            first_block.Hash = first_hash;
            chain.AddTailBlock(first_block);

            // Create second block
            Block second_block = new Block();
            second_block.Transactions.Add(first_peer.CreateTransaction(second_peer.WalletPublicKey, 1.5));
            (miner as IMiner).LinkBlockToChain(chain, second_block);
            var second_hash = miner.StartBlockMining(chain, second_block, first_peer.WalletPublicKey, CancellationToken.None).Result;
            second_block.Hash = second_hash;

            /*// verify block is really correct by chain controller
            Microcoin.Microcoin.Blockchain.ChainController.ChainController chainController = new ChainController(chain, miner);
            chainController.DefaultInitialize();
            Assert.True(chainController.AcceptBlock(second_block).Result);*/
        }
    }
}
