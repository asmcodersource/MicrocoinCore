using Microcoin.Microcoin.Blockchain.Transaction;
using Microcoin.Microcoin.Blockchain.BlocksPool;
using Microcoin.Microcoin.Blockchain.TransactionsPool;
using Microcoin.Microcoin.Blockchain.ChainController;
using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin.Mining;
using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin.ChainFetcher;
using NodeNet.NodeNet;
using Microcoin.Microcoin.Network.ChainFethingNetwork.ProviderSessionListener;
using SimpleInjector;

namespace Microcoin.Microcoin
{
    public class Peer
    {
        public BlocksPool BlocksPool { get; protected set; }
        public TransactionsPool TransactionsPool { get; protected set; } 
        public ProviderSessionListener ProviderSessionListener { get; protected set; }
        public PeerNetworking PeerNetworking { get; protected set; }
        public PeerWalletKeys PeerWalletKeys { get; protected set; }
        public PeerChain PeerChain { get; protected set; }
        public PeerMining PeerMining { get; protected set; } 
        public ChainFetcher.ChainFetcher ChainFetcher { get; protected set; }
        public string WalletPublicKey { get { return PeerWalletKeys.TransactionSigner.SignOptions.PublicKey; } }
        public Container ServicesContainer { get; protected set; }

        public Peer(Container servicesContainer)
        {
            ServicesContainer = servicesContainer;
            ChainFetcher = servicesContainer.GetInstance<ChainFetcher.ChainFetcher>();
            PeerWalletKeys = servicesContainer.GetInstance<PeerWalletKeys>();
            TransactionsPool = servicesContainer.GetInstance<TransactionsPool>();
            BlocksPool = servicesContainer.GetInstance<BlocksPool>();
            PeerNetworking = new PeerNetworking(servicesContainer);
            PeerMining = new PeerMining(servicesContainer);
            ProviderSessionListener = new ProviderSessionListener(servicesContainer);
            PeerChain = new PeerChain(servicesContainer, this);
            ConnectComponents();
        }
 
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

        private void ConnectComponents()
        {
            PeerMining.LinkToTransactionsPool(TransactionsPool);
            ChainFetcher.ChangeSourceChain(PeerChain.GetChainTail());
            ProviderSessionListener.ChangeSourceChain(PeerChain.GetChainTail());
            ChainFetcher.ChainFetchCompleted += PeerChain.ReplaceByMoreComprehinsive;
            PeerMining.BlockMined += async (block) => await BlocksPool.HandleBlock(block);
            PeerMining.BlockMined += (block) => PeerNetworking.SendBlockToNetwork(block);
            PeerChain.ChainTailPartChanged += ChainFetcher.ChangeSourceChain;
            PeerChain.ChainTailPartChanged += ProviderSessionListener.ChangeSourceChain;
            PeerChain.ChainTailPartChanged += (chain) => PeerMining.ResetBlockMiningHandler(chain, WalletPublicKey);
            PeerChain.ChainReceiveNextBlock += (chain, block) => PeerMining.ResetBlockMiningHandler(chain, WalletPublicKey);
            PeerNetworking.TransactionReceived += async (transaction) => await TransactionsPool.HandleTransaction(transaction);
            PeerNetworking.BlockReceived += async (block) => await BlocksPool.HandleBlock(block);
            BlocksPool.OnBlockReceived += async (pool, block) => await PeerChain.TryAcceptBlock(block);
            TransactionsPool.OnTransactionReceived += (transaction) => PeerMining.TryStartMineBlock(PeerChain.GetChainTail(), WalletPublicKey);
        }
    }
}
