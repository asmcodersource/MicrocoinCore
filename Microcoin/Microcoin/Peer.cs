using Microcoin.Microcoin.Blockchain.Mining;
using Microcoin.Microcoin.Blockchain.Transaction;
using Microcoin.Microcoin.Blockchain.BlocksPool;
using Microcoin.Microcoin.Blockchain.TransactionsPool;
using Microcoin.Microcoin.Blockchain.ChainController;
using Microcoin.Microcoin.Blockchain.Block;

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
            TransactionsPool.HandleTransaction(transaction).Wait();
            return transaction;
        }

        public void InitializeChain()
        {
            ChainFetcher = new ChainFetcher.ChainFetcher();
            PeerChain = new PeerChain(PeerMining.Miner, ChainFetcher);
            var chainsStorage = DepencyInjection.Container.GetInstance<ChainStorage.ChainStorage>();
            chainsStorage.FetchChains();
            if (chainsStorage.CountOfChainsHeaders() == 0)
                PeerChain.SetInitialChain();
            else
                PeerChain.SetMostComprehensive();
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

            TransactionsPool.OnTransactionReceived += (transaction)
                => PeerMining.TryStartMineBlock(PeerChain.GetChainTail(), new DeepTransactionsVerify());
            Serilog.Log.Information($"Microcoin peer | Peer({this.GetHashCode()}) mining initialized");
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
            BlocksPool.HandleBlock(block).Wait();
            PeerNetworking.SendBlockToNetwork(block);
            PeerMining.StopMining();
            PeerMining.TryStartMineBlock(PeerChain.GetChainTail(), new DeepTransactionsVerify());
        }
    }
}
