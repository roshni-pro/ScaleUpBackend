namespace ScaleUP.ApiGateways.Aggregator.DTOs
{
    public class eNachSendReqAggDTO
    {
        public string url { get; set; }
        public ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.request request { get; set; }
        public bool status { get; set; }
        public string error { get; set; }
    }

    public class request11
    {
        public string CheckSum { get; set; }
        //Customer Account Number|
        //Customer_StartDate|
        //Customer_ExpiryDate|
        //Customer_DebitAmount|
        //Customer_MaxAmount
        public string MsgId { get; set; }
        public string Customer_Name { get; set; }
        public string Customer_Mobile { get; set; }
        public string Customer_EmailId { get; set; }
        public string Customer_AccountNo { get; set; }
        public string Customer_StartDate { get; set; }
        public string Customer_ExpiryDate { get; set; }
        public string Customer_DebitAmount { get; set; }
        public string Customer_MaxAmount { get; set; }
        public string Customer_DebitFrequency { get; set; }
        public string Customer_InstructedMemberId { get; set; }
        public string Short_Code { get; set; }
        public string UtilCode { get; set; }

        public string Customer_SequenceType { get; set; }
        public string Merchant_Category_Code { get; set; }
        public string Customer_Reference1 { get; set; }
        public string Customer_Reference2 { get; set; }
        public string Channel { get; set; }

        public string Filler1 { get; set; }
        public string Filler2 { get; set; }
        public string Filler3 { get; set; }
        public string Filler4 { get; set; }
        public string Filler5 { get; set; }
        public string Filler6 { get; set; }
        public string Filler7 { get; set; }
        public string Filler8 { get; set; }
        public string Filler9 { get; set; }
        public string Filler10 { get; set; }
    }
}
