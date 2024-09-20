using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.Interfaces.NBFC
{
    public interface ICreateLeadNBFCResponse
    {
        public bool IsSuccess { get; set; }
        public string Message{ get; set; }
    }
}
