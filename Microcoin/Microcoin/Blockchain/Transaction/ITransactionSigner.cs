using Microcoin.Microcoin.RSAEncryptions;

namespace Microcoin.Microcoin.Blockchain.Transaction
{
    public interface ITransactionSigner
    {
        public void SetSignOptions(ISenderSignOptions options);
        public void Sign(Transaction message);
    }
}
