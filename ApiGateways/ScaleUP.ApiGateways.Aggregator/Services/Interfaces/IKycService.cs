using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Location.DataContracts;

namespace ScaleUP.ApiGateways.Aggregator.Services.Interfaces
{
    public interface IKycService
    {
        Task<KYCPANReply> GetKYCPAN(KYCPanRequest request);
        Task<KYCAadharReply> GetKYCAadhar(KYCAadharRequest request);
        Task<KYCBankStatementReply> GetKYCBankStatement(KYCBankStatementRequest request);
        Task<KYCMSMEReply> GetKYCMSME(KYCMSMERequest request);
        Task<KYCPersonalDetailReply> GetKYCPersonalDetail(KYCPersonalDetailRequest request);
        Task<KYCSelfieReply> GetKYCSelfie(KYCSelfieRequest request);
        Task<UserDetailReply> GetUserDetail(string UserId);
        Task<List<UserListPageInfoReply>> GetUsersListInfo(UserDetailRequest userDetailRequest);
        Task<List<KYCAllInfoResponse>> GetUserLeadInfo(KYCAllInfoRequest kYCAllInfoRequest);
        Task<GRPCReply<Dictionary<string, List<KYCSpecificDetailResponse>>>> GetKYCSpecificDetail(GRPCRequest<KYCSpecificDetailRequest> request);
        Task<GRPCReply<bool>> RemoveSecretInfo(GRPCRequest<string> request);
        Task<GRPCReply<bool>> RemoveKYCPersonalInfo(GRPCRequest<string> userid);
        Task<GRPCReply<bool>> RemoveKYCInfoOnReset(GRPCRequest<ResetLeadRequestDc> request);
        Task<GRPCReply<bool>> UpdateAddress(GRPCRequest<UpdateAddressRequest> updateAddressRequest);
        Task<GRPCReply<bool>> UpdateBuisnessDetail(GRPCRequest<UpdateBuisnessDetailRequest> updateBuisnessDetailRequest);
        Task<GRPCReply<bool>> UploadLeadDocuments(GRPCRequest<UpdateLeadDocumentDetailRequest> updateLeadDocumentDetailRequest);
        Task<GRPCReply<bool>> UploadMultiLeadDocuments(GRPCRequest<UpdateLeadDocumentDetailListRequest> updateLeadDocumentDetailRequest);
        Task<GRPCReply<bool>> RemoveDSAPersonalDetails(GRPCRequest<string> req);
        Task<List<KYCAllInfoResponse>> GetAllKycInfoByUserIdsList(KYCAllInfoByUserIdsRequest kYCAllInfoRequest);
        Task<GRPCReply<bool>> GetDSAGSTExist(GRPCRequest<DSAGSTExistRequest> req);
       // Task<GRPCReply<bool>> DeleteDSADetails(GRPCRequest<List<KYCAllInfoResponse>> req);
        Task<GRPCReply<bool>> RemoveKYCMaterInfos(GRPCRequest<string> req);
        //Task<ResultViewModel<KarzaAadharDTO>> KarzaAadhaarOtpVerifyForNBFC(KYCActivityAadhar request);
        Task<GRPCReply<List<DSACityListDc>>> GetDSACityList(GRPCRequest<string> req);

        Task<GRPCReply<bool>> UpdateAadharStatus(KYCAllInfoByUserIdsRequest req);

    }
}
