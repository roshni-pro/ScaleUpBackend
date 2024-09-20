using Grpc.Net.Client;
using Microsoft.Net.Http.Headers;
using ProtoBuf.Grpc.Client;
using ScaleUP.ApiGateways.Aggregator.Constants;
using ScaleUP.ApiGateways.Aggregator.Extensions;
using ScaleUP.ApiGateways.Aggregator.Services.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Communication.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Communication.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Location.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Location.Interfaces;

namespace ScaleUP.ApiGateways.Aggregator.Services
{
    public class CommunicationService : ICommunicationService
    {
        private IConfiguration Configuration;
        private readonly ICommunicationGrpcService _client;

        public CommunicationService(IConfiguration _configuration,
            ICommunicationGrpcService client)
        {
            Configuration = _configuration;
            _client = client;
           
        }
        public async Task<SendSMSReply> SendSMS(SendSMSRequest request)
        {
            var reply = await _client.SendSMS(request);
            return reply;
        }

        public async Task<ValidateOTPReply> ValidateOTP(ValidateOTPRequest request)
        {
            var reply = await _client.ValidateOTP(request);
            return reply;
        }
        public async Task<SendEmailReply> SendEmail(SendEmailRequest request)
        {           
            var reply = await _client.SendEmail(request);
            return reply;
        }
        public async Task<EmailVerifyReply> SendOTPOnEmail(SendEmailRequest request)
        {
            var reply = await _client.SendOTPOnEmail(request);
            return reply;
        }
        public async Task<ValidateEmailReply> EmailValidateOTP(ValidateEmailRequest request)
        {
            var reply = await _client.EmailValidateOTP(request);
            return reply;
        }
        public async Task<ValidateEmailReply> ExistValidOTP(string MobileNo)
        {
            var reply = await _client.ExistValidOTP(MobileNo);
            return reply;
        }
    }
}
