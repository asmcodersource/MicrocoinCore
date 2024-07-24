namespace Microcoin.Microcoin.PipelineHandling
{
    public interface IPipelineHandler<HandleType>
    {
        public bool Handle(HandleType handleObject);
    }
}
