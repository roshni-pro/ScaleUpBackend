using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Common
{
    public class ResultViewModel<T>
    {
        public T Result { get; set; }
        public bool IsSuccess { get; set; }
        public string Message{ get; set; }
    }
}
