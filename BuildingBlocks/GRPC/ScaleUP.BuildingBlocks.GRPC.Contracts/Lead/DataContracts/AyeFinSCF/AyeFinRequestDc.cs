using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.AyeFinSCF
{
    [DataContract]
    public class AyeFinRequestDc
    {
        [DataMember(Order = 1)]
        public string refId { get; set; }
        [DataMember(Order = 2)]
        public string partnerId { get; set; }
        [DataMember(Order = 3)]
        public string apiKey { get; set; }

    }



    [DataContract]
    public class AyeleadReq
    {
        [DataMember(Order = 1)]
        public long LeadId { get; set; }
        [DataMember(Order = 2)]
        public string? token { get; set; }

    }
    [DataContract]
    public class AddLeadRequestDc
    {
        [DataMember(Order = 1)]
        public string refId { get; set; }
        [DataMember(Order = 2)]
        public string leadId { get; set; }
        [DataMember(Order = 3)]
        public string salutation { get; set; }
        [DataMember(Order = 4)]
        public string firstName { get; set; }
        [DataMember(Order = 5)]
        public string lastName { get; set; }
        [DataMember(Order = 6)]
        public string mobileNumber { get; set; }
        [DataMember(Order = 7)]
        public string email { get; set; }
        [DataMember(Order = 8)]
        public string gender { get; set; }
        [DataMember(Order = 9)]
        public string fatherName { get; set; }
        [DataMember(Order = 10)]
        public string address { get; set; }
        [DataMember(Order = 11)]
        public string addressLine1 { get; set; }
        [DataMember(Order = 12)]
        public string landmark { get; set; }
        [DataMember(Order = 13)]
        public string pincode { get; set; }
        [DataMember(Order = 14)]
        public string state { get; set; }
        [DataMember(Order = 15)]
        public string city { get; set; }
        [DataMember(Order = 16)]
        public string storeSize { get; set; }
        [DataMember(Order = 17)]
        public string gstNo { get; set; }
        [DataMember(Order = 18)]
        public string shopName { get; set; }
        [DataMember(Order = 19)]
        public string udyogAadhaar { get; set; }
        [DataMember(Order = 20)]
        public string businessType { get; set; }
        [DataMember(Order = 21)]
        public string storeLength { get; set; }
        [DataMember(Order = 22)]
        public string storeWidth { get; set; }
        [DataMember(Order = 23)]
        public string signBoardLength { get; set; }
        [DataMember(Order = 24)]
        public string signBoardWidth { get; set; }
        [DataMember(Order = 25)]
        public string signBoardType { get; set; }
        [DataMember(Order = 26)]
        public string customerAttendingCapacity { get; set; }
        [DataMember(Order = 27)]
        public string storeArea { get; set; }
        [DataMember(Order = 28)]
        public string userName { get; set; }


    }
    [DataContract]

    public class CheckCreditLineReqDc
    {
        [DataMember(Order = 1)]
        public string refId { get; set; }
        [DataMember(Order = 2)]
        public string leadId { get; set; }

    }
    [DataContract]

    public class GetWebUrlReqDc
    {
        [DataMember(Order = 1)]
        public string refId { get; set; }
        [DataMember(Order = 2)]
        public string leadId { get; set; }
        [DataMember(Order = 3)]
        public string callbackUrl { get; set; }

    }
    [DataContract]

    public class TransactionSendOtpReqDc
    {
        [DataMember(Order = 1)]
        public string refId { get; set; }
        [DataMember(Order = 2)]
        public string leadId { get; set; }


    }
    //public class TransactionVerifyOtpReqDc
    //{
    //    [DataMember(Order = 1)]
    //    public string refId { get; set; }
    //    [DataMember(Order = 2)]
    //    public string leadId { get; set; }
    //    [DataMember(Order = 3)]
    //    public string otp { get; set; }

    //} 
    [DataContract]

    public class TransactionVerifyOtpReqDc
    {
        [DataMember(Order = 1)]
        public long leadId { get; set; }
        [DataMember(Order = 2)]
        public string otp { get; set; }
        [DataMember(Order = 3)]
        public string token { get; set; }
    }

    [DataContract]

    public class TransactionVerifythirdpartyreq
    {
        [DataMember(Order = 1)]
        public string refId { get; set; }
        [DataMember(Order = 2)]
        public string leadId { get; set; }
        [DataMember(Order = 3)]
        public string otp { get; set; }

    }
    [DataContract]

    public class ApplyLoanthirdpartyreq
    {
        [DataMember(Order = 1)]
        public string refId { get; set; }
        [DataMember(Order = 2)]
        public string leadId { get; set; }
        [DataMember(Order = 3)]
        public double amount { get; set; }
        [DataMember(Order = 4)]
        public string orderId { get; set; }

    }
    [DataContract]

    public class ApplyLoanreq
    {
        [DataMember(Order = 1)]
        public long loanId { get; set; }
        [DataMember(Order = 2)]
        public double amount { get; set; }
        [DataMember(Order = 3)]
        public string orderId { get; set; }
        [DataMember(Order = 4)]
        public string? token { get; set; }
        [DataMember(Order = 5)]
        public long NBFCCompanyAPIDetailId { get; set; }
        [DataMember(Order = 6)]
        public string APIUrl {  get; set; }
        [DataMember(Order = 7)]
        public long invoiceId { get; set; }
        [DataMember(Order = 8)]
        public double? TotalLimit { get; set; }
        [DataMember(Order = 9)]
        public double AvailableLimit { get; set; }

    }

    [DataContract]

    public class DeliveryConfirmationreq
    {
        [DataMember(Order = 1)]
        public long loanId { get; set; }
        [DataMember(Order = 2)]
        public string? invoiceNo { get; set; }
        [DataMember(Order = 3)]
        public double amount { get; set; }
        [DataMember(Order = 4)]
        public string orderId { get; set; }
        [DataMember(Order = 5)]
        public string? token { get; set; }

    }
    [DataContract]
    public class DeliveryConfirmationthirdpartyreq
    {
        [DataMember(Order = 1)]
        public string refId { get; set; }
        [DataMember(Order = 2)]
        public string leadId { get; set; }
        [DataMember(Order = 3)]
        public string switchpeReferenceId { get; set; }
        [DataMember(Order = 4)]
        public string invoiceNo { get; set; }
        [DataMember(Order = 5)]
        public double amount { get; set; }
        [DataMember(Order = 6)]
        public string orderId { get; set; }
        [DataMember(Order = 7)]
        public string trackingId { get; set; }

    }

    [DataContract]
    public class AyeloanReq
    {
        [DataMember(Order = 1)]
        public long loanId { get; set; }
        [DataMember(Order = 2)]
        public string? token { get; set; }

    }

    [DataContract]

    public class CancelTxnThirdPartyRed
    {
        [DataMember(Order = 1)]
        public string refId { get; set; }
        [DataMember(Order = 2)]
        public string leadId { get; set; }
        [DataMember(Order = 3)]
        public string switchpeReferenceId { get; set; }
        [DataMember(Order = 4)]
        public double amount { get; set; }
        [DataMember(Order = 5)]
        public string orderId { get; set; }
        [DataMember(Order = 6)]
        public string cancellationCode { get; set; }
        [DataMember(Order = 7)]
        public string remarks { get; set; }
    }
    [DataContract]

    public class CancelTxnReq
    {
        [DataMember(Order = 1)]
        public long loanId { get; set; }
        [DataMember(Order = 2)]
        public double amount { get; set; }
        [DataMember(Order = 3)]
        public string orderId { get; set; }
        [DataMember(Order = 4)]
        public string cancellationCode { get; set; }
        [DataMember(Order = 5)]
        public string remarks { get; set; }
        [DataMember(Order = 6)]
        public string? token { get; set; }
    }
    [DataContract]

    public class RepaymentThirdpartyreq
    {
        [DataMember(Order = 1)]
        public string refId { get; set; }
        [DataMember(Order = 2)]
        public string leadId { get; set; }
        [DataMember(Order = 3)]
        public string orderId { get; set; }
        [DataMember(Order = 4)]
        public double amount { get; set; }
        [DataMember(Order = 5)]
        public string modeOfPayment { get; set; }
        [DataMember(Order = 6)]
        public string adjustment { get; set; }
        [DataMember(Order = 7)]
        public string receiptId { get; set; }

    }

    [DataContract]
    public class Repaymentreq
    {
        [DataMember(Order = 1)]
        public string? refId { get; set; }
        [DataMember(Order = 2)]
        public long loanId { get; set; }
        [DataMember(Order = 3)]
        public string? orderId { get; set; }
        [DataMember(Order = 4)]
        public double amount { get; set; }
        [DataMember(Order = 5)]
        public string modeOfPayment { get; set; }
        [DataMember(Order = 6)]
        public string adjustment { get; set; }
        [DataMember(Order = 7)]
        public string receiptId { get; set; }
        [DataMember(Order = 8)]
        public string? token { get; set; }
        [DataMember(Order = 9)]
        public string TransactionReqNo { get; set; }

    }
    [DataContract]
    public class GetNBFCLoanAccountDetailDTO
    {
        [DataMember(Order = 1)]
        public long loanAccountId { get; set; }
        [DataMember(Order = 2)]
        public string? NBFCToken { get; set; }
        
    }
}
