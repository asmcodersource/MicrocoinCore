﻿using NodeNet.NodeNet.Message;
using Newtonsoft.Json.Linq;

namespace Microcoin.Microcoin.Network.MessageAcceptors
{
    public class BlocksAcceptor : IAcceptor
    {
        public event Action<Blockchain.Block.Block>? BlockReceived;

        public virtual async Task Handle(MessageContext messageContext)
        {
            JObject jsonRequestObject = JObject.Parse(messageContext.Message.Data);
            JToken? jsonBlockToken = jsonRequestObject["block"];
            if (jsonBlockToken is null)
                return;
            string blockJsonString = jsonBlockToken.ToString();
            Blockchain.Block.Block? block = Blockchain.Block.Block.ParseBlockFromJson(blockJsonString);
            if (block != null)
            {
                BlockReceived?.Invoke(block);
                Serilog.Log.Debug($"Microcoin peer | Block({block.GetMiningBlockHash()}) accepted from network");
            }
        }
    }
}
