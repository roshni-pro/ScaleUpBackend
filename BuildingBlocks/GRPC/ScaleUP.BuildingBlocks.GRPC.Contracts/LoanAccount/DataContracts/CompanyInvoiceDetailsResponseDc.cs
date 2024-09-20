using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts
{
    [DataContract]
    public class CompanyInvoiceDetailsResponseDc
    {
        [DataMember(Order = 1)]
        public string AnchorCode { get; set; }

        [DataMember(Order = 2)]
        public string AnchorName { get; set; }
        [DataMember(Order = 3)]
        public string? NBFCName { get; set; }
        [DataMember(Order = 4)]
        public string? InvoiceNo { get; set; }
        [DataMember(Order = 5)]
        public DateTime? InvoiceDate { get; set; }
        [DataMember(Order = 6)]
        public double ProcessingFeeTotal { get; set; }
        [DataMember(Order = 7)]
        public double InterestTotal { get; set; }
        [DataMember(Order = 8)]
        public double OverDueInterestTotal { get; set; }
        [DataMember(Order = 9)]
        public double PenalTotal { get; set; }
        [DataMember(Order = 10)]
        public double BounceTotal { get; set; }
        [DataMember(Order = 11)]
        public double InvoiceAmount { get; set; }
        [DataMember(Order = 12)]
        public double ScaleupShare { get; set; }
        [DataMember(Order = 13)]
        public long? NBFCCompanyId { get; set; }
        [DataMember(Order = 14)]
        public double TotalAmount { get; set; }
        [DataMember(Order = 15)]
        public string ReferenceNo { get; set; }
        [DataMember(Order = 16)]
        public int Status { get; set; }
        [DataMember(Order = 17)]
        public long CompanyInvoiceId { get; set; }
        [DataMember(Order = 18)]
        public long AnchorCompanyId { get; set; }
        [DataMember(Order = 19)]
        public long AccountTransactionId { get; set; }
        [DataMember(Order = 20)]
        public bool IsActive { get; set; }
        [DataMember(Order = 21)]
        public string StatusName { get; set; }
        [DataMember(Order = 22)]
        public bool IsCheckboxVisible { get; set; }
        [DataMember(Order = 23)]
        public double GST { get; set; }
        [DataMember(Order = 24)]
        public string InvoiceUrl { get; set; }
        [DataMember(Order = 25)]
        public double ProcessingFeeScaleupShare { get; set; }
        [DataMember(Order = 26)]
        public double InterestScaleupShare { get; set; }
        [DataMember(Order = 27)]
        public double OverDueInterestScaleupShare { get; set; }
        [DataMember(Order = 28)]
        public double PenalScaleupShare { get; set; }
        [DataMember(Order = 29)]
        public double BounceScaleupShare { get; set; }
        [DataMember(Order = 30)]
        public string topUpNumber { get; set; }
        [DataMember(Order = 31)]
        public string ThirdPartyTxnId { get; set;}

    }
}
