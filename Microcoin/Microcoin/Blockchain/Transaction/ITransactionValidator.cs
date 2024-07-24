﻿using Microcoin.Microcoin.RSAEncryptions;

namespace Microcoin.Microcoin.Blockchain.Transaction
{
    public interface ITransactionValidator
    {
        void SetValidateOptions(IReceiverSignOptions options);
        bool Validate(Transaction message);
    }
}
