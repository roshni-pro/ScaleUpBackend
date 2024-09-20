using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class eNachBankDetailDc
    {
        [DataMember(Order = 1)]
        public long LeadMasterId { set; get; }
        [DataMember(Order = 2)]
        public string Name { set; get; }
        [DataMember(Order = 3)]
        public string BankName { set; get; }
        [DataMember(Order = 4)]
        public string AccountNo { set; get; }
        [DataMember(Order = 5)]
        public string IfscCode { set; get; }
        [DataMember(Order = 6)]
        public string AccountType { set; get; } // “S” for Savings , “C” for Current or “O” “Other”.
        [DataMember(Order = 7)]
        public string RelationshipTypes { set; get; } //SOW,JOF,NRE,JOO,NRO ()
        [DataMember(Order = 8)]
        public string Channel { set; get; } //"Debit" for Debit Card, "Net" for Net-banking.
        [DataMember(Order = 9)]
        public string MsgId { set; get; }
        [DataMember(Order = 10)]
        public string CreatedBy { set; get; }

    }


    [DataContract]
    public class SendeMandateRequestDc
    {
        [DataMember(Order = 1)]
        public string CheckSum { get; set; }

        [DataMember(Order = 2)]
        public string MsgId { get; set; }
        [DataMember(Order = 3)]
        public string Customer_Name { get; set; }

        [DataMember(Order = 4)]
        public string Customer_Mobile { get; set; }
        [DataMember(Order = 5)]
        public string Customer_EmailId { get; set; }
        [DataMember(Order = 6)]
        public string Customer_AccountNo { get; set; }
        [DataMember(Order = 7)]
        public string Short_Code { get; set; }
        [DataMember(Order = 8)]
        public string UtilCode { get; set; }
        [DataMember(Order = 9)]
        public string LeadNo { get; set; }
        [DataMember(Order = 10)]
        public long LeadId { get; set; }
        [DataMember(Order = 11)]
        public string CreatedBy { get; set; }
        [DataMember(Order = 12)]
        public long CreditLimit { get; set; }
        [DataMember(Order = 13)]
        public long? ActivityId { get; set; }
        [DataMember(Order = 14)]
        public long? SubActivityId { get; set; }

        [DataMember(Order = 15)]
        public long? CompanyId { get; set; }



        [DataMember(Order = 16)]
        public string Customer_InstructedMemberId { get; set; }

        [DataMember(Order = 17)]
        public string Channel { get; set; }

        [DataMember(Order = 18)]
        public string Filler5 { get; set; }


    }
}
