using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class eNachSendReqDTO
    {
        [DataMember(Order = 1)]
        public string url { get; set; }
        [DataMember(Order = 2)]
        public request request { get; set; }
        [DataMember(Order = 3)]
        public bool status { get; set; }
        [DataMember(Order = 4)]
        public string error { get; set; }
    }

    [DataContract]
    public class request
    {
        [DataMember(Order = 1)]
        public string CheckSum { get; set; }
        //Customer Account Number|
        //Customer_StartDate|
        //Customer_ExpiryDate|
        //Customer_DebitAmount|
        //Customer_MaxAmount
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
        public string Customer_StartDate { get; set; }
        [DataMember(Order = 8)]
        public string Customer_ExpiryDate { get; set; }
        [DataMember(Order = 9)]
        public string Customer_DebitAmount { get; set; }
        [DataMember(Order = 10)]
        public string Customer_MaxAmount { get; set; }
        [DataMember(Order = 11)]
        public string Customer_DebitFrequency { get; set; }
        [DataMember(Order = 12)]
        public string Customer_InstructedMemberId { get; set; }
        [DataMember(Order = 13)]
        public string Short_Code { get; set; }
        [DataMember(Order = 14)]
        public string UtilCode { get; set; }
        [DataMember(Order = 15)]

        public string Customer_SequenceType { get; set; }
        [DataMember(Order = 16)]
        public string Merchant_Category_Code { get; set; }
        [DataMember(Order = 17)]
        public string Customer_Reference1 { get; set; }
        [DataMember(Order = 18)]
        public string Customer_Reference2 { get; set; }
        [DataMember(Order = 19)]
        public string Channel { get; set; }
        [DataMember(Order = 20)]

        public string Filler1 { get; set; }
        [DataMember(Order = 21)]
        public string Filler2 { get; set; }
        [DataMember(Order = 22)]
        public string Filler3 { get; set; }
        [DataMember(Order = 23)]
        public string Filler4 { get; set; }
        [DataMember(Order = 24)]
        public string Filler5 { get; set; }
        [DataMember(Order = 25)]
        public string Filler6 { get; set; }
        [DataMember(Order = 26)]
        public string Filler7 { get; set; }
        [DataMember(Order = 27)]
        public string Filler8 { get; set; }
        [DataMember(Order = 28)]
        public string Filler9 { get; set; }
        [DataMember(Order = 29)]
        public string Filler10 { get; set; }
    }



}
