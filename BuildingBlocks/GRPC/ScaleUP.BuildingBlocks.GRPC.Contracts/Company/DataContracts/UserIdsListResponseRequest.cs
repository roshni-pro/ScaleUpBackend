using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts
{
    [DataContract]
    public class UserIdsListResponseRequest
    {

            [DataMember(Order = 1)]
            public List<long> CompanyIds { get; set; }

    }
}
