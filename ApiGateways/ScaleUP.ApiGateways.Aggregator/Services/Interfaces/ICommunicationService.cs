using ScaleUP.BuildingBlocks.GRPC.Contracts.Communication.DataContracts;

namespace ScaleUP.ApiGateways.Aggregator.Services.Interfaces
{
    public interface ICommunicationService
    {
        Task<SendSMSReply> SendSMS(SendSMSRequest request);
        Task<ValidateOTPReply> ValidateOTP(ValidateOTPRequest request);
        Task<SendEmailReply> SendEmail(SendEmailRequest request);
        Task<EmailVerifyReply> SendOTPOnEmail(SendEmailRequest request);
        Task<ValidateEmailReply> EmailValidateOTP(ValidateEmailRequest request);
        Task<ValidateEmailReply> ExistValidOTP(string MobileNo);
    }
}
