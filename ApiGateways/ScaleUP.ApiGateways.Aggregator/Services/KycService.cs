using Grpc.Net.Client;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using ProtoBuf.Grpc.Client;
using ScaleUP.ApiGateways.Aggregator.Constants;
using ScaleUP.ApiGateways.Aggregator.Services.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Communication.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Identity.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Location.DataContracts;

namespace ScaleUP.ApiGateways.Aggregator.Services
{
    public class KycService : IKycService
    {
        private IConfiguration Configuration;
        private readonly IKycGrpcService _client;

        public KycService(IConfiguration _configuration, IKycGrpcService client)
        {
            Configuration = _configuration;
            _client = client;
        }

        public async Task<KYCPANReply> GetKYCPAN(KYCPanRequest request)
        {
            var reply = await _client.GetKYCPAN(request);
            return reply;
        }
        public async Task<KYCAadharReply> GetKYCAadhar(KYCAadharRequest request)
        {
            var reply = await _client.GetKYCAadhar(request);
            return reply;
        }
        public async Task<KYCBankStatementReply> GetKYCBankStatement(KYCBankStatementRequest request)
        {
            var reply = await _client.GetKYCBankStatement(request);
            return reply;
        }
        public async Task<KYCMSMEReply> GetKYCMSME(KYCMSMERequest request)
        {
            var reply = await _client.GetKYCMSME(request);
            return reply;
        }

        public async Task<KYCPersonalDetailReply> GetKYCPersonalDetail(KYCPersonalDetailRequest request)
        {
            var reply = await _client.GetKYCPersonalDetail(request);
            return reply;
        }
        public async Task<KYCSelfieReply> GetKYCSelfie(KYCSelfieRequest request)
        {
            var reply = await _client.GetKYCSelfie(request);
            return reply;
        }

        public async Task<UserDetailReply> GetUserDetail(string UserId)
        {
            var reply = await _client.GetUserDetail(UserId);
            return reply;
        }
        public async Task<List<UserListPageInfoReply>> GetUsersListInfo(UserDetailRequest userDetailRequest)
        {
            var reply = await _client.GetUsersListInfo(userDetailRequest);
            return reply;
        }
        public async Task<List<KYCAllInfoResponse>> GetUserLeadInfo(KYCAllInfoRequest kYCAllInfoRequest)
        {
            var reply = await _client.GetUserLeadInfo(kYCAllInfoRequest);
            return reply;
        }

        public async Task<GRPCReply<Dictionary<string, List<KYCSpecificDetailResponse>>>> GetKYCSpecificDetail(GRPCRequest<KYCSpecificDetailRequest> request)
        {
            var reply = await _client.GetKYCSpecificDetail(request);
            return reply;
        }

        public async Task<GRPCReply<bool>> RemoveSecretInfo(GRPCRequest<string> request)
        {
            var reply = await _client.RemoveSecretInfo(request);
            return reply;
        }
        public async Task<GRPCReply<bool>> RemoveKYCPersonalInfo(GRPCRequest<string> userid)
        {
            var reply = await _client.RemoveKYCPersonalInfo(userid);
            return reply;
        }
        public async Task<GRPCReply<bool>> RemoveKYCInfoOnReset(GRPCRequest<ResetLeadRequestDc> request)
        {
            var reply = await _client.RemoveKYCInfoOnReset(request);
            return reply;
        }
        [AllowAnonymous]
        public async Task<GRPCReply<bool>> UpdateAddress(GRPCRequest<UpdateAddressRequest> updateAddressRequest)
        {
               var reply = await _client.UpdateAddress(updateAddressRequest);
                
            return reply;

        }
        [AllowAnonymous]
        public async Task<GRPCReply<bool>> UpdateBuisnessDetail(GRPCRequest<UpdateBuisnessDetailRequest> updateBuisnessDetailRequest)
        {
            var reply = await _client.UpdateBuisnessDetail(updateBuisnessDetailRequest);

            return reply;
        }
        [AllowAnonymous]
        public async Task<GRPCReply<bool>> UploadLeadDocuments(GRPCRequest<UpdateLeadDocumentDetailRequest> updateLeadDocumentDetailRequest)
        {
            var reply = await _client.UploadLeadDocuments(updateLeadDocumentDetailRequest);

            return reply;

        }
        public async Task<GRPCReply<bool>> UploadMultiLeadDocuments(GRPCRequest<UpdateLeadDocumentDetailListRequest> updateLeadDocumentDetailRequest)
        {
            var reply = await _client.UploadMultiLeadDocuments(updateLeadDocumentDetailRequest);

            return reply;

        }

        public async Task<GRPCReply<bool>> RemoveDSAPersonalDetails(GRPCRequest<string> req)
        {
            var reply = await _client.RemoveDSAPersonalDetails(req);

            return reply;

        }

        public async Task<List<KYCAllInfoResponse>> GetAllKycInfoByUserIdsList(KYCAllInfoByUserIdsRequest kYCAllInfoRequest)
        {
            var reply = await _client.GetAllKycInfoByUserIdsList(kYCAllInfoRequest);
            return reply;
        }

        public async Task<GRPCReply<bool>> GetDSAGSTExist(GRPCRequest<DSAGSTExistRequest> req)
        {
            var reply = await _client.GetDSAGSTExist(req);
            return reply;
        }

        public async Task<GRPCReply<bool>> RemoveKYCMaterInfos(GRPCRequest<string> req)
        {
            var reply = await _client.RemoveKYCMaterInfos(req);

            return reply;

        }

        public async Task<GRPCReply<List<DSACityListDc>>> GetDSACityList(GRPCRequest<string> req)
        {
            var reply = await _client.GetDSACityList(req);
            return reply;
        }
        public async Task<GRPCReply<bool>> UpdateAadharStatus(KYCAllInfoByUserIdsRequest req)
        {
            var reply = await _client.UpdateAadharStatus(req);
            return reply;
        }
    }
}
