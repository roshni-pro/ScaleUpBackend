using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts
{
    [DataContract]
    public class GSTDetailReply
    {
        [DataMember(Order = 1)]
        public bool status { get; set; }
        [DataMember(Order = 2)]
        public string Message { get; set; }
        [DataMember(Order = 3)]
        public string Name { get; set; }
        [DataMember(Order = 4)]
        public string ShopName { get; set; }
        [DataMember(Order = 5)]
        public string State { get; set; }
        [DataMember(Order = 6)]
        public string City { get; set; }
        [DataMember(Order = 7)]
        public string Zipcode { get; set; }
        [DataMember(Order = 8)]
        public string AddressLine1 { get; set; }
    }
}
