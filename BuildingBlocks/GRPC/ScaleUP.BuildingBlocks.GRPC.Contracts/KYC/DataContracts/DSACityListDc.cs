using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts
{
    [DataContract]
    public class DSACityListDc
    {
        [DataMember(Order =1)]
        public long CityId { get; set; }
        [DataMember(Order =2)]
        public string CityName { get; set; }
   
    }
}
