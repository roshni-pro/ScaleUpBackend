using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Location.DataContracts
{
    [DataContract]
    public class StateReply
    {

        [DataMember(Order = 1)]
        public long stateId { get; set; }

        [DataMember(Order = 2)]
        public string stateName { get; set; }
    }


    [DataContract]
    public class CityReply
    {

        [DataMember(Order = 1)]
        public long cityId { get; set; }

        [DataMember(Order = 2)]
        public string cityName { get; set; }
    }

    [DataContract]
    public class CityRequest
    {
        [DataMember(Order = 1)]
        public string cityName { get; set; }
        [DataMember(Order = 2)]
        public long stateId { get; set; }

    }
    [DataContract]
    public class AddCityRequest
    {
        [DataMember(Order = 1)]
        public string StateName { get; set; }
        [DataMember(Order = 2)]
        public string CityName { get; set; }
    }
}
