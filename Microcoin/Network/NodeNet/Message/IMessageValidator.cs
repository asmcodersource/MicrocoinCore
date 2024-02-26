﻿using Microcoin.RSAEncryptions;

namespace Microcoin.Network.NodeNet.Message
{
    public interface IMessageValidator
    {
        void SetValidateOptions(IReceiverSignOptions options);
        bool Validate(Message message);
    }
}
