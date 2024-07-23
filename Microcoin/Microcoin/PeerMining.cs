using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin.Blockchain.ChainController;
using Microcoin.Microcoin.Blockchain.TransactionsPool;
using Microcoin.Microcoin.Mining;
using SimpleInjector;

namespace Microcoin.Microcoin
{
    public class PeerMining
    {
        public event Action<Microcoin.Blockchain.Block.Block>? BlockMined;
        public int MaxTransactionsPerBlock { get; set; } = 512;
        public IMiner Miner { get; protected set; }
        public TransactionsPool? LinkedTransactionsPool { get; protected set; }
        public bool IsMiningEnabled { get; protected set; } = false;
        public Container ServicesContainer { get; protected set; }

        private Thread? MiningThread;
        private CancellationTokenSource? MiningCancellationTokenSource;

        public PeerMining(Container servicesContainer)
        {
            ServicesContainer = servicesContainer;
            Miner = servicesContainer.GetInstance<IMiner>();
        }

        public void StartMining()
        {
            IsMiningEnabled = true;
        }

        public void StopMining()
        {
            IsMiningEnabled = false;
            CancelCurrentMiningProcess();
        }

        public void LinkToTransactionsPool(TransactionsPool transactionsPool)
        {
            lock (this)
            {
                LinkedTransactionsPool = transactionsPool;
            }
        }

        public bool CancelCurrentMiningProcess()
        {
            lock (this)
            {
                // I use temporary variable, to prevent some concurrency error
                var temp = MiningThread;
                if (temp is not null)
                {
                    MiningCancellationTokenSource?.Cancel();
                    return true;
                }
                return false;
            }
        }

        public void ResetBlockMiningHandler(AbstractChain chainTail, string minerWallet)
        {
            lock (this)
            {
                CancelCurrentMiningProcess();
                TryStartMineBlock(chainTail, minerWallet);
            }
        }

        public void TryStartMineBlock(AbstractChain tailChain, string minerWallet)
        {
            Serilog.Log.Verbose($"Microcoin peer | Trying to start mine new block");
            lock (this)
            {
                if (MiningThread is not null || IsMiningEnabled is not true )
                    return;
                MiningCancellationTokenSource = new CancellationTokenSource();
                StartMineBlock(tailChain, minerWallet, MiningCancellationTokenSource.Token);
            }
        }

        private void StartMineBlock(AbstractChain tailChain, string minerWallet, CancellationToken cancellationToken)
        {
            if (LinkedTransactionsPool is null)
                throw new Exception("Mining can't be initiated without linked Linked transactions pool");
            var deepTransactionsVerificator = ServicesContainer.GetInstance<IDeepTransactionsVerify>();
            var transactions = LinkedTransactionsPool.ClaimTailTransactions(tailChain, deepTransactionsVerificator, MaxTransactionsPerBlock);
            if (transactions.Count == 0)
                return;
            Microcoin.Blockchain.Block.Block block = new Microcoin.Blockchain.Block.Block();
            block.MiningBlockInfo = new MiningBlockInfo();
            block.Transactions = transactions;
            // TODO: verify this line
            MiningThread = new Thread(() => MineBlock(tailChain, minerWallet, block, cancellationToken));
            MiningThread.Start();
        }


        private void MineBlock(AbstractChain tailChain, string minerWaller, Microcoin.Blockchain.Block.Block block, CancellationToken cancellationToken)
        {
            try
            {
                (Miner as IMiner).LinkBlockToChain(tailChain, block);
                block.Hash = Miner.StartBlockMining(tailChain, block, minerWaller, cancellationToken).Result;
                MiningThread = null;
                if (cancellationToken.IsCancellationRequested is not true)
                {
                    Serilog.Log.Debug($"Microcoin peer | Mining finished block={block.GetHashCode()} id={block.MiningBlockInfo.BlockId} hash={block.Hash}");
                    Task.Run(() => BlockMined?.Invoke(block));
                } else
                {
                    Serilog.Log.Debug($"Microcoin peer | Mining cancelled block={block.GetHashCode()} id={block.MiningBlockInfo.BlockId}");
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Debug($"Microcoin peer | Mining exception block={block.GetHashCode()} id={block.MiningBlockInfo.BlockId}");
                MiningThread = null;
            }
        }
    }
}
