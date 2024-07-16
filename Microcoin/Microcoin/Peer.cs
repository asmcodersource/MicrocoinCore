﻿using Microcoin.Microcoin.Blockchain.Transaction;
using Microcoin.Microcoin.Blockchain.BlocksPool;
using Microcoin.Microcoin.Blockchain.TransactionsPool;
using Microcoin.Microcoin.Blockchain.ChainController;
using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin.Mining;
using NodeNet.NodeNet;

namespace Microcoin.Microcoin
{
    public class Peer
    {
        public BlocksPool BlocksPool { get; protected set; } = new BlocksPool();
        public TransactionsPool TransactionsPool { get; protected set; } = new TransactionsPool();
        public PeerNetworking PeerNetworking { get; protected set; }
        public PeerWalletKeys PeerWalletKeys { get; protected set; }
        public PeerChain PeerChain { get; protected set; }
        public PeerMining PeerMining { get; protected set; } 
        public ChainFetcher.ChainFetcher ChainFetcher { get; protected set; }


        public string WalletPublicKey { get { return PeerWalletKeys.TransactionSigner.SignOptions.PublicKey; } }

        public Transaction CreateTransaction(string receiverPublicKey, double coinsCount)
        {
            var transaction = new Transaction();
            transaction.ReceiverPublicKey = receiverPublicKey;
            transaction.TransferAmount = coinsCount;
            PeerWalletKeys.SignTransaction(transaction);
            return transaction;
        }

        public Transaction SendCoins(string receiverPublicKey, double coinsCount)
        {
            if (PeerNetworking == null || PeerWalletKeys == null)
                throw new NullReferenceException("Peer is not initialized");

            var transaction = CreateTransaction(receiverPublicKey, coinsCount);
            PeerNetworking.SendTransactionToNetwork(transaction);
            TransactionsPool.HandleTransaction(transaction).Wait();
            return transaction;
        }

        public void InitializeChain()
        {
            PeerChain = new PeerChain(PeerMining.Miner);
            var chainsStorage = DepencyInjection.Container.GetInstance<ChainStorage.ChainStorage>();
            chainsStorage.FetchChains();
            if (chainsStorage.CountOfChainsHeaders() == 0)
                PeerChain.InitByInitialChain();
            else
                PeerChain.InitByMostComprehensive();
            PeerChain.ChainReceiveNextBlock += (block) => ResetBlockMiningHandler(block);
            Serilog.Log.Information($"Microcoin peer | Peer({this.GetHashCode()}) chain initialized");
        }

        public void InitializeAcceptancePools()
        {
            TransactionsPool.InitializeHandlerPipeline();
            BlocksPool.InitializeHandlerPipeline(TransactionsPool.HandlePipeline);
            BlocksPool.OnBlockReceived += (pool, block) => PeerChain.ChainController.AcceptBlock(block);
        }

        public void InitializeMining(bool miningEnable = true)
        {
            var complexityRule = new ComplexityRule();
            var rewardRule = new RewardRule();
            var miningRules = new MiningRules(complexityRule, rewardRule);
            Miner miner = new Miner();
            miner.SetRules(miningRules);
            PeerMining = new PeerMining();
            PeerMining.InizializeMiner(miner, WalletPublicKey, TransactionsPool);
            PeerMining.BlockMined += BlockMinedHandler;
            if (miningEnable)
                PeerMining.StartMining();

            TransactionsPool.OnTransactionReceived += (transaction) => PeerMining.TryStartMineBlock(PeerChain.GetChainTail(), new DeepTransactionsVerify());
            Serilog.Log.Information($"Microcoin peer | Peer({this.GetHashCode()}) mining initialized");
        }

        public void InitializeNetworking(Node nodeNet)
        {
            PeerNetworking = new PeerNetworking(nodeNet);
            PeerNetworking.CreateDefaultRouting();
            ChainFetcher = new ChainFetcher.ChainFetcher(PeerNetworking.NetworkNode);
            PeerChain.SetChainFetcher(ChainFetcher);
            PeerNetworking.TransactionReceived += async (transaction) => await TransactionsPool.HandleTransaction(transaction);
            PeerNetworking.BlockReceived += async (block) => await BlocksPool.HandleBlock(block);
            Serilog.Log.Information($"Microcoin peer | Peer({this.GetHashCode()}) network initialized");
            Serilog.Log.Information($"Microcoin peer | Peer({this.GetHashCode()}) listening on port: {this.PeerNetworking.NetworkNode.GetNodeTcpPort()}");
        }

        public void InitializeNetworking(int portForNodeNet = 0)
        {
            PeerNetworking = new PeerNetworking();
            PeerNetworking.CreateDefaultNode(portForNodeNet);
            PeerNetworking.CreateDefaultRouting();
            PeerNetworking.PostInitialize();
            PeerNetworking.TransactionReceived += (transaction) => TransactionsPool.HandleTransaction(transaction);
            PeerNetworking.BlockReceived += (block) => BlocksPool.HandleBlock(block);
            Serilog.Log.Information($"Microcoin peer | Peer({this.GetHashCode()}) network initialized");
            Serilog.Log.Information($"Microcoin peer | Peer({this.GetHashCode()}) listening on port: {this.PeerNetworking.NetworkNode.GetNodeTcpPort()}");
        }

        public void LoadOrCreateWalletKeys(string filePath = "wallet.keys")
        {
            PeerWalletKeys = new PeerWalletKeys();
            if (File.Exists(filePath))
            {
                PeerWalletKeys.LoadKeys(filePath);
            }
            else
            {
                PeerWalletKeys.CreateKeys();
                PeerWalletKeys.SaveKeys(filePath);
            }
            Serilog.Log.Information($"Microcoin peer | Peer({this.GetHashCode()}) keys initialized");
        }

        protected void BlockMinedHandler(Microcoin.Blockchain.Block.Block block)
        {
            PeerNetworking.SendBlockToNetwork(block);
            PeerChain.ChainController.AcceptBlock(block).Wait();
            ResetBlockMiningHandler(block);
        }

        protected void ResetBlockMiningHandler(Block block)
        {
            PeerMining.CancelCurrentMiningProcess();
            PeerMining.TryStartMineBlock(PeerChain.GetChainTail(), new DeepTransactionsVerify());
        }
    }
}
