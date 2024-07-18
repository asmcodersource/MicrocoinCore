using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin.ChainStorage;
using NodeNet.NodeNet.NetworkExplorer.Requests;
using System.Collections.Concurrent;
using NodeNet.NodeNet;
using System.Collections.Generic;
using Microcoin.Microcoin.Network.ChainFethingNetwork.FetcherSession;

namespace Microcoin.Microcoin.ChainFetcher { 
    public record FetchRequest(Blockchain.Block.Block RequestedBlock, DateTime HandleAfterTime, int NumberOfRetries);
    public record ActiveHandlingFetchRequest(FetchRequest Request, HandlingFetchRequest RequestHandler, CancellationTokenSource cts);
    public record FetchResult(HandlingFetchRequest HandlingFetchRequest, ChainContext? ChainContext);

/// <summary>
/// Part of blockchain realizations, responsible to loading long chains of blocks
/// Chain defined by two blocks on start and end of loading part
/// Loading allowen only by direct peer to peer connection
/// </summary>
public class ChainFetcher
    {
        public readonly Node CommunicationNode;
        public AbstractChain? SourceChain { get; set; } 
        public int MaxFetchQueueSize { get; set; } = 50;
        public int MaxHandlingConcurrentTask { get; set; } = 5;
        public int ChainBranchBlocksCount { get; private set; }
        public int MinutesBetweenRetries { get; private set; } = 5;
        public ChainProvidersRating ChainProvidersRating { get; protected set; } = new ChainProvidersRating();
        public ChainVerificator ChainVerificator { get; protected set; }

        private HashSet<Blockchain.Block.Block> BlocksInFetchSystem = new HashSet<Blockchain.Block.Block>();
        private List<ActiveHandlingFetchRequest> HandlingRequests = new List<ActiveHandlingFetchRequest>();
        private LinkedList<FetchRequest> RequestLinkedList = new LinkedList<FetchRequest>();

        public event Action<MutableChain> ChainFetchCompleted;
        public event Action<Block> ChainFetchFail;

        public ChainFetcher(Node communcationNode, ChainVerificator chainVerificator)
        {
            CommunicationNode = communcationNode;
        }

        public void SetChainBranchValue(int chainBranchBlocksCount)
        {
            if (chainBranchBlocksCount <= 0)
                throw new ArgumentException("Chain branch blocks count must be more than zero");
            ChainBranchBlocksCount = chainBranchBlocksCount;
        }

        public bool RequestChainFetch(Microcoin.Blockchain.Block.Block block)
        {
            return RequestChainFetch(block, DateTime.UtcNow);
        }

        public bool RequestChainFetch(Microcoin.Blockchain.Block.Block block, DateTime handleTime)
        {
            // Lets assume this part of code as transaction
            // It prevent from adding more than 'MaxFetchQueueSize' requests
            lock (this)
            {
                if (RequestLinkedList.Count >= MaxFetchQueueSize)
                    return false;
                if (BlocksInFetchSystem.Contains(block))
                    return false;
                var newFetchRequest = new FetchRequest(block, handleTime, 5);
                BlocksInFetchSystem.Add(block);
                AddFetchRequestToList(newFetchRequest);
                return true;
            }
        }

