using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.AyeFinSCF
{
    [DataContract]

    public class GenerateTokenResponseDC
    {
        [DataMember(Order = 1)]

        public string refId { get; set; }
        [DataMember(Order = 2)] 
        public string switchpeReferenceId { get; set; }
        [DataMember(Order = 3)]
        public string status { get; set; }
        [DataMember(Order = 4)]
        public string? token { get; set; }
        [DataMember(Order = 5)]
        public string? message { get; set; }

    }

    
    public class AddLeadResponseDC
    {
        [DataMember(Order = 1)]

        public string refId { get; set; }
        [DataMember(Order = 2)]
        public string switchpeReferenceId { get; set; }
        [DataMember(Order = 3)]
        public string status { get; set; }
        [DataMember(Order = 4)]
        public string message { get; set; }
    }
    public class GetWebUrlResponseDC
    {
        [DataMember(Order = 1)]

        public string refId { get; set; }
        [DataMember(Order = 2)]
        public string status { get; set; }
        [DataMember(Order = 3)]
        public string switchpeReferenceId { get; set; }
        [DataMember(Order = 4)]
        public string url { get; set; }
        [DataMember(Order = 5)]
        public string message { get; set; }


    }
    public class CheckCreditLineResponseDC
    {
        [DataMember(Order = 1)]

        public string refId { get; set; }
        [DataMember(Order = 2)]
        public string switchpeReferenceId { get; set; }
        [DataMember(Order = 3)]
        public string status { get; set; }
        [DataMember(Order = 4)]
        public string message { get; set; }
        [DataMember(Order = 5)]

        public Data data { get; set; }

    }
    public class Data
    {
        [DataMember(Order = 1)]

        public string status { get; set; }
        [DataMember(Order = 2)]
        public string statusMessage { get; set; }
        [DataMember(Order = 3)]
        public bool creditLineExist { get; set; }
        [DataMember(Order = 4)]
        public string loanId { get; set; }
        [DataMember(Order = 5)]
        public double totalLimit { get; set; }
        [DataMember(Order = 6)]
        public double availableLimit { get; set; }

    }
    public class TransactionVerifyOtpResponseDC
    {
        [DataMember(Order = 1)]
        public string refId { get; set; }
        [DataMember(Order = 2)]
        public string switchpeReferenceId { get; set; }
        [DataMember(Order = 3)]
        public string status { get; set; }
        [DataMember(Order = 4)]
        public string message { get; set; }
        [DataMember(Order = 5)]
        public TransactionVerifyOtpData data { get; set; }

    }
    public class TransactionVerifyOtpData
    {

        [DataMember(Order = 1)]
        public double totalLimit { get; set; }
        [DataMember(Order = 2)] 
        public double availableLimit { get; set; }

    }
    [DataContract]
    public class CheckCreditLineData
    {
        [DataMember(Order = 1)]

        public string status { get; set; }
        [DataMember(Order = 2)]
        public string statusMessage { get; set; }
        [DataMember(Order = 3)]
        public bool creditLineExist { get; set; }
        [DataMember(Order = 4)]
        public string loanId { get; set; }
        [DataMember(Order = 5)]
        public double totalLimit { get; set; }
        [DataMember(Order = 6)]
        public double availableLimit { get; set; }
        [DataMember(Order = 7)]
        public bool isweburlapproved { get; set; }
        [DataMember(Order = 8)]
        public string? webUrl { get; set; }

    }

    [DataContract]
    public class ApplyLoanResponseDC
    {
        [DataMember(Order = 1)]

        public string refId { get; set; }
        [DataMember(Order = 2)]
        public string switchpeReferenceId { get; set; }
        [DataMember(Order = 3)]
        public string status { get; set; }
        [DataMember(Order = 4)]
        public string message { get; set; }
        [DataMember(Order = 5)]
        public TxnData data { get; set; }

    }
    [DataContract]
    public class TxnData
    {
        [DataMember(Order = 1)]

        public string transactionAmount { get; set; }
        [DataMember(Order = 2)]
        public string transactionId { get; set; }
      

    }
    [DataContract]
    public class DeliveryConfirmationResponseDC
    {
        [DataMember(Order = 1)]

        public string refId { get; set; }
        [DataMember(Order = 2)]
        public string switchpeReferenceId { get; set; }
        [DataMember(Order = 3)]
        public string status { get; set; }
        [DataMember(Order = 4)]
        public string message { get; set; }
        [DataMember(Order = 5)]
        public DeliveryData data { get; set; }

    }
    [DataContract]
    public class DeliveryData
    {
        [DataMember(Order = 1)]

        public double loanAmount { get; set; }
        [DataMember(Order = 2)]
        public string transactionId { get; set; }


    }

    [DataContract]
    public class CancellationResponseDC
    {
        [DataMember(Order = 1)]

        public string refId { get; set; }
        [DataMember(Order = 2)]
        public string switchpeReferenceId { get; set; }
        [DataMember(Order = 3)]
        public string status { get; set; }
        [DataMember(Order = 4)]
        public string message { get; set; }
        [DataMember(Order = 5)]
        public CancellationData data { get; set; }

    }
    [DataContract]
    public class CancellationData
    {
        [DataMember(Order = 1)]
        public double loanAmount { get; set; }

    }

    [DataContract]
    public class ReturnRequestResponseDC
    {
        [DataMember(Order = 1)]

        public string refId { get; set; }
        [DataMember(Order = 2)]
        public string switchpeReferenceId { get; set; }
        [DataMember(Order = 3)]
        public string status { get; set; }
        [DataMember(Order = 4)]
        public string message { get; set; }
        [DataMember(Order = 5)]
        public ReturnRequestData data { get; set; }

    }
    [DataContract]
    public class ReturnRequestData
    {
        [DataMember(Order = 1)]
        public double unloanAmount { get; set; }

    }
    [DataContract]
    public class CheckTotalAndAvailableLimitResponseDc
    {
        [DataMember(Order = 1)]
        public string refId { get; set; }
        [DataMember(Order = 2)]
        public string switchpeReferenceId { get; set; }
        [DataMember(Order = 3)]
        public string status { get; set; }
        [DataMember(Order = 4)]
        public string message { get; set; }
        [DataMember(Order = 5)]
        public CheckTotalAndAvailabeData data { get; set; }

    }


    [DataContract]
    public class CheckTotalAndAvailabeData
    {
        [DataMember(Order = 1)]
        public double totalLimit { get; set; }
        [DataMember(Order = 2)]
        public double availableLimit { get; set; }
    }
}
