using Microcoin.Microcoin.Blockchain.Transaction;
using NodeNet.NodeNet;
using NodeNet.NodeNet.RSAEncryptions;
using NodeNet.NodeNet.TcpCommunication;
using Microcoin.Microcoin.Network.MessageAcceptors;
using Microcoin.Microcoin.Network.ChainFethingNetwork.ProviderSessionListener;
using Microcoin.Microcoin.Blockchain.BlocksPool;
using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin.Blockchain.TransactionsPool;
using SimpleInjector;

namespace Microcoin.Microcoin
{
    public class PeerNetworking
    {
        public Container ServicesContainer { get; protected set; }
        public Node NetworkNode { get; protected set; }
        public IAcceptor EntryAcceptor { get; protected set; }
        public BlocksAcceptor BlocksAcceptor { get; protected set; }
        public TransactionsAcceptor TransactionsAcceptor { get; protected set; }

        public event Action<Microcoin.Blockchain.Block.Block>? BlockReceived;
        public event Action<List<Transaction>>? TransactionsReceived;

        public PeerNetworking(Container servicesContainer)
        {
            BlocksAcceptor = new BlocksAcceptor();
            TransactionsAcceptor = new TransactionsAcceptor();
            EntryAcceptor = new EntryAcceptor(TransactionsAcceptor, BlocksAcceptor);
            NetworkNode = servicesContainer.GetInstance<Node>();
            ServicesContainer = servicesContainer;
            NetworkNode.NetworkExplorer.LoadRecentConnectionsFromFile("knownPeers.json");
            CreateDefaultRouting();
            PostInitialize();
        }

        public virtual void CreateDefaultRouting()
        {
            BlocksAcceptor.BlockReceived += (block) => BlockReceived?.Invoke(block);
            TransactionsAcceptor.TransactionsReceived += (transactions) => TransactionsReceived?.Invoke(transactions);
            NetworkNode.MessageReceived += (messageContext) => EntryAcceptor.Handle(messageContext);
        }

        public async Task SendTransactionsToNetwork(List<Transaction> transaction)
        {
            var messageDTO = new
            {
                transaction,
                application = "Microcoin",
                type = "WalletTransaction"
            };
            var messageJson = Newtonsoft.Json.JsonConvert.SerializeObject(messageDTO);
            await NetworkNode.SendMessageAsync(messageJson, null, false, 128);
        }

        public async Task SendBlockToNetwork(Microcoin.Blockchain.Block.Block block)
        {
            var messageDTO = new
            {
                block,
                application = "Microcoin",
                type = "ChainBlock"
            };
            var messageJson = Newtonsoft.Json.JsonConvert.SerializeObject(messageDTO);
            await NetworkNode.SendMessageAsync(messageJson, null, false, 128);
        }

        private void PostInitialize()
        {
            /*            NetworkNode.NetworkExplorer.SendExploreEcho();
                        // Wait until node speaks with network for first time
                        Thread.Sleep(5000);
                        NetworkNode.NetworkExplorer.SaveRecentConnectionsToFile("knownPeers.json");*/
        }
    }
}
