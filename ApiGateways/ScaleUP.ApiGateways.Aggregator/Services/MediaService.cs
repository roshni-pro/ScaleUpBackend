using Grpc.Net.Client;
using ProtoBuf.Grpc.Client;
using ScaleUP.ApiGateways.Aggregator.Constants;
using ScaleUP.ApiGateways.Aggregator.Services.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Media.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Media.Interfaces;

namespace ScaleUP.ApiGateways.Aggregator.Services
{
    public class MediaService : IMediaService
    {
        private IConfiguration Configuration;
        private readonly IMediaGrpcService _client;

        public MediaService(IConfiguration _configuration
                            , IMediaGrpcService client)
        {
            Configuration = _configuration;
            _client= client;
        }
        public async Task<DocReply> GetMediaDetail(DocRequest request)
        {
            var reply = await _client.GetMediaDetail(request);
            return reply;
        }

        public async Task<GRPCReply<DocReply>> SaveFile(GRPCRequest<FileUploadRequest> request)
        {
            var reply = await _client.SaveFile(request);
            return reply;
        }


        public async Task<List<DocReply>> GetMediaDetails(MultiDocRequest request)
        {
            var reply = await _client.GetMediaDetails(request);
            return reply;
        }
        public async Task<GRPCReply<GRPCPdfResponse>> HtmlToPdf(GRPCRequest<GRPCHtmlConvertRequest> convertRequest)
        {
            var reply = await _client.HtmlToPdf(convertRequest);
            return reply;
        }
    }
}
