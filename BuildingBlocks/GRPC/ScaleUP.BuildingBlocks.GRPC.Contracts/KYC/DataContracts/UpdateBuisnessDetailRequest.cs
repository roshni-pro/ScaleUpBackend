using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts
{
    [DataContract]
    public class UpdateBuisnessDetailRequest
    {
        [DataMember(Order = 1)]
        public long LeadMasterId { get; set; }
        [DataMember(Order = 2)]
        public string BusName { get; set; }
        [DataMember(Order = 3)]
        public string DOI { get; set; }
        [DataMember(Order = 4)]
        public string? BusGSTNO { get; set; }
        [DataMember(Order = 5)]
        public string BusEntityType { get; set; }
        [DataMember(Order = 6)]
        public double? BuisnessMonthlySalary { get; set; }
        [DataMember(Order = 7)]
        public string IncomeSlab { get; set; }
        [DataMember(Order = 8)]
        public string? BuisnessDocumentNo { get; set; }
        [DataMember(Order = 9)]
        public double? InquiryAmount { get; set; }
        [DataMember(Order = 10)]
        public string? SurrogateType { get; set; }
        [DataMember(Order = 11)]
        public string? UserId { get; set; }
        [DataMember(Order = 12)]
        public string? ProductCode { get; set;}
        [DataMember(Order = 13)]
        public string BusAddCorrLine1 { get; set; }
        [DataMember(Order = 14)]
        public string BusAddCorrLine2 { get; set; }
        [DataMember(Order = 15)]
        public string BusAddCorrPincode { get; set; }
        [DataMember(Order = 16)]
        public string BusAddCorrCity { get; set; }
        [DataMember(Order = 17)]
        public string? BusAddCorrState { get; set; }
        [DataMember(Order = 18)]
        public long AddressId { get; set; }
        [DataMember(Order = 19)]
        public long? CurrentAddressId { get; set; }
    }
}
