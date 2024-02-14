using Microcoin.Transaction;
using Newtonsoft.Json.Linq;
using NodeNet.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.MessageAcceptors
{
    internal class BlocksAcceptor : IAcceptor
    {
        public event Action<Block.Block> BlockReceived;

        public async Task Handle(MessageContext messageContext)
        {
            JObject jsonRequestObject = JObject.Parse(messageContext.Message.Data);
            JToken? jsonBlockToken = jsonRequestObject["block"];
            if (jsonBlockToken is null)
                return;
            string blockJsonString = jsonBlockToken.ToString();
            Block.Block? block = Block.Block.ParseBlockFromJson(blockJsonString);
            if (block != null)
                BlockReceived?.Invoke(block);
        }
    }
}
