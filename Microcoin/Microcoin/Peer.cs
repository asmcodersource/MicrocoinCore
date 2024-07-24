using Microcoin.Microcoin.Blockchain.BlocksPool;
using Microcoin.Microcoin.Blockchain.Transaction;
using Microcoin.Microcoin.Blockchain.TransactionsPool;
using SimpleInjector;

namespace Microcoin.Microcoin
{
    public class Peer
    {
        public BlocksPool BlocksPool { get; protected set; }
        public TransactionsPool TransactionsPool { get; protected set; }
        public PeerNetworking PeerNetworking { get; protected set; }
        public PeerWalletKeys PeerWalletKeys { get; protected set; }
        public PeerChain PeerChain { get; protected set; }
        public PeerMining PeerMining { get; protected set; }
        public PeerChainsProvider PeerChainsProvider { get; protected set; }
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
            PeerChain = new PeerChain(servicesContainer, this);
            PeerChainsProvider = new PeerChainsProvider(servicesContainer);
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
            TransactionsPool.HandleTransaction(transaction);
            return transaction;
        }

        private void ConnectComponents()
        {
            PeerMining.LinkToTransactionsPool(TransactionsPool);
            ChainFetcher.ChangeSourceChain(PeerChain.GetChainTail());
            PeerChainsProvider.ChangeSourceChain(PeerChain.GetChainTail());
            ChainFetcher.ChainFetchCompleted += PeerChain.ReplaceByMoreComprehinsive;
            PeerMining.BlockMined += (block) => BlocksPool.HandleBlock(block);
            PeerMining.BlockMined += async (block) => await PeerNetworking.SendBlockToNetwork(block);
            PeerChain.ChainTailPartChanged += ChainFetcher.ChangeSourceChain;
            PeerChain.ChainTailPartChanged += PeerChainsProvider.ChangeSourceChain;
            PeerChain.ChainTailPartChanged += (chain) => PeerMining.ResetBlockMiningHandler(chain, WalletPublicKey);
            PeerChain.ChainReceiveNextBlock += (chain, block) => PeerMining.ResetBlockMiningHandler(chain, WalletPublicKey);
            PeerNetworking.TransactionsReceived += (transactions) => TransactionsPool.HandleTransactions(transactions);
            PeerNetworking.BlockReceived += (block) => BlocksPool.HandleBlock(block);
            BlocksPool.OnBlockReceived += (pool, block) => PeerChain.TryAcceptBlock(block);
            TransactionsPool.OnTransactionReceived += (transaction) => PeerMining.TryStartMineBlock(PeerChain.GetChainTail(), WalletPublicKey);
            TransactionsPool.TransactionsBag.OnTransactionsBagReady += async (transactions) => await PeerNetworking.SendTransactionsToNetwork(transactions);
        }
    }
}
