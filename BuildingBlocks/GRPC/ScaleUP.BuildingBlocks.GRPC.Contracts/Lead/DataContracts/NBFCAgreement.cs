using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class NBFCAgreement
    {
        [DataMember(Order = 1)]
        public string NameOfBorrower { get; set; }
        [DataMember(Order = 2)]
        public string FatherName { get; set; }
        [DataMember(Order = 3)]
        public string GstNumber { get; set; }
        [DataMember(Order = 4)]
        public double CreditLimit { get; set; }
        [DataMember(Order = 5)]
        public double TransactionConvenienceFeeRate { get; set; }
        [DataMember(Order = 6)]
        public double ProcessingFees { get; set; }
        [DataMember(Order = 7)]
        public double NetPF { get; set; }
        [DataMember(Order = 8)]
        public double DelayPenaltyFeeRate { get; set; }
        [DataMember(Order = 9)]
        public double BounceCharge { get; set; }
        [DataMember(Order = 10)]
        public DateTime ApplicationDate { get; set; }
        [DataMember(Order = 11)]
        public string address1 { get; set; }
        [DataMember(Order = 12)]
        public string address2 { get; set; }
        [DataMember(Order = 13)]
        public long CustomerAgreementDocId { get; set; }

        [DataMember(Order = 14)]
        public string CustomerAgreementURL { get; set; }
        [DataMember(Order = 15)]
        public bool IsAccept { get; set; }
        [DataMember(Order = 16)]
        public string LeadCode { get; set;}
        [DataMember(Order = 17)]
        public string CompanyLogo { get; set;}
        [DataMember(Order = 18)]
        public string ProcessingFeeType { get; set; }
        [DataMember(Order = 19)]
        public double GSTRate { get;set; }
        [DataMember(Order = 20)]
        public double DiscountAmount { get; set; }

    }
}
