using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Nito.AsyncEx;
using ProtoBuf.Grpc;
using ScaleUp.Services.CommunicationAPI.Persistence;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Communication.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Communication.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using ScaleUP.Services.CommunicationAPI.Helper;
using ScaleUP.Services.CommunicationAPI.Manager;
using ScaleUP.Services.CommunicationModels;

namespace ScaleUP.Services.CommunicationAPI.GRPC.Server
{
    public class CommunicationGrpcService : ICommunicationGrpcService
    {
        private readonly CommunicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly CommunicationManager _CommunicationManager;
        public CommunicationGrpcService(CommunicationDbContext context, IConfiguration configuration, CommunicationManager CommunicationManager)
        {
            _context = context;
            _configuration = configuration;
            _CommunicationManager= CommunicationManager;
        }
        public Task<SendSMSReply> SendSMS(SendSMSRequest request, CallContext context = default)
        {
            var sendSMSReply = AsyncContext.Run(() =>  _CommunicationManager.SendSMS(request));
            return Task.FromResult(sendSMSReply);
        }

        [AllowAnonymous]
        public Task<ValidateOTPReply> ValidateOTP(ValidateOTPRequest request, CallContext context = default)
        {
            var validateOTPReply = AsyncContext.Run(() => _CommunicationManager.ValidateOTP(request));
            return Task.FromResult(validateOTPReply);
        }
        [AllowAnonymous]
        public Task<SendEmailReply> SendEmail(SendEmailRequest request, CallContext context = default)
        {
            var sendEmailReply = AsyncContext.Run(() => _CommunicationManager.SendEmail(request));
            return Task.FromResult(sendEmailReply);
        }

        public Task<EmailVerifyReply> SendOTPOnEmail(SendEmailRequest request, CallContext context = default)
        {
            var emailVerifyReply = AsyncContext.Run(() => _CommunicationManager.SendOTPOnEmail(request));
            return Task.FromResult(emailVerifyReply);
        }
        public Task<ValidateEmailReply> EmailValidateOTP(ValidateEmailRequest request, CallContext context = default)
        {
            var validateOTPReply = AsyncContext.Run(() => _CommunicationManager.EmailValidateOTP(request));
            return Task.FromResult(validateOTPReply);
        }
        public Task<ValidateEmailReply> ExistValidOTP(string MobileNo, CallContext context = default)
        {
            var existOtp = AsyncContext.Run(() => _CommunicationManager.ExistValidOTP(MobileNo));
            return Task.FromResult(existOtp);
        }

    }
}
