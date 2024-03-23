using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin.ChainStorage;
using Microcoin.Microcoin.Mining;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin
{
    // Used to create the very first chain, as well as the very first block in the chain. This is the point from where it is determined who will receive the first coins on the network.
    internal class InitialChainCreator
    {
        public Peer InitialPeer { get; protected set; }
        public Chain InitialChain { get; protected set; }


        public InitialChainCreator()
        {
            InitialPeer = new Peer();
            InitialPeer.LoadOrCreateWalletKeys("initial-peer-wallet.keys");
            InitialPeer.InitializeAcceptancePools();
            InitialPeer.InitializeMining();
        }

        /// <summary>
        /// Peer should be initilized, at least peer mining, acceptance pools, and mining
        /// </summary>
        /// <param name="peer"></param>
        public InitialChainCreator(Peer peer) 
        {
            InitialPeer = peer;
        }

        public  void CreateInitialialChain()
        {
            var miner = InitialPeer.PeerMining.Miner;
            var zeroChain = new Chain();
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
        }

        /// <summary>
        /// Store chain to chain storage, chains storage should be initialized in depency injection
        /// </summary>
        /// <param name="peer"></param>
        public void StoreInitialChainToFile()
        {
            var chainStorage = DepencyInjection.Container.GetInstance<ChainStorage>();
            StoreInitialChainToFile(chainStorage);
        }

        public void StoreInitialChainToFile(ChainStorage chainStorage)
        {
            if (InitialChain is null)
                throw new NullReferenceException("InitialChain is not initialized");
            chainStorage.AddNewChainToStorage(InitialChain);
        }
    }
}
