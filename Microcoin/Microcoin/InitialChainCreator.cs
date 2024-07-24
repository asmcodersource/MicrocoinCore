using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin.Mining;

namespace Microcoin.Microcoin
{
    // Used to create the very first chain, as well as the very first block in the chain. This is the point from where it is determined who will receive the first coins on the network.
    public class InitialChainCreator
    {
        public Peer InitialPeer { get; protected set; }
        public MutableChain? InitialChain { get; protected set; }

        /// <summary>
        /// Peer should be initilized, at least peer mining, acceptance pools, and mining
        /// </summary>
        /// <param name="peer"></param>
        public InitialChainCreator(Peer peer)
        {
            InitialPeer = peer;
        }

        public MutableChain CreateInitialialChain()
        {
            var miner = InitialPeer.PeerMining.Miner;
            var zeroChain = new MutableChain();
            var zeroTransactionPeer = InitialPeer;
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
            InitialChain = zeroChain;
            return InitialChain;
        }

        /// <summary>
        /// Store chain to chain storage, chains storage should be initialized in depency injection
        /// </summary>
        /// <param name="peer"></param>
        public void StoreInitialChainToFile()
        {
            var chainStorage = InitialPeer.ServicesContainer.GetInstance<ChainStorage.ChainStorage>();
            StoreInitialChainToFile(chainStorage);
        }

        public void StoreInitialChainToFile(ChainStorage.ChainStorage chainStorage)
        {
            if (InitialChain is null)
                throw new NullReferenceException("InitialChain is not initialized");
            chainStorage.AddNewChainToStorage(InitialChain);
        }
    }
}
