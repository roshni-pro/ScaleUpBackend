using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts
{
    [DataContract]
    public class GRPCRequest<T>
    {
        [DataMember(Order = 1)]
        public T Request { get; set; }
    }

    
   
}
