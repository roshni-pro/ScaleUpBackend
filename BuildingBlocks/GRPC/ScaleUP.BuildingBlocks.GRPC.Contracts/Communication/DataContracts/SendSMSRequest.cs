using System.Runtime.Serialization;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Communication.DataContracts
{
    [DataContract]
    public class SendSMSRequest
    {
        [DataMember(Order = 1)]
        public string MobileNo { get; set; }
        [DataMember(Order = 2)]
        public string SMS { get; set; }
        [DataMember(Order = 3)]
        public int ExpiredInMin { get; set; }

        [DataMember(Order = 4)]
        public string routeId { get; set; }
        [DataMember(Order = 5)]
        public string DLTId { get; set; }
        [DataMember(Order = 6)]
        public string OTP { get; set; }

    }
}
