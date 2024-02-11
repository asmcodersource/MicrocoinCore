using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.PipelineHandling
{
    internal record PipelineHandleResult<HandleType>
    {
        public IPipelineHandler<HandleType> LastExecutedHandle { get; set; } = null;
        public bool IsHandleSuccesful { get; set; } = false;
    }
}
