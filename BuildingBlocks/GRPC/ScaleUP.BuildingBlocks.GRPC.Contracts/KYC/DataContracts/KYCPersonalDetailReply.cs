using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts
{
    [DataContract]
    public class KYCPersonalDetailReply
    {
        [DataMember(Order = 1)]
        public string FirstName { get; set; }
        [DataMember(Order = 2)]
        public string LastName { get; set; }
        [DataMember(Order = 3)]
        public string FatherName { get; set; }
        [DataMember(Order = 4)]
        public string FatherLastName { get; set; }
        [DataMember(Order = 5)]
        public string DOB { get; set; }
        [DataMember(Order = 6)]
        public string Gender { get; set; }
        [DataMember(Order = 7)]
        public string AlternatePhoneNo { get; set; }
        [DataMember(Order = 8)]
        public string EmailId { get; set; }
        [DataMember(Order = 9)]
        public string TypeOfAddress { get; set; }
        [DataMember(Order = 10)]
        public string ResAddress1 { get; set; }

        [DataMember(Order = 11)]
        public string ResAddress2 { get; set; }
        [DataMember(Order = 12)]
        public string Pincode { get; set; }
        [DataMember(Order = 13)]
        public string City { get; set; }
        [DataMember(Order = 14)]
        public string State { get; set; }
        [DataMember(Order = 15)]
        public string PermanentAddressLine1 { get; set; }
        [DataMember(Order = 16)]
        public string PermanentAddressLine2 { get; set; }
        [DataMember(Order = 17)]
        public string PermanentPincode { get; set; }
        [DataMember(Order = 18)]
        public string PermanentCity { get; set; }
        [DataMember(Order = 19)]
        public string PermanentState { get; set; }
        [DataMember(Order = 20)]
        public string ResidenceStatus { get; set; }
        [DataMember(Order = 21)]
        public long? DocumentId { get; set; }
        [DataMember(Order = 22)]
        public bool Status { get; set; }
        [DataMember(Order = 23)]
        public string Message { get; set; }

    }
}
