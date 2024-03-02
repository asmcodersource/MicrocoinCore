using Microcoin.Blockchain.Chain;
using Microcoin.Blockchain.Block;
using Microcoin.Blockchain.Mining;

namespace Tests
{
    public class MiningTests
    {
        [Fact]
        public void StandardMiningTest()
        {
            // Create peer for transactions in chain 
            Peer peer = new Peer();
            peer.LoadOrCreateWalletKeys("NUL");
            peer.InitializeAcceptancePools();
           
            // Create first block with some transactions
            Block block = new Block();
            block.Hash = "none";
            block.MiningBlockInfo = new Microcoin.Blockchain.Mining.MiningBlockInfo();
            block.MiningBlockInfo.BlockId = 0;
            block.MiningBlockInfo.MinerReward = 1000;
            block.MiningBlockInfo.PreviousBlockHash = "none";
            block.Transactions.Add(peer.CreateTransaction(peer.WalletPublicKey, 0));


            Chain chain = new Chain();
            Miner miner = new Miner();
            miner.SetRules(new MiningRules(new ComplexityRule(), new RewardRule()));
            var blockHash = miner.StartBlockMining(chain, block, peer.WalletPublicKey, CancellationToken.None).Result;
            block.Hash = blockHash;

        }
    }
}
