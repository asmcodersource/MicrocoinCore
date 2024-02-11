using Microcoin.RSAEncryptions;

namespace NodeNet.Message
{
    internal interface IMessageValidator
    {
        void SetValidateOptions(IReceiverSignOptions options);
        bool Validate(Message message);
    }
}