        public bool HandleNextRequest()
        {
            // Get first request, if it ready, then handle it, in other case return false
            lock (this)
            {
                if (RequestLinkedList.Count == 0)
                    return false;
                FetchRequest peekFetchRequest = RequestLinkedList.First();
                if( peekFetchRequest.HandleAfterTime >= DateTime.UtcNow )
                    return false;
                RequestLinkedList.RemoveFirst();

                // Create task that will handle this request
                var newHandlingRequest = new ActiveHandlingFetchRequest(
                    peekFetchRequest,
                    new HandlingFetchRequest(peekFetchRequest, ChainBranchBlocksCount),
                    new CancellationTokenSource()
                );
                if (SourceChain is null)
                    throw new Exception("Source chain isn't initialized");
                newHandlingRequest.RequestHandler.ChainIsntFetched += () => OnChainFetchFaulted(newHandlingRequest);
                newHandlingRequest.RequestHandler.ChainFetched += async (result) => await OnNewResult(newHandlingRequest, result);
                newHandlingRequest.RequestHandler.SessionFinishedSuccesful += (connection) => ChainProvidersRating.ChainFetchSuccesful(connection.OppositeSidePublicKey);
                newHandlingRequest.RequestHandler.SessionFinishedFaulty += (connection) => ChainProvidersRating.ChainFetchFailed(connection.OppositeSidePublicKey);
                Task.Run(() => newHandlingRequest.RequestHandler.StartHandling(CommunicationNode, SourceChain, newHandlingRequest.cts.Token));
                HandlingRequests.Add(newHandlingRequest);
                return true;
            }
        }

        /// <summary>
        /// It is necessary to add a new request to the list, in the correct place, 
        /// which is determined by the time after which it should be processed (in ascending order).
        /// </summary>
        private void AddFetchRequestToList(FetchRequest fetchRequest)
        {
            if(RequestLinkedList.Count == 0)
            {
                RequestLinkedList.AddFirst(fetchRequest);
                return;
            }
            var enumerator = RequestLinkedList.GetEnumerator();
            FetchRequest? insertTargetRequest = null;
            while (enumerator.MoveNext())
            {
                if (enumerator.Current.HandleAfterTime >= fetchRequest.HandleAfterTime)
                {
                    insertTargetRequest = enumerator.Current;
                    break;
                }
            }
            if( insertTargetRequest is not null )
                RequestLinkedList.AddBefore(new LinkedListNode<FetchRequest>(insertTargetRequest), fetchRequest);
            else 
                RequestLinkedList.AddLast(fetchRequest);
        }

        private async Task OnNewResult(ActiveHandlingFetchRequest finishedFetchRequest, ChainDownloadingResult result)
        {
            if (await VerifyFetchResult(result) is true)
                OnChainFetchCompleted(finishedFetchRequest, result);
        }

        private void OnChainFetchCompleted(ActiveHandlingFetchRequest finishedFetchRequest, ChainDownloadingResult result)
        {
            lock (this)
            {
                foreach (var handlingRequest in HandlingRequests)
                {
                    if (handlingRequest == finishedFetchRequest)
                        continue;
                    if (handlingRequest.Request.RequestedBlock.MiningBlockInfo.BlockId <= result.DownloadedChain.EntireChainLength)
                    {
                        BlocksInFetchSystem.Remove(handlingRequest.Request.RequestedBlock);
                        handlingRequest.cts.Cancel();
                    }
                    
                }
                var requestToRemove = RequestLinkedList.Where(request => request.RequestedBlock.MiningBlockInfo.BlockId <= result.DownloadedChain.EntireChainLength);
                RequestLinkedList = new LinkedList<FetchRequest>(RequestLinkedList.Except(requestToRemove));
                BlocksInFetchSystem.Remove(finishedFetchRequest.Request.RequestedBlock);
            }
            ChainFetchCompleted?.Invoke(result.DownloadedChain);
        }

        private void OnChainFetchFaulted(ActiveHandlingFetchRequest finishedFetchRequest)
        {
            if ( finishedFetchRequest.Request.NumberOfRetries > 0)
            {
                var fetchRequest = new FetchRequest(
                    finishedFetchRequest.Request.RequestedBlock,
                    DateTime.UtcNow + new TimeSpan(0, MinutesBetweenRetries, 0),
                    finishedFetchRequest.Request.NumberOfRetries - 1
                );
                lock (this)
                    AddFetchRequestToList(fetchRequest);
            } else
            {
                ChainFetchFail?.Invoke(finishedFetchRequest.Request.RequestedBlock);
            }
        }

        private async Task<bool> VerifyFetchResult(ChainDownloadingResult result)
        {
            return await ChainVerificator.VerifyChain(result.DownloadedChain, result.LastBlockFromSource);
        }
    }
}
