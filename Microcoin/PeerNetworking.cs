using Microcoin.Network.MessageAcceptors;
using Microcoin.Network.NodeNet.TcpCommunication;
using Microcoin.Network.NodeNet;
using Microcoin.RSAEncryptions;

namespace Microcoin
{
    internal class PeerNetworking
    {
        public Node NetworkNode { get; protected set; }
        public IAcceptor EntryAcceptor { get; protected set; }
        public BlocksAcceptor BlocksAcceptor { get; protected set; }
        public TransactionsAcceptor TransactionsAcceptor { get; protected set; }

        public event Action<Blockchain.Block.Block> BlockReceived;
        public event Action<Blockchain.Transaction.Transaction> TransactionReceived;

        public virtual void CreateDefaultRouting()
        {
            BlocksAcceptor = new BlocksAcceptor();
            TransactionsAcceptor = new TransactionsAcceptor();
            EntryAcceptor = new EntryAcceptor(TransactionsAcceptor, BlocksAcceptor);
            BlocksAcceptor.BlockReceived += BlockReceived;
            TransactionsAcceptor.TransactionReceived += TransactionReceived;
            NetworkNode.MessageReceived += (messageContext) => EntryAcceptor.Handle(messageContext);
        }

        public virtual void CreateDefaultNode()
        {
            var senderEncryptionOptions = RSAEncryption.CreateSignOptions();
            var senderTcpOptions = new TcpListenerOptions(1300);
            NetworkNode = Node.CreateRSAHttpNode(senderEncryptionOptions, senderTcpOptions);
            NetworkNode.NetworkExplorer.LoadRecentConnectionsFromFile("knownPeers.json");
        }

        public void PostInitialize()
        {
            NetworkNode.NetworkExplorer.SendExploreEcho();
            // Wait until node speaks with network for first time
            Thread.Sleep(5000);
            NetworkNode.NetworkExplorer.SaveRecentConnectionsToFile("knownPeers.json");
        } 
    }
}
