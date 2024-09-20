using MassTransit.Saga;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts
{
    [DataContract]
    public class AnchorMISListResponseDC
    {
        [DataMember(Order = 1)]
        public string? ReferenceId { get; set; }
        [DataMember(Order = 2)]
        public string? LoanID { get; set; }
        [DataMember(Order = 3)]
        public string? AnchorName { get; set; }
        [DataMember(Order = 4)]
        public string? OrderNo { get; set; }
        [DataMember(Order = 5)]
        public string? InvoiceStatus { get; set; }
        [DataMember(Order = 6)]
        public string? InvoiceNo { get; set; }
        [DataMember(Order = 7)]
        public DateTime? InvoiceDate { get; set; }
        [DataMember(Order = 8)]
        public double? InvoiceAmount { get; set; }
        [DataMember(Order = 9)]
        public string? BusinessName { get; set; }
        [DataMember(Order = 10)]
        public string? AnchorCode { get; set; }
        [DataMember(Order = 11)]
        public string? NBFCName { get; set; }
        [DataMember(Order = 12)]
        public DateTime? DisbursementDate { set; get; }
        [DataMember(Order = 13)]
        public double? DisbursalAmount { get; set; }
        [DataMember(Order = 14)]
        public double? ServiceFee { get; set; }
        [DataMember(Order = 15)]
        public double? GST { get; set; }
        [DataMember(Order = 16)]
        public string? BeneficiaryName { get; set; }
        [DataMember(Order = 17)]
        public string? BeneficiaryAccountNumber { get; set; }
        [DataMember(Order = 18)]
        public long LeadId { get; set; }
        [DataMember(Order = 19)]
        public long NBFCCompanyId { get; set; }
        [DataMember(Order = 20)]
        public long ProductId { get; set; }
        [DataMember(Order = 21)]
        public long AnchorCompanyId { get; set; }
        [DataMember(Order = 22)]
        public string? UTR { get; set; }
        [DataMember(Order = 23)]
        public string? Status { get; set; }
    }
    [DataContract]
    public class AnchorMISExcelDC
    {
        [DataMember(Order = 1)]
        public string? ReferenceId { get; set; }
        [DataMember(Order = 2)]
        public string? LoanID { get; set; }
        [DataMember(Order = 3)]
        public string? AnchorName { get; set; }
        [DataMember(Order = 4)]
        public string? OrderNo { get; set; }
        [DataMember(Order = 5)]
        public string? InvoiceStatus { get; set; }
        [DataMember(Order = 6)]
        public string? InvoiceNo { get; set; }
        [DataMember(Order = 7)]
        public string InvoiceDate { get; set; }
        [DataMember(Order = 8)]
        public double? InvoiceAmount { get; set; }
        [DataMember(Order = 9)]
        public string? BusinessName { get; set; }
        [DataMember(Order = 10)]
        public string? AnchorCode { get; set; }
        [DataMember(Order = 11)]
        public string? NBFCName { get; set; }
        [DataMember(Order = 12)]
        public string DisbursementDate { set; get; }
        [DataMember(Order = 13)]
        public double? DisbursalAmount { get; set; }
        [DataMember(Order = 14)]
        public double? ServiceFee { get; set; }
        [DataMember(Order = 15)]
        public double? GST { get; set; }
        [DataMember(Order = 16)]
        public string? BeneficiaryName { get; set; }
        [DataMember(Order = 17)]
        public string? BeneficiaryAccountNumber { get; set; }
        [DataMember(Order = 18)]
        public string? UTR { get; set; }
        [DataMember(Order = 19)]
        public string? Status { get; set; }
    }
    [DataContract]
    public class EmailAnchorMISDataJobResponse
    {
        [DataMember(Order = 1)]
        public long AnchorCompanyId { get; set; }
        [DataMember(Order = 2)]
        public List<AnchorMISExcelDC> AnchorMISExcelDatas { get; set; }
        [DataMember(Order = 3)]
        public long NBFCCompanyId { get; set; }
    }

    [DataContract]
    public class DSAMISListResponseDC
    {
        [DataMember(Order = 1)]
        public string State { get; set; }
        [DataMember(Order = 2)]
        public string Location { get; set; }
        [DataMember(Order = 3)]
        public string UserType { get; set; }
        [DataMember(Order = 4)]
        public string SalesAgentName { get; set; }
        [DataMember(Order = 5)]
        public string SalesAgentPanNo { get; set; }
        [DataMember(Order = 6)]
        public string SalesAgentCode { get; set; }
        [DataMember(Order = 7)]
        public string ScaleUpCode { get; set; }
        [DataMember(Order = 8)]
        public string CustomerName { get; set; }
        [DataMember(Order = 9)]
        public DateTime? LoginDate { get; set; }
        [DataMember(Order = 10)]
        public string LAN { get; set; }
        [DataMember(Order = 11)]
        public string Lender { get; set; }
        [DataMember(Order = 12)]
        public DateTime? DisbursedDate { get; set; }
        [DataMember(Order = 13)]
        public double SanctionAmount { get; set; }
        [DataMember(Order = 14)]
        public double DisbursedAmount { get; set; }
        [DataMember(Order = 15)]
        public double PayoutPecentage { get; set; }
        [DataMember(Order = 16)]
        public double Amount { get; set; }
        [DataMember(Order = 17)]
        public double GSTAmount { get; set; }
        [DataMember(Order = 18)]
        public double TotalAmount { get; set; }
        [DataMember(Order = 19)]
        public double TDSAmount { get; set; }
        [DataMember(Order = 20)]
        public double NetPayoutAmount { get; set; }
    }
    [DataContract]
    public class DSAMISLoanResponseDC
    {
        [DataMember(Order = 1)]
        public long SalesAgentLeadId { get; set; }
        [DataMember(Order = 2)]
        public long SalesAgentLoanAccountId { get; set; }
        [DataMember(Order = 3)]
        public string SalesAgentUserId { get; set; }
        [DataMember(Order = 4)]
        public string SalesAgentCity { get; set; }
        [DataMember(Order = 5)]
        public string SalesAgentType { get; set; }
        [DataMember(Order = 6)]
        public string SalesAgentName { get; set; }
        [DataMember(Order = 7)]
        public long CustomerLeadId { get; set; }
        [DataMember(Order = 8)]
        public long CustomerLoanAccountId { get; set; }
        [DataMember(Order = 9)]
        public string CustomerUserId { get; set; }
        [DataMember(Order = 10)]
        public string CustomerCode { get; set; }
        [DataMember(Order = 11)]
        public string CustomerName { get; set; }
        [DataMember(Order = 12)]
        public string CustomerLoanAccountNo { get; set; }
        [DataMember(Order = 13)]
        public string NBFCName { get; set; }
        [DataMember(Order = 14)]
        public DateTime? DisbursmentDate { get; set; }
        [DataMember(Order = 15)]
        public double SanctionAmount { get; set; }
        [DataMember(Order = 16)]
        public double DisbursementAmount { get; set; }
        [DataMember(Order = 17)]
        public double PayoutPercentage { get; set; }
        [DataMember(Order = 18)]
        public double PayoutAmount { get; set; }
        [DataMember(Order = 19)]
        public double GSTAmount { get; set; }
        [DataMember(Order = 20)]
        public double TotalAmount { get; set; }
        [DataMember(Order = 21)]
        public double TDSAmount { get; set; }
        [DataMember(Order = 22)]
        public double NetPayoutAmount { get; set;}
    }

}
