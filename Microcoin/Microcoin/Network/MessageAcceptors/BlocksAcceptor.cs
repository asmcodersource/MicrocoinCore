using Microcoin.Microcoin.Blockchain.Block;
using System.Text.Json;

namespace Microcoin.Microcoin.Network.MessageAcceptors
{
    public class BlocksAcceptor : IAcceptor
    {
        public event Action<Block>? BlockReceived;

        public void Handle(IBroadcastMessage message)
        {
            Block block = JsonSerializer.Deserialize<Block>(message.Payload);
            Serilog.Log.Debug($"Microcoin peer | Block({block.GetMiningBlockHash()}) accepted from network");
            BlockReceived?.Invoke(block);
        }
    }
}
