using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Media.DataContracts;

namespace ScaleUP.ApiGateways.Aggregator.Services.Interfaces
{
    public interface IMediaService
    {
        Task<DocReply> GetMediaDetail(DocRequest request);

        Task<GRPCReply<DocReply>> SaveFile(GRPCRequest<FileUploadRequest> request);

        Task<List<DocReply>> GetMediaDetails(MultiDocRequest request);

        Task<GRPCReply<GRPCPdfResponse>> HtmlToPdf(GRPCRequest<GRPCHtmlConvertRequest> convertRequest);
    }
}
