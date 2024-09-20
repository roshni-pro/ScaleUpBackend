using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts
{
    [DataContract]
    public class UpdateCompanyInvoiceDetailsRequestDC
    {
        [DataMember(Order = 1)]
        public long AccountTransactionId { get; set; }
        [DataMember(Order = 2)]
        public bool Active { get; set; }
    }

    [DataContract]
    public class UpdateCompanyInvoiceRequestDC
    {
        [DataMember(Order = 1)]
        public long CompanyInvoiceId { get; set; }
        [DataMember(Order = 2)]
        public string UserType { get; set; }
        [DataMember(Order = 3)]
        public bool IsApproved { get; set; }
        [DataMember(Order = 4)]
        public List<UpdateCompanyInvoiceDetailsRequestDC> UpdateCompanyInvoiceDetailsRequestDC { get; set; }
        [DataMember(Order = 5)]
        public string UserId { get; set; }
    }

    [DataContract]
    public class UpdateCompanyInvoicePDFDC
    {
        [DataMember(Order = 1)]
        public long CompanyInvoiceId { get; set; }
        [DataMember(Order = 2)]
        public string PDFPath { get; set; }
    }

    [DataContract]
    public class UpdateCompanyInvoiceReplyDC
    {
        [DataMember(Order = 1)]
        public bool IsPDFGenerate { get; set; }
        [DataMember(Order = 2)]
        public InvoicePdfdetailDC InvoicePdfdetailDC { get; set; }
    }
    [DataContract]
    public class InvoicePdfdetailDC
    {
        [DataMember(Order = 1)]
        public string InvoiceNo { get; set; }
        [DataMember(Order = 2)]
        public string InvoiceDate { get; set; }
        [DataMember(Order = 3)]
        public string DueDate { get; set; }
        [DataMember(Order = 4)]
        public string InvoicePeriodFrom { get; set; }
        [DataMember(Order = 5)]
        public string InvoicePeriodTo { get; set; }
        [DataMember(Order = 6)]
        public string InvoiceParticulars { get; set; }
        [DataMember(Order = 7)]
        public double InvoiceRate { get; set; }
        [DataMember(Order = 8)]
        public double InvoiceTaxableValue { get; set; }
        [DataMember(Order = 9)]
        public long NBFCCompanyId { get; set; }
    }

    [DataContract]
    public class SendInvoiceEmailDC
    {
        [DataMember(Order = 1)]
        public List<string> To { get; set; }
        [DataMember(Order = 2)]
        public string InvoiceUrl { get; set; }
        [DataMember(Order = 3)]
        public string Subject { get; set; }
        [DataMember(Order = 4)]
        public string Body { get; set; }
    }


    [DataContract]
    public class GetInvoiceRegisterDataRequest
    {
        [DataMember(Order = 1)]
        public DateTime StartDate { get; set; }
        [DataMember(Order = 2)]
        public DateTime EndDate { get; set; }
    }

    [DataContract]
    public class GetInvoiceRegisterDataResponse
    {
        [DataMember(Order = 1)]
        public string InvoiceDate { get; set; }
        [DataMember(Order = 2)]
        public string InvoiceNumber { get; set; }
        [DataMember(Order = 3)]
        public string Month { get; set; }
        [DataMember(Order = 4)]
        public string CustomerName { get; set; }
        [DataMember(Order = 5)]
        public string CustomerCityState { get; set; }
        [DataMember(Order = 6)]
        public string CustomerGSTIN { get; set; }
        [DataMember(Order = 7)]
        public string Description { get; set; }
        [DataMember(Order = 8)]
        public string SAC { get; set; }
        [DataMember(Order = 9)]
        public double TaxableAmount { get; set; }
        [DataMember(Order = 10)]
        public double CGSTPercentage { get; set; }
        [DataMember(Order = 11)]
        public double SGSTPercentage { get; set; }
        [DataMember(Order = 12)]
        public double IGSTPercentage { get; set; }
        [DataMember(Order = 13)]
        public double CGSTAmount { get; set; }
        [DataMember(Order = 14)]
        public double SGSTAmount { get; set; }
        [DataMember(Order = 15)]
        public double IGSTAmount { get; set; }
        [DataMember(Order = 16)]
        public double InvoiceAmount { get; set; }
        [DataMember(Order = 17)]
        public double TDSAmount { get; set; }
        [DataMember(Order = 18)]
        public double NetReceivable { get; set; }
        [DataMember(Order = 19)]
        public string Status { get; set; }
        [DataMember(Order = 20)]
        public long NBFCCompanyId { get; set; }
    }

    [DataContract]
    public class SettleCompanyInvoiceTransactionsRequest
    {
        [DataMember(Order = 1)]
        public List<InvoiceSettlementDataDc> InvoiceSettlementDatas { get; set; }
        [DataMember(Order = 2)]
        public long CompanyInvoiceId { get; set; }
    }

    [DataContract]
    public class InvoiceSettlementDataDc
    {
        [DataMember(Order = 1)]
        public double Amount { get; set; }
        [DataMember(Order = 2)]
        public double TDSAmount { get; set; }
        [DataMember(Order = 3)]
        public DateTime PaymentDate { get; set; }
        [DataMember(Order = 4)]
        public string UTRNumber { get; set; }
        [DataMember(Order = 5)]
        public bool IsSettled { get; set; }
        [DataMember(Order = 6)]
        public long Id { get; set; }
    }

    [DataContract]
    public class GetSettlePaymentJobDataResponse
    {
        [DataMember(Order = 1)]
        public long AnchorCompanyId { get; set; }
        [DataMember(Order = 2)]
        public long NBFCCompanyId { get; set; }
        [DataMember(Order = 3)]
        public long ProductId { get; set; }
    }

    [DataContract]
    public class SettlePaymentJobRequest
    {
        [DataMember(Order = 1)]
        public List<AnchorCompanyConfigsDc> AnchorCompanyConfigs { get; set; }
        [DataMember(Order = 2)]
        public List<NBFCCompanyConfigsDc> NBFCCompanyConfigs { get; set; }
        [DataMember(Order = 3)]
        public long FintechCompanyId { get; set; }
        [DataMember(Order = 4)]
        public double GSTRate { get; set; }
        [DataMember(Order = 5)]
        public long? loanAccountRepaymentsId { get; set; }
        public bool IsRunningManually { get; set; }
    }

    [DataContract]
    public class AnchorCompanyConfigsDc
    {
        [DataMember(Order = 1)]
        public long AnchorCompanyId { get; set; }
        [DataMember(Order = 2)]
        public long ProductId { get; set; }
        [DataMember(Order = 3)]
        public double AnnualInterestRate { get; set; }
        [DataMember(Order = 4)]
        public double BounceCharge { get; set; }
        [DataMember(Order = 5)]
        public double DelayPenaltyRate { get; set; }
    }

    [DataContract]
    public class NBFCCompanyConfigsDc
    {
        [DataMember(Order = 1)]
        public long NBFCCompanyId { get; set; }
        [DataMember(Order = 2)]
        public long ProductId { get; set; }
        [DataMember(Order = 3)]
        public double AnnualInterestRate { get; set; }
        [DataMember(Order = 4)]
        public double BounceCharge { get; set; }
        [DataMember(Order = 5)]
        public double PenaltyCharges { get; set; }
        [DataMember(Order = 6)]
        public bool IsInterestRateCoSharing { get; set; }
        [DataMember(Order = 7)]
        public bool IsBounceChargeCoSharing { get; set; }
        [DataMember(Order = 8)]
        public bool IsPenaltyChargeCoSharing { get; set; }
    }
}
