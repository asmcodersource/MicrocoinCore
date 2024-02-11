using NodeNet.Message;


namespace NodeNet.ReceiveMiddleware
{
    // Message sing verification middleware
    // If message sing isn't correct than next middlewares will not be called
    internal class SignVerificationMiddleware : IReceiveMiddleware
    {
        public IReceiveMiddleware Next { get; protected set; } = null;
        public IMessageValidator MessageValidator { get; protected set; }

        public SignVerificationMiddleware(Node node, IMessageValidator messageValidator)
        {
            MessageValidator = messageValidator; 
        }

        public bool Invoke(MessageContext messageContext)
        {
            var validateOptions = NodeNet.Message.MessageValidator.GetReceiverValidateOptions(messageContext.Message);
            MessageValidator.SetValidateOptions(validateOptions);
            var signCorrect = MessageValidator.Validate(messageContext.Message);
            if (signCorrect)
                return Next != null ? Next.Invoke(messageContext) : true;
            else
                return false;
        }

        public void SetNext(IReceiveMiddleware next)
        {
            Next = next;
        }
    }
}
