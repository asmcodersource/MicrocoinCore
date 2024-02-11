using Microcoin.RSAEncryptions;

namespace Microcoin.Transaction
{
    internal interface ITransactionSigner
    {
        void SetSignOptions(ISenderSignOptions options);
        void Sign(ITransaction message);
    }
}
