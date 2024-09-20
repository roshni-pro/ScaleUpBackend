using MassTransit;
using ScaleUP.ApiGateways.Aggregator.Services.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.NBFC;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Media.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;

namespace ScaleUP.ApiGateways.Aggregator.Services
{
    public class NBFCService : INBFCService
    {
        private IConfiguration Configuration;
        private readonly INBFCGrpcService _client;

        public NBFCService(IConfiguration _configuration
            , INBFCGrpcService client
            )
        {
            Configuration = _configuration;
            _client = client;
        }



        public async Task<GRPCReply<List<NBFCSelfOfferReply>>> GetCompanyselfOfferList(List<GenerateOfferRequest> generateOfferrequest)
        {
            GRPCReply<List<NBFCSelfOfferReply>> reply = new GRPCReply<List<NBFCSelfOfferReply>>();

            reply = await _client.GetCompanyselfOfferList(generateOfferrequest);
            return reply;
        }

        public async Task<GRPCReply<List<DefaultOfferSelfConfigurationDc>>> AddUpdateSelfConfiguration(GRPCRequest<List<DefaultOfferSelfConfigurationDc>> gRPCRequest)
        {
            var reply = await _client.AddUpdateSelfConfiguration(gRPCRequest);
            return reply;
        }
    }
}
