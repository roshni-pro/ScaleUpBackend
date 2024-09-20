using ProtoBuf.Grpc;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Location.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Media.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Media.Interfaces
{
    [ServiceContract]
    public interface IMediaGrpcService
    {
        [OperationContract]
        Task<DocReply> GetMediaDetail(DocRequest request,
            CallContext context = default);

        [OperationContract]
        Task<GRPCReply<DocReply>> SaveFile(GRPCRequest<FileUploadRequest> request, CallContext context = default);


        [OperationContract]
        Task<List<DocReply>> GetMediaDetails(MultiDocRequest request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<GRPCPdfResponse>> HtmlToPdf(GRPCRequest<GRPCHtmlConvertRequest> convertRequest, CallContext context = default);
    }
}
