using ProtoBuf.Grpc;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Location.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.Interfaces
{
    [ServiceContract]
    public interface IKycGrpcService
    {
        [OperationContract]
        Task<KYCPANReply> GetKYCPAN(KYCPanRequest request,
            CallContext context = default);

        [OperationContract]
        Task<KYCAadharReply> GetKYCAadhar(KYCAadharRequest request,
           CallContext context = default);

        [OperationContract]
        Task<KYCBankStatementReply> GetKYCBankStatement(KYCBankStatementRequest request,
          CallContext context = default);

        [OperationContract]
        Task<KYCMSMEReply> GetKYCMSME(KYCMSMERequest request,
         CallContext context = default);
        [OperationContract]
        Task<KYCPersonalDetailReply> GetKYCPersonalDetail(KYCPersonalDetailRequest request,
          CallContext context = default);

        [OperationContract]
        Task<KYCSelfieReply> GetKYCSelfie(KYCSelfieRequest request,
         CallContext context = default);

        [OperationContract]
        Task<UserDetailReply> GetUserDetail(string UserId,
                CallContext context = default);

        [OperationContract]
        Task<List<UserListPageInfoReply>> GetUsersListInfo(UserDetailRequest UserId, CallContext context = default);

        [OperationContract]
        Task<List<KYCAllInfoResponse>> GetUserLeadInfo(KYCAllInfoRequest kYCAllInfoRequest, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<Dictionary<string, List<KYCSpecificDetailResponse>>>> GetKYCSpecificDetail(GRPCRequest<KYCSpecificDetailRequest> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<bool>> RemoveSecretInfo(GRPCRequest<string> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> RemoveKYCPersonalInfo(GRPCRequest<string> userid, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> RemoveKYCInfoOnReset(GRPCRequest<ResetLeadRequestDc> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> UpdateAddress(GRPCRequest<UpdateAddressRequest> updateAddressRequest, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> UpdateBuisnessDetail(GRPCRequest<UpdateBuisnessDetailRequest> updateBuisnessDetailRequest, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> UploadLeadDocuments(GRPCRequest<UpdateLeadDocumentDetailRequest> updateLeadDocumentDetailRequest, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> UploadMultiLeadDocuments(GRPCRequest<UpdateLeadDocumentDetailListRequest> updateLeadDocumentDetailRequest, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> RemoveDSAPersonalDetails(GRPCRequest<string> req, CallContext context = default);
        [OperationContract]
        Task<List<KYCAllInfoResponse>> GetAllKycInfoByUserIdsList(KYCAllInfoByUserIdsRequest kYCAllInfoRequest, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<bool>> GetDSAGSTExist(GRPCRequest<DSAGSTExistRequest> req,
          CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> RemoveKYCMaterInfos(GRPCRequest<string> req, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<DSACityListDc>>> GetDSACityList(GRPCRequest<string> req, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> UpdateAadharStatus(KYCAllInfoByUserIdsRequest req,
          CallContext context = default);
    }
}
