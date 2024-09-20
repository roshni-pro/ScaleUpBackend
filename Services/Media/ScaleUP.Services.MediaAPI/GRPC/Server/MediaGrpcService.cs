using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nito.AsyncEx;
using ProtoBuf.Grpc;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Media.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Media.Interfaces;
using ScaleUP.Services.MediaAPI.Constants;
using ScaleUP.Services.MediaAPI.Manager;
using ScaleUP.Services.MediaAPI.Persistence;
using ScaleUP.Services.MediaDTO.Transaction.DocumentConverter;

namespace ScaleUP.Services.MediaAPI.GRPC.Server
{
    //[ApiExplorerSettings(GroupName = "Media Grpc")]
    public class MediaGrpcService : IMediaGrpcService
    {
       
        private readonly MediaGrpcManager _mediaGrpcManager;
        public MediaGrpcService( MediaGrpcManager mediaGrpcManager)
        {
            _mediaGrpcManager = mediaGrpcManager;
        }

        public Task<DocReply> GetMediaDetail(DocRequest request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _mediaGrpcManager.GetMediaDetail(request));
            return Task.FromResult(response);
        }

        public Task<GRPCReply<DocReply>> SaveFile(GRPCRequest<FileUploadRequest> request, CallContext context = default)
        {
            GRPCReply<DocReply> gRPCReply;

            var response = AsyncContext.Run(() => _mediaGrpcManager.SaveFile(request.Request));
            if (response != null && response.Status)
            {
                gRPCReply = new GRPCReply<DocReply>
                {
                    Response = response,
                    Status = response.Status,
                    Message = "Uploaded Successfully"
                };
            }
            else
            {
                gRPCReply = new GRPCReply<DocReply>
                {
                    Response = null,
                    Status = response.Status,
                    Message = "Failed to upload Successfully"
                };
            }
            return Task.FromResult(gRPCReply);
        }

        public Task<List<DocReply>> GetMediaDetails(MultiDocRequest request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _mediaGrpcManager.GetMediaDetails(request));
            return Task.FromResult(response);
        }       

        public Task<GRPCReply<GRPCPdfResponse>> HtmlToPdf(GRPCRequest<GRPCHtmlConvertRequest> convertRequest, CallContext context)
        {
            var response = AsyncContext.Run(() => _mediaGrpcManager.HtmlToPdf(convertRequest));
            return Task.FromResult(response);
        }
    }
}
