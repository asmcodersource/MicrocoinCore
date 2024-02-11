using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.PipelineHandling
{
    internal class EmptyPipeline<HandleType> : IHandlePipeline<HandleType>
    {
        public void AddHandlerToPipeline(IPipelineHandler<HandleType> handler)
        {
            throw new NotImplementedException();
        }

        public Task<PipelineHandleResult<HandleType>> Handle(HandleType handleObject)
        {
            throw new NotImplementedException();
        }

        public bool RemoveHandlerFromPipeline(IPipelineHandler<HandleType> handler)
        {
            throw new NotImplementedException();
        }
    }
}
