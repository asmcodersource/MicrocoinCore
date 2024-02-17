using Microcoin.RSAEncryptions;

namespace Microcoin.Blockchain.Transaction
{
    internal interface ITransactionSigner
    {
        void SetSignOptions(ISenderSignOptions options);
        void Sign(Transaction message);
    }
}
