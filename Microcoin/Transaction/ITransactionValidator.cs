using Microcoin.RSAEncryptions;

namespace Microcoin.Transaction
{
    internal interface ITransactionValidator
    {
        void SetValidateOptions(IReceiverSignOptions options);
        bool Validate(Transaction message);
    }
}
