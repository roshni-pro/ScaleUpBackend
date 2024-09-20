using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.Interfaces.NBFC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.NBFC
{
    public class CreateLeadNBFCResponse: ICreateLeadNBFCResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}
