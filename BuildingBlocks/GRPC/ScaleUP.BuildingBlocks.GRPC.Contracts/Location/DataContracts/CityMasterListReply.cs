using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Location.DataContracts
{
    [DataContract]
    public class CityMasterListReply
    {
        [DataMember(Order = 1)]
        public long Id { get; set; }
        [DataMember(Order = 2)]
        public string CityName { get; set; }
        [DataMember(Order = 3)]
        public string StateName { get; set; }
        [DataMember(Order = 4)]
        public bool status { get; set; }
        [DataMember(Order = 5)]
        public long? stateId { get; set; }
    }
}
