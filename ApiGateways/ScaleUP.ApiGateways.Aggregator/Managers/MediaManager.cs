using Nito.AsyncEx;
using ScaleUP.ApiGateways.Aggregator.DTOs;
using ScaleUP.ApiGateways.Aggregator.Services;
using ScaleUP.ApiGateways.Aggregator.Services.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Media.DataContracts;

namespace ScaleUP.ApiGateways.Aggregator.Managers
{
    public class MediaManager
    {
        private IMediaService _iMediaService;
        public MediaManager(IMediaService mediaService)
        {
            _iMediaService = mediaService;
        }

        public async Task<GRPCReply<GRPCPdfResponse>> HtmlToPdf(GRPCRequest<GRPCHtmlConvertRequest> convertRequest)
        {
            var data = await _iMediaService.HtmlToPdf(convertRequest);
            return data;
        }
    }
}
