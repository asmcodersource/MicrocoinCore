using Microcoin.PipelineHandling;

namespace Microcoin.Microcoin.Blockchain.BlocksPool
{
    public class BlocksPool
    {
        public event Action<BlocksPool, Block.Block>? OnBlockReceived;
        public IHandlePipeline<Block.Block> HandlePipeline { get; set; } = new EmptyPipeline<Microcoin.Blockchain.Block.Block>();
        public HashSet<Transaction.Transaction> PresentedTransactions { get; protected set; } = new HashSet<Transaction.Transaction>();

        public async Task HandleBlock(Block.Block block)
        {
            // Handle block on verifing pipeline
            var handleResult = await Task.Run(() => HandlePipeline.Handle(block));
            if (handleResult.IsHandleSuccesful is not true)
            {
                Serilog.Log.Debug($"Microcoin peer | Block({block.GetMiningBlockHash()}) failed pass handle pipeline");
                return;
            }
            // If block succesfully pass pipeline, add it to pool
            Serilog.Log.Verbose($"Microcoin peer | Block({block.GetMiningBlockHash()}) succesfully passed handle pipeline");
            OnBlockReceived?.Invoke(this, block);
        }

        public void InitializeHandlerPipeline(IHandlePipeline<Transaction.Transaction> transactionVerifyPipeline)
        {
            HandlePipeline = new HandlePipeline<Block.Block>();
            HandlePipeline.AddHandlerToPipeline(new VerifyBlockFields());
            HandlePipeline.AddHandlerToPipeline(new VerifyBlockTransactions(transactionVerifyPipeline));
        }
    }
}
