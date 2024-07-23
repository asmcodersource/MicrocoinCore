namespace Microcoin.PipelineHandling
{
    public interface IHandlePipeline<HandleType>
    {
        public PipelineHandleResult<HandleType> Handle(HandleType handleObject);
        public void AddHandlerToPipeline(IPipelineHandler<HandleType> handler);
        public bool RemoveHandlerFromPipeline(IPipelineHandler<HandleType> handler);
    }
}
