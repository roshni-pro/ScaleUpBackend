using System.Runtime.Serialization;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Communication.DataContracts
{
    [DataContract]
    public class SendSMSReply
    {
        [DataMember(Order = 1)]
        public bool Status { get; set; }
        [DataMember(Order = 2)]
        public string Message { get; set; }
    }
}
