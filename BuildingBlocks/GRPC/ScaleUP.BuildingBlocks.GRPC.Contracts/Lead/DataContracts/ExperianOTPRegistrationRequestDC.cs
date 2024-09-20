using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class ExperianOTPRegistrationRequestDC
    {
        [DataMember(Order = 1)]
        public string firstName { get; set; }
        [DataMember(Order = 2)]
        public string surName { get; set; }
        [DataMember(Order = 3)]
        public string mobileNo { get; set; }
        [DataMember(Order = 4)]
        public string email { get; set; }
        [DataMember(Order = 5)]
        public string dateOfBirth { get; set; }
        [DataMember(Order = 6)]
        public int? gender { get; set; }
        [DataMember(Order = 7)]
        public string flatno { get; set; }
        [DataMember(Order = 8)]
        public string city { get; set; }
        [DataMember(Order = 9)]
        public long? state { get; set; }
        [DataMember(Order = 10)]
        public string pincode { get; set; }
        [DataMember(Order = 11)]
        public string pan { get; set; }
        [DataMember(Order = 12)]
        public string? middleName { get; set; }
        [DataMember(Order = 13)]
        public string? passport { get; set; }
        [DataMember(Order = 14)]
        public string? aadhaar { get; set; }
        [DataMember(Order = 15)]
        public long LeadId { get; set; }
        [DataMember(Order = 16)]
        public long activityId { get; set; }
        [DataMember(Order = 17)]
        public long? subActivityId { get; set; }
        [DataMember(Order = 18)]
        public long companyId { get; set; }
        [DataMember(Order = 19)]
        public long? experianState { get; set; }
        [DataMember(Order = 20)]
        public long? cityId { get; set; }

    }
}
