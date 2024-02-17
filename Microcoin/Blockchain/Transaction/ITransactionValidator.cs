using Microcoin.RSAEncryptions;

namespace Microcoin.Blockchain.Transaction
{
    internal interface ITransactionValidator
    {
        void SetValidateOptions(IReceiverSignOptions options);
        bool Validate(Transaction message);
    }
}
