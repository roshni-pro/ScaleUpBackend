using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts
{
    [DataContract]
    public class GetCompanyProductConfigRequest
    {
        [DataMember(Order = 1)]
        public List<long> CompanyIds {  get; set; }
        [DataMember(Order = 2)]
        public string CompanyType { get; set; }

    }


    [DataContract]
    public class GetAnchoreProductConfigRequest
    {
        [DataMember(Order = 1)]
        public long CompanyId { get; set; }
        [DataMember(Order = 2)]
        public long ProductId { get; set; }

    }

}
