using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts
{
    [DataContract]
    public class CompanyInvoicesListResponseDc
    {
        [DataMember(Order = 1)]
        public string? NBFCName { get; set; }
        [DataMember(Order = 2)]
        public string InvoiceNo { get; set; }
        [DataMember(Order = 3)]
        public DateTime InvoiceDate { get; set; }
        [DataMember(Order = 4)]
        public int Status { get; set; }
        [DataMember(Order = 5)]
        public DateTime? PaymentDate { get; set; }
        [DataMember(Order = 6)]
        public string? PaymentRefNo { get; set; }
        [DataMember(Order = 7)]
        public long? NBFCCompanyId { get;set; }
        [DataMember(Order = 8)]
        public int TotalCount { get;set; }
        [DataMember(Order = 9)]
        public string ReferenceNo { get; set; }
        [DataMember(Order = 10)]
        public double Amount { get; set; }
        [DataMember(Order = 11)]
        public string InvoiceUrl { get; set; }
        [DataMember(Order = 12)]
        public string StatusName { get; set; }
        [DataMember(Order = 13)]
        public string CompanyEmail { get; set; }
        [DataMember(Order = 14)]
        public DateTime CreatedDate { get; set; }
        [DataMember(Order = 15)]
        public string UserType { get; set; }
        [DataMember(Order = 16)]
        public long CompanyInvoiceId { get; set; }
    }
}
