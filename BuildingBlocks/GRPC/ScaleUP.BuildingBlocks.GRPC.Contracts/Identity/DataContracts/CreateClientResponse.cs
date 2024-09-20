using System.Runtime.Serialization;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Identity.DataContracts
{
    [DataContract]
    public class CreateClientResponse
    {
        [DataMember(Order = 1)]
        public string ApiKey { get; set; }

        [DataMember(Order = 2)]
        public string ApiSecret { get; set; }
    }
}
