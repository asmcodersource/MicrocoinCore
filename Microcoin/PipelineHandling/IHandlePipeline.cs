using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.PipelineHandling
{
    internal interface IHandlePipeline<HandleType>
    {
        public Task<PipelineHandleResult<HandleType>> Handle(HandleType handleObject);
        public void AddHandlerToPipeline(IPipelineHandler<HandleType> handler);
        public bool RemoveHandlerFromPipeline(IPipelineHandler<HandleType> handler);
    }
}
