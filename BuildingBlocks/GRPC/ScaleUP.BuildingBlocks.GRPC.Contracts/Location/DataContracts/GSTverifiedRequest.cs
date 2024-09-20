using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Location.DataContracts
{
    [DataContract]
    public class GSTverifiedRequest
    {
        [DataMember(Order = 1)]
        public string RequestPath { get; set; }
        [DataMember(Order = 2)]
        public string RefNo { get; set; }
        [DataMember(Order = 3)]
        public string Name { get; set; }
        [DataMember(Order = 4)]
        public string ShopName { get; set; }
        [DataMember(Order = 5)]
        public string ShippingAddress { get; set; }
        [DataMember(Order = 6)]
        public string City { get; set; }
        [DataMember(Order = 7)]
        public string State { get; set; }
        [DataMember(Order = 8)]
        public string Citycode { get; set; }
        [DataMember(Order = 9)]
        public string RegisterDate { get; set; }
        [DataMember(Order = 10)]
        public string CustomerBusiness { get; set; }
        [DataMember(Order = 11)]
        public string HomeName { get; set; }
        [DataMember(Order = 12)]
        public string HomeNo { get; set; }
        [DataMember(Order = 13)]
        public string LastUpdate { get; set; }
        [DataMember(Order = 14)]
        public string PlotNo { get; set; }
        [DataMember(Order = 15)]
        public string lat { get; set; }
        [DataMember(Order = 16)]
        public string lg { get; set; }
        [DataMember(Order = 17)]
        public string Zipcode { get; set; }
        [DataMember(Order = 18)]
        public bool Delete { get; set; }
        [DataMember(Order = 19)]
        public string Active { get; set; }
        [DataMember(Order = 20)]
        public DateTime CreateDate { get; set; }
        [DataMember(Order = 21)]
        public DateTime UpdateDate { get; set; }
        [DataMember(Order = 22)]
        public bool Message { get; set; }
        [DataMember(Order = 23)]
        public long? stateId { get; set; }

    }
}
