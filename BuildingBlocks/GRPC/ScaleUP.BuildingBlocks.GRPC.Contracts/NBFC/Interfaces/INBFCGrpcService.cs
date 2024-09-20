using ProtoBuf.Grpc;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.NBFC;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.Interfaces
{
    [ServiceContract]
    public interface INBFCGrpcService
    {
        [OperationContract]
        Task<GRPCReply<List<NBFCSelfOfferReply>>> GetCompanyselfOfferList(List<GenerateOfferRequest> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<DefaultOfferSelfConfigurationDc>>> AddUpdateSelfConfiguration(GRPCRequest<List<DefaultOfferSelfConfigurationDc>> gRPCRequest, CallContext context = default);
    }
}
