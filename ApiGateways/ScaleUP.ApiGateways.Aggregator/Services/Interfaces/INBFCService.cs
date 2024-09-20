using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.NBFC;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts;

namespace ScaleUP.ApiGateways.Aggregator.Services.Interfaces
{
    public interface INBFCService
    {
        Task<GRPCReply<List<NBFCSelfOfferReply>>> GetCompanyselfOfferList(List<GenerateOfferRequest> generateOfferrequest);
        Task<GRPCReply<List<DefaultOfferSelfConfigurationDc>>> AddUpdateSelfConfiguration(GRPCRequest<List<DefaultOfferSelfConfigurationDc>> selfConfigList);
    }
}
