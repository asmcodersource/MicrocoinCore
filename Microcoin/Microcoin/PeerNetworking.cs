using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin.Blockchain.Transaction;
using Microcoin.Microcoin.Network;
using Microcoin.Microcoin.Network.MessageAcceptors;
using SimpleInjector;
using System.Text.Json;


namespace Microcoin.Microcoin
{
    public class PeerNetworking
    {
        public Container ServicesContainer { get; protected set; }
        public IBroadcastTransceiver BroadcastTransceiver { get; protected set; }
        public IAcceptor EntryAcceptor { get; protected set; }
        public BlocksAcceptor BlocksAcceptor { get; protected set; }
        public TransactionsAcceptor TransactionsAcceptor { get; protected set; }

        public event Action<Block>? BlockReceived;
        public event Action<List<Transaction>>? TransactionsReceived;

        public PeerNetworking(Container servicesContainer)
        {
            ServicesContainer = servicesContainer;
            BlocksAcceptor = new BlocksAcceptor();
            TransactionsAcceptor = new TransactionsAcceptor();
            EntryAcceptor = new EntryAcceptor(TransactionsAcceptor, BlocksAcceptor);
            BroadcastTransceiver = servicesContainer.GetInstance<IBroadcastTransceiver>();
            CreateDefaultRouting();
        }

        public virtual void CreateDefaultRouting()
        {
            BlocksAcceptor.BlockReceived += (block) => BlockReceived?.Invoke(block);
            TransactionsAcceptor.TransactionReceived += (transactions) => TransactionsReceived?.Invoke(transactions);
            BroadcastTransceiver.OnMessageReceived += (messageContext) => EntryAcceptor.Handle(messageContext);
        }

        public async Task SendTransactionsToNetwork(List<Transaction> transactions)
        {
            var transactionJson = JsonSerializer.Serialize(transactions);
            BroadcastTransceiver.SendBroadcastMessageAsync(transactionJson, "transactions", 128, CancellationToken.None);
        }

        public async Task SendBlockToNetwork(Block block)
        {
            var blockJson = JsonSerializer.Serialize(block);
            BroadcastTransceiver.SendBroadcastMessageAsync(block, "block", 128, CancellationToken.None);
        }
    }
}
