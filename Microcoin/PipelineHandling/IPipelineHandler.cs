using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.PipelineHandling
{
    public interface IPipelineHandler<HandleType>
    {
        public Task<bool> Handle(HandleType handleObject);
    }
}
