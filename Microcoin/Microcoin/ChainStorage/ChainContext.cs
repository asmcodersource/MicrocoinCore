using System;
using System.IO;
using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin.Blockchain.Chain;

namespace Microcoin.Microcoin.ChainStorage
{
    public class ChainContext
    {
        public string HeaderFilePath { get; protected set; }
        public MutableChain? Chain { get; protected set; } = null;
        public ChainHeader? ChainHeader { get; protected set; } = null;


        public ChainContext(string headerFilePath)
        {
            HeaderFilePath = headerFilePath;
        }

        public ChainContext(string headerFilePath, AbstractChain? chain, ChainHeader? chainHeader) : this(headerFilePath)
        {
            Chain = chain switch
            {
                ImmutableChain immutableChain => new MutableChain(immutableChain),
                MutableChain mutableChain => mutableChain,
                _ => throw new InvalidOperationException("Unexpected chain type")
            };
            this.ChainHeader = chainHeader;
        }

        public ChainHeader GetChainHeader()
        {
            if (ChainHeader is null)
                throw new Exception("Chain context is not initialized");
            UpdateChainHeader();
            return ChainHeader;
        }

        public void FetchLastPart()
        {
            try
            {
                ChainHeader = ChainHeader.LoadFromFile(HeaderFilePath);
                Chain = FetchChainFromFile(ChainHeader.ChainFilePath);
            } catch
            {
                ChainHeader = null;
                Chain = null;
            }
        }

        public void Fetch()
        {
            try
            {
                ChainHeader = ChainHeader.LoadFromFile(HeaderFilePath);
                Chain = FetchChainFromFile(ChainHeader.ChainFilePath);
                string? lastChainHeaderFile = ChainHeader.PreviousChainHeaderPath;
                var lastChainContext = this;
                while (lastChainHeaderFile is not null) {
                    var previousPartContext = new ChainContext(lastChainHeaderFile);
                    previousPartContext.FetchLastPart();
                    lastChainContext.Chain.LinkPreviousChain(previousPartContext.Chain);
                    lastChainContext = previousPartContext;
                    lastChainHeaderFile = previousPartContext.ChainHeader.PreviousChainHeaderPath;
                }
            }
            catch
            {
                ChainHeader = null;
                Chain = null;
            }
        }

        public void Push()
        {
            if (ChainHeader is null || Chain is null)
                throw new Exception("Chain context is not initialized");

            UpdateChainHeader();
            ChainHeader.StoreToFile(HeaderFilePath);
            PushChainToFile(ChainHeader.ChainFilePath);
        }

        protected void PushChainToFile(string chainFilePath)
        {
            // Right know it is simple implementation, where file rewrites
            // TODO: Implement addition of missing blocks
            using (var fileStream = File.OpenWrite(chainFilePath))
                ChainsIO.ChainStreaming.WriteChainToStream(fileStream as Stream, Chain, CancellationToken.None).Wait();
        }

        protected MutableChain FetchChainFromFile(string chainFilePath)
        {
            using (var fileStream = File.OpenRead(chainFilePath))
                return ChainsIO.ChainStreaming.ReadChainFromStream(fileStream as Stream, ChainHeader.ChainIdentifier.TailChainBlockCount, CancellationToken.None).Result as MutableChain;
        }

        /// <summary>
        /// Actions performed on a chain can change its parameters, such as the number of blocks and the complexity of the chain.
        /// Therefore, you should update the chain header before performing an action on it.
        /// </summary>
        protected void UpdateChainHeader()
        {
            if (Chain is null)
                throw new Exception("Chain context is not initialized");
            ChainIdentifier chainIdentifier = new ChainIdentifier(Chain);
            ChainHeader = new ChainHeader(chainIdentifier, ChainHeader.ChainFilePath, ChainHeader.PreviousChainHeaderPath);
        }
    }
}
