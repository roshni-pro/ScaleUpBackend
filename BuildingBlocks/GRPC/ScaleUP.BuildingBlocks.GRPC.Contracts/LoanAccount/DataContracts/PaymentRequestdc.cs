using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts
{
    [DataContract]
    public class PaymentRequestdc
    {
        [DataMember(Order = 1)]
        public double TransactionAmount { get; set; }

        [DataMember(Order = 2)]
        public string AnchorCompanyCode { get; set; }
        [DataMember(Order = 3)]
        public string OrderNo { get; set; }
        [DataMember(Order = 4)]
        public long LoanAccountId { get; set; }        
        [DataMember(Order = 5)]
        public double OrderAmount { get; set; }       

    }

    [DataContract]
    public class LeadProductDc
    {
        [DataMember(Order = 1)]
        public long LoanAccountId { get; set; }

        [DataMember(Order = 2)]
        public long LeadId { get; set; }
        [DataMember(Order = 3)]
        public long ProductId { get; set; }
        [DataMember(Order = 4)]
        public long AnchorCompanyId { get; set; }
        [DataMember(Order = 5)]
        public string MobileNo { get; set; }
        [DataMember(Order = 6)]
        public double CreditLimitAmount { get; set; }

        [DataMember(Order = 7)]
        public long NBFCCompanyId { get; set; }
        
        [DataMember(Order = 8)]
        public double UtilizateLimit { get; set; }
        [DataMember(Order = 9)]
        public double InvoiceAmount { get; set; }
        [DataMember(Order = 10)]
        public string AnchorName { get; set; }
        [DataMember(Order = 11)]
        public string OrderNo { get; set; }
        [DataMember(Order = 12)]
        public string CustomerName { get; set; }
        [DataMember(Order = 13)]
        public string ImageUrl { get; set; }
        [DataMember(Order = 14)]
        public long? creditDay { get; set; }
        [DataMember(Order = 15)]
        public string? TransactionStatus { get; set;}
        [DataMember(Order = 16)]
        public bool IsAccountActive { get; set; }
        [DataMember(Order = 17)]
        public bool IsBlock { get; set; }
        [DataMember(Order = 18)]
        public string? IsBlockComment { get; set; }
        [DataMember(Order = 19)]
        public string? NBFCIdentificationCode { get; set; }

    }
}
