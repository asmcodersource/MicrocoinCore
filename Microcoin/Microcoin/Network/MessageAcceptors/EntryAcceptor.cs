namespace Microcoin.Microcoin.Network.MessageAcceptors
{
    public class EntryAcceptor : IAcceptor
    {
        private readonly TransactionsAcceptor TransactionsAcceptor;
        private readonly BlocksAcceptor BlocksAcceptor;

        public EntryAcceptor(TransactionsAcceptor transactionsAcceptor, BlocksAcceptor blocksAcceptor)
        {
            TransactionsAcceptor = transactionsAcceptor;
            BlocksAcceptor = blocksAcceptor;
        }

        public void Handle(IBroadcastMessage message)
        {
            switch (message.PayloadType)
            {
                case "transactions":
                    TransactionsAcceptor.Handle(message);
                    break;
                case "block":
                    BlocksAcceptor.Handle(message);
                    break;
            }
        }
    }
}
