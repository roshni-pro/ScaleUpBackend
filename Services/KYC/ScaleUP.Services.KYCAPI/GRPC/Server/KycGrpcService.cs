using Microsoft.AspNetCore.Authorization;
using Nito.AsyncEx;
using ProtoBuf.Grpc;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.KYCActivity;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Communication.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Location.DataContracts;
using ScaleUP.Services.KYCAPI.KYCFactory;
using ScaleUP.Services.KYCAPI.Managers;
using ScaleUP.Services.KYCAPI.Persistence;
using ScaleUP.Services.KYCDTO.Constant;
using ScaleUP.Services.KYCDTO.Transacion;

namespace ScaleUP.Services.KYCAPI.GRPC.Server
{
    public class KycGrpcService : IKycGrpcService
    {
        private string _basePath;
        private readonly ApplicationDbContext _context;
        private readonly KycGrpcManager _kycGrpcManager;
        private readonly KYCMasterInfoManager _kYCMasterInfoManager;

        public KycGrpcService(KycGrpcManager kycGrpcManager, KYCMasterInfoManager kYCMasterInfoManager)
        {

            _kycGrpcManager = kycGrpcManager;
            _kYCMasterInfoManager = kYCMasterInfoManager;
            // _kycGrpcManager = new KycGrpcManager(context, _basePath);
            //_basePath = basepath;

        }
        public Task<KYCPANReply> GetKYCPAN(KYCPanRequest request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _kycGrpcManager.GetKYCPAN(request));
            return Task.FromResult(response);
        }
        public Task<KYCAadharReply> GetKYCAadhar(KYCAadharRequest request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _kycGrpcManager.GetKYCAadhar(request));
            return Task.FromResult(response);
        }

        public Task<KYCBankStatementReply> GetKYCBankStatement(KYCBankStatementRequest request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _kycGrpcManager.GetKYCBankStatement(request));
            return Task.FromResult(response);
        }

        public Task<KYCMSMEReply> GetKYCMSME(KYCMSMERequest request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _kycGrpcManager.GetKYCMSME(request));
            return Task.FromResult(response);
        }
        public Task<KYCPersonalDetailReply> GetKYCPersonalDetail(KYCPersonalDetailRequest request, ProtoBuf.Grpc.CallContext context = default)
        {
            var response = AsyncContext.Run(() => _kycGrpcManager.GetKYCPersonalDetail(request));
            return Task.FromResult(response);
        }

        public Task<KYCSelfieReply> GetKYCSelfie(KYCSelfieRequest request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _kycGrpcManager.GetKYCSelfie(request));
            return Task.FromResult(response);
        }
        public Task<UserDetailReply> GetUserDetail(string UserId, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _kycGrpcManager.GetUserDetail(UserId));
            return Task.FromResult(response);
        }

        public Task<List<UserListPageInfoReply>> GetUsersListInfo(UserDetailRequest userDetailRequest, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _kycGrpcManager.GetUsersListInfo(userDetailRequest));
            return Task.FromResult(response);
        }

        public Task<List<KYCAllInfoResponse>> GetUserLeadInfo(KYCAllInfoRequest kYCAllInfoRequest, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _kYCMasterInfoManager.GetAllKycInfo(kYCAllInfoRequest));
            return Task.FromResult(response);
        }

        public Task<GRPCReply<Dictionary<string, List<KYCSpecificDetailResponse>>>> GetKYCSpecificDetail(GRPCRequest<KYCSpecificDetailRequest> request, CallContext context = default)
        {
            GRPCReply<Dictionary<string, List<KYCSpecificDetailResponse>>> result = null;

            var response = AsyncContext.Run(() => _kycGrpcManager.GetKYCSpecificDetail(request));
            if (response == null || !response.Any())
            {
                result = new GRPCReply<Dictionary<string, List<KYCSpecificDetailResponse>>>
                {
                    Message = "Data not found",
                    Status = true,
                    Response = null
                };
            }
            else
            {
                result = new GRPCReply<Dictionary<string, List<KYCSpecificDetailResponse>>>
                {
                    Message = "Data found",
                    Status = true,
                    Response = response
                };
            }
            
            return Task.FromResult(result);
        }

        public  Task<GRPCReply<bool>> RemoveSecretInfo(GRPCRequest<string> request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _kycGrpcManager.RemoveSecretInfo(request));
            return Task.FromResult(response);
        }
        public  Task<GRPCReply<bool>> RemoveKYCPersonalInfo(GRPCRequest<string> userid, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _kycGrpcManager.RemoveKYCPersonalInfo(userid));
            return Task.FromResult(response);
        }
        public Task<GRPCReply<bool>> RemoveKYCInfoOnReset(GRPCRequest<ResetLeadRequestDc> request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _kycGrpcManager.RemoveKYCInfoOnReset(request));
            return Task.FromResult(response);
        }
        [AllowAnonymous]
        public Task<GRPCReply<bool>> UpdateAddress(GRPCRequest<UpdateAddressRequest> updateAddressRequest, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _kycGrpcManager.UpdateAddress(updateAddressRequest));
            return Task.FromResult(response);
        }
        [AllowAnonymous]
        public Task<GRPCReply<bool>> UpdateBuisnessDetail(GRPCRequest<UpdateBuisnessDetailRequest> updateBuisnessDetailRequest, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _kycGrpcManager.UpdateBuisnessDetail(updateBuisnessDetailRequest));
            return Task.FromResult(response);
        }
        [AllowAnonymous]
        public Task<GRPCReply<bool>> UploadLeadDocuments(GRPCRequest<UpdateLeadDocumentDetailRequest> updateLeadDocumentDetailRequest, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _kycGrpcManager.UploadLeadDocuments(updateLeadDocumentDetailRequest));
            return Task.FromResult(response);
        }
        public Task<GRPCReply<bool>> UploadMultiLeadDocuments(GRPCRequest<UpdateLeadDocumentDetailListRequest> updateLeadDocumentDetailRequest, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _kycGrpcManager.UploadMultiLeadDocuments(updateLeadDocumentDetailRequest));
            return Task.FromResult(response);
        }

        public Task<GRPCReply<bool>> RemoveDSAPersonalDetails(GRPCRequest<string> req, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _kYCMasterInfoManager.RemoveDSAPersonalDetails(req));
            return Task.FromResult(response);
        }

        public Task<List<KYCAllInfoResponse>> GetAllKycInfoByUserIdsList(KYCAllInfoByUserIdsRequest kYCAllInfoRequest, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _kYCMasterInfoManager.GetAllKycInfoByUserIdsList(kYCAllInfoRequest));
            return Task.FromResult(response);
        }
        public Task<GRPCReply<bool>> GetDSAGSTExist(GRPCRequest<DSAGSTExistRequest> req, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _kYCMasterInfoManager.DSAGSTExist(req));
            return Task.FromResult(response);
        }
        public Task<GRPCReply<bool>> RemoveKYCMaterInfos(GRPCRequest<string> req, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _kYCMasterInfoManager.RemoveKYCMasterInfo(req));
            return Task.FromResult(response);
        }
        public Task<GRPCReply<List<DSACityListDc>>> GetDSACityList(GRPCRequest<string> req, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _kYCMasterInfoManager.GetDSACityList(req));
            return Task.FromResult(response);
        }
        public Task<GRPCReply<bool>> UpdateAadharStatus(KYCAllInfoByUserIdsRequest req, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _kYCMasterInfoManager.UpdateAadharStatus(req));
            return Task.FromResult(response);
        }
    }
}
