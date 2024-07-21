using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin.ChainStorage;
using NodeNet.NodeNet.NetworkExplorer.Requests;
using System.Collections.Concurrent;
using NodeNet.NodeNet;
using System.Collections.Generic;
using Microcoin.Microcoin.Mining;
using Microcoin.Microcoin.Network.ChainFethingNetwork.FetcherSession;
using SimpleInjector;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Microcoin.Microcoin.ChainFetcher
{
    public record FetchRequest(Block RequestedBlock, DateTime HandleAfterTime, int NumberOfRetries);
    public record ActiveHandlingFetchRequest(FetchRequest Request, HandlingFetchRequest RequestHandler, CancellationTokenSource cts);
    public record FetchResult(HandlingFetchRequest HandlingFetchRequest, ChainContext? ChainContext);

    public class ChainFetcher
    {
        public Node CommunicationNode { get; }
        public AbstractChain? SourceChain { get; set; }
        public int MaxFetchQueueSize { get; set; } = 1000;
        public int MaxHandlingConcurrentTask { get; set; } = 1000;
        public int ChainBranchBlocksCount { get; private set; } = 5;
        public int MinutesBetweenRetries { get; private set; } = 10;
        public ChainProvidersRating ChainProvidersRating { get; protected set; } = new ChainProvidersRating();
        public ChainVerificator ChainVerificator { get; protected set; }

        private readonly Timer _tryAcceptNextRequestTimer;
        private readonly HashSet<Block> _blocksInFetchSystem = new();
        private readonly List<ActiveHandlingFetchRequest> _handlingRequests = new();
        private LinkedList<FetchRequest> _requestLinkedList = new();

        public event Action<MutableChain>? ChainFetchCompleted;
        public event Action<Block>? ChainFetchFail;

        public ChainFetcher(Container servicesContainer)
        {
            CommunicationNode = servicesContainer.GetInstance<Node>();
            ChainVerificator = new ChainVerificator(servicesContainer);
            _tryAcceptNextRequestTimer = new Timer(_ => TryHandleNextRequest(), null, 0, 1000);
        }

        public void SetChainBranchValue(int chainBranchBlocksCount)
        {
            if (chainBranchBlocksCount <= 0)
                throw new ArgumentException("Chain branch blocks count must be more than zero");
            ChainBranchBlocksCount = chainBranchBlocksCount;
        }

        public void ChangeSourceChain(AbstractChain sourceChain)
        {
            SourceChain = sourceChain;
        }

        public bool RequestChainFetch(Block block)
        {
            return RequestChainFetch(block, DateTime.UtcNow);
        }

        public bool RequestChainFetch(Block block, DateTime handleTime)
        {
            lock (this)
            {
                if (_requestLinkedList.Count >= MaxFetchQueueSize || _blocksInFetchSystem.Contains(block))
                    return false;

                var newFetchRequest = new FetchRequest(block, handleTime, 3);
                _blocksInFetchSystem.Add(block);
                AddFetchRequestToList(newFetchRequest);
                return true;
            }
        }

        public void TryHandleNextRequest()
        {
            try
            {
                while (HandleNextRequest() && _handlingRequests.Count < MaxHandlingConcurrentTask) ;
            }
            catch (Exception e)
            {
                Serilog.Log.Error(e.Message);
            }
        }

        public bool HandleNextRequest()
        {
            lock (this)
            {
                if (!_requestLinkedList.Any())
                    return false;

                var peekFetchRequest = _requestLinkedList.First!.Value;
                if (peekFetchRequest.HandleAfterTime >= DateTime.UtcNow)
                    return false;

                _requestLinkedList.RemoveFirst();

                if (SourceChain == null)
                    throw new Exception("Source chain isn't initialized");

                var newHandlingRequest = new ActiveHandlingFetchRequest(
                    peekFetchRequest,
                    new HandlingFetchRequest(peekFetchRequest, ChainBranchBlocksCount, ChainProvidersRating),
                    new CancellationTokenSource()
                );

                Serilog.Log.Debug($"Chain fetch started {newHandlingRequest.Request.RequestedBlock.GetHashCode()} {newHandlingRequest.Request.RequestedBlock.Hash}");

                newHandlingRequest.RequestHandler.ChainIsntFetched += () => OnChainFetchFaulted(newHandlingRequest);
                newHandlingRequest.RequestHandler.ChainFetched += async result => await OnNewResult(newHandlingRequest, result);
                newHandlingRequest.RequestHandler.SessionFinishedSuccesful += provider => ChainProvidersRating.ChainFetchSuccesful(provider);
                newHandlingRequest.RequestHandler.SessionFinishedFaulty += provider => ChainProvidersRating.ChainFetchFailed(provider);

                Task.Run(() => newHandlingRequest.RequestHandler.StartHandling(CommunicationNode, SourceChain, newHandlingRequest.cts.Token));
                _handlingRequests.Add(newHandlingRequest);
                return true;
            }
        }

        private void AddFetchRequestToList(FetchRequest fetchRequest)
        {
            lock (this)
            {
                var current = _requestLinkedList.First;
                while (current != null && current.Value.HandleAfterTime < fetchRequest.HandleAfterTime)
                    current = current.Next;

                if (current != null)
                    _requestLinkedList.AddBefore(current, fetchRequest);
                else
                    _requestLinkedList.AddLast(fetchRequest);
            }
        }

        private async Task OnNewResult(ActiveHandlingFetchRequest finishedFetchRequest, ChainDownloadingResult result)
        {
            if (await VerifyFetchResult(result))
                CompleteFetchRequest(finishedFetchRequest, result);
        }

        private void CompleteFetchRequest(ActiveHandlingFetchRequest finishedFetchRequest, ChainDownloadingResult result)
        {
            Serilog.Log.Debug($"Chain fetched successfully {finishedFetchRequest.Request.RequestedBlock.GetHashCode()} {finishedFetchRequest.Request.RequestedBlock.Hash}");
            ChainFetchCompleted?.Invoke(result.DownloadedChain);

            RemoveHandledRequests(result.DownloadedChain.EntireChainLength);
            RemoveFetchRequest(finishedFetchRequest.Request.RequestedBlock);
        }

        private void OnChainFetchFaulted(ActiveHandlingFetchRequest finishedFetchRequest)
        {
            if (finishedFetchRequest.Request.NumberOfRetries > 0)
            {
                RetryFetchRequest(finishedFetchRequest);
            }
            else
            {
                Serilog.Log.Debug($"Chain fetch faulted {finishedFetchRequest.Request.RequestedBlock.GetHashCode()} {finishedFetchRequest.Request.RequestedBlock.Hash}");
                RemoveFetchRequest(finishedFetchRequest.Request.RequestedBlock);
                ChainFetchFail?.Invoke(finishedFetchRequest.Request.RequestedBlock);
            }
        }

        private void RetryFetchRequest(ActiveHandlingFetchRequest finishedFetchRequest)
        {
            var fetchRequest = new FetchRequest(
                finishedFetchRequest.Request.RequestedBlock,
                DateTime.UtcNow.AddMinutes(MinutesBetweenRetries),
                finishedFetchRequest.Request.NumberOfRetries - 1
            );

            Serilog.Log.Debug($"Chain fetch retry {finishedFetchRequest.Request.RequestedBlock.GetHashCode()} {finishedFetchRequest.Request.RequestedBlock.Hash}");
            RemoveFetchRequest(finishedFetchRequest);
            AddFetchRequestToList(fetchRequest);
        }

        private void RemoveHandledRequests(int chainComplexity)
        {
            lock (this)
            {
                foreach (var handlingRequest in _handlingRequests.ToList())
                {
                    if (handlingRequest.Request.RequestedBlock.MiningBlockInfo.ChainComplexity <= chainComplexity)
                    {
                        _blocksInFetchSystem.Remove(handlingRequest.Request.RequestedBlock);
                        _handlingRequests.Remove(handlingRequest);
                        handlingRequest.cts.Cancel();
                    }
                }
            }
        }

        private void RemoveFetchRequest(ActiveHandlingFetchRequest handlingFetchRequest)
        {
            lock (this)
            {
                _handlingRequests.Remove(handlingFetchRequest);
            }
        }

        private void RemoveFetchRequest(Block block)
        {
            lock (this)
            {
                _blocksInFetchSystem.Remove(block);
            }
        }

        private async Task<bool> VerifyFetchResult(ChainDownloadingResult result)
        {
            try
            {
                return await ChainVerificator.VerifyChain(result.DownloadedChain, result.LastBlockFromSource);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error($"Verify fetch result exception: {ex.Message}");
                throw;
            }
        }
    }
}
