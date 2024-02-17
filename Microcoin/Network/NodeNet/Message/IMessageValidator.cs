using Microcoin.RSAEncryptions;

namespace Microcoin.Network.NodeNet.Message
{
    internal interface IMessageValidator
    {
        void SetValidateOptions(IReceiverSignOptions options);
        bool Validate(Message message);
    }
}
