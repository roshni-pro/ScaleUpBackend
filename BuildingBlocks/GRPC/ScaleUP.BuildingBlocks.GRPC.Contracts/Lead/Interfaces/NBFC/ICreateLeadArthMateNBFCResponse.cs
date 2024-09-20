using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.Interfaces.NBFC
{
    public interface ICreateLeadArthMateNBFCResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
    }
}
