using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin.Blockchain.Block;
using System.Text.Json;


namespace Microcoin.Microcoin.ChainsIO
{
    public static class ChainStreaming
    {
        public static async Task<AbstractChain> ReadChainFromStream(Stream stream, int chainsBlocks, CancellationToken cancellationToken)
        {
            var jsonStreamParser = new Json.JsonStreamParser();
            var chain = new MutableChain();
            for (int i = 0; i < chainsBlocks; i++)
            {
                var chainBlock = (await jsonStreamParser.ParseJsonObject(stream, cancellationToken)).Deserialize<Block>();
                if (chainBlock is null)
                    throw new Exception("Deserialized object has wrong type, must be 'Block'");
                chain.AddTailBlock(chainBlock);
            }
            return chain;
        }

        public static async Task<List<Block>> ReadBlocksFromStream(Stream stream, int blocksCount, CancellationToken cancellationToken)
        {
            var jsonStreamParser = new Json.JsonStreamParser();
            var blocks = new List<Block>();
            for (int i = 0; i < blocksCount; i++)
            {
                var block = (await jsonStreamParser.ParseJsonObject(stream, cancellationToken)).Deserialize<Block>();
                if (block is null)
                    throw new Exception("Deserialized object has wrong type, must be 'Block'");
                blocks.Add(block);
            }   
            return blocks;
        }

        public static async Task WriteChainToStream(Stream stream, AbstractChain chain, CancellationToken cancellationToken)
        {
            StreamWriter streamWriter = new StreamWriter(stream);
            var blocks = chain.GetBlocksList();
            await WriteBlocksToStream(stream, blocks, cancellationToken);
        }

        public static async Task WriteBlocksToStream(Stream stream, IReadOnlyCollection<Block> blocks, CancellationToken cancellationToken)
        {
            StreamWriter streamWriter = new StreamWriter(stream);
            foreach (var block in blocks)
                await ChainSerialization.SerilizeBlockToStream(block, streamWriter, cancellationToken);
            await streamWriter.FlushAsync();
        }
    }
}
