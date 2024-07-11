using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin;
using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin.Mining;


namespace Tests.Generators
{
    internal class MicrocoinTestChainsGenerator
    {
        protected IMiner miner;
        protected List<Peer> peers;
        protected Dictionary<Peer, double> peersCoins;

        public MicrocoinTestChainsGenerator()
        {
            var miner = new Miner(); // I use default microcoin miner for creating test chains
            miner.SetRules(new MiningRules(new DebugComplexityRule(), new RewardRule()));
            this.miner = miner;
            peers = TransactionTheoriesGenerator.CreateTestPeers(10);
            peersCoins = new Dictionary<Peer, double>();
        }

        public MutableChain CreateChain(int chainPartsOfChain, int blocksCountPerChain, int trasnactionsCountPerChain)
        {
            var tailChain = GetZeroChain();
            for (int chainPartId = 0; chainPartId < chainPartsOfChain; chainPartId++)
            {
                var nextChain = CreateChainContinueChain(tailChain, blocksCountPerChain, trasnactionsCountPerChain);
                tailChain = nextChain;
            }
            return tailChain;
        }

        protected MutableChain CreateChainContinueChain(MutableChain previousChain, int blocksCountPerChain, int trasnactionsCountPerChain)
        {
            MutableChain chainContinue = new MutableChain();
            chainContinue.LinkPreviousChain(previousChain);
            for (int blockId = 0; blockId < blocksCountPerChain; blockId++)
            {
                Block block = new Block();
                for (int transactionId = 0; transactionId < trasnactionsCountPerChain; transactionId++)
                {
                    // Get two random differend peers for new transaction
                    Peer peerSender, peerReceiver;
                    peerSender = peersCoins.ElementAt(Random.Shared.Next(peersCoins.Count)).Key;
                    peerReceiver = peers.ElementAt(Random.Shared.Next(peers.Count));
                    double coinsToSend = peersCoins[peerSender] / trasnactionsCountPerChain; // this way guaranteed to not take too much, leaving coins for the next transactions in the block.
                    var transaction = peerSender.CreateTransaction(peerReceiver.WalletPublicKey, coinsToSend);
                    DoPeerCoinsCount(peerSender, -coinsToSend);
                    DoPeerCoinsCount(peerReceiver, +coinsToSend);
                    block.Transactions.Add(transaction);
                }
                // Mine block before adding it to chain
                var minerPeer = peers[Random.Shared.Next(peers.Count)];
                (miner as IMiner).LinkBlockToChain(chainContinue, block);
                block.Hash = miner.StartBlockMining(chainContinue, block, minerPeer.WalletPublicKey, CancellationToken.None).Result;
                // Add mined block to chain, and count mining reward to miner wallet
                chainContinue.AddTailBlock(block);
                DoPeerCoinsCount(minerPeer, block.MiningBlockInfo.MinerReward);
            }
            return chainContinue;
        }

        // Any chain must start from the first block, which is the initial block.
        // In this project, this block is called Zero Block, and this Chain is called also Zero Chain.
        protected MutableChain GetZeroChain()
        {
            var zeroChain = new MutableChain();
            var zeroTransactionPeer = peers[Random.Shared.Next(peers.Count)];
            // Create new block with one initial transaction
            Block zeroBlock = new Block();
            zeroBlock.MiningBlockInfo = new MiningBlockInfo();
            zeroBlock.MiningBlockInfo.CreateTime = DateTime.Now;
            zeroBlock.MiningBlockInfo.BlockId = 0;
            zeroBlock.MiningBlockInfo.PreviousBlockHash = "none";
            zeroBlock.Transactions.Add(zeroTransactionPeer.CreateTransaction(zeroTransactionPeer.WalletPublicKey, 0));
            // Mine this block with miner, and add it to zero chain
            zeroBlock.Hash = miner.StartBlockMining(zeroChain, zeroBlock, zeroTransactionPeer.WalletPublicKey, CancellationToken.None).Result;
            zeroChain.AddTailBlock(zeroBlock);
            DoPeerCoinsCount(zeroTransactionPeer, zeroBlock.MiningBlockInfo.MinerReward);
            return zeroChain;
        }

        // We keep records of all coins from all users of the test blockchain,
        // to easily find errors if they occur, as well as an easy way to create realistic test transactions.
        protected void DoPeerCoinsCount(Peer peer, double coinsChange)
        {
            peersCoins.TryAdd(peer, 0);
            var peerCoins = peersCoins[peer];
            var newPeerCoins = peerCoins + coinsChange;
            Assert.True(newPeerCoins >= 0);
            if (newPeerCoins == 0)
                peersCoins.Remove(peer);
            else
                peersCoins[peer] = newPeerCoins;
        }
    }
}
