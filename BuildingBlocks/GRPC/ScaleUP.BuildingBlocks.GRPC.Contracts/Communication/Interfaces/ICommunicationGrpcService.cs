using ProtoBuf.Grpc;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Communication.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Communication.Interfaces
{
    [ServiceContract]
    public interface ICommunicationGrpcService
    {
        [OperationContract]
        Task<SendSMSReply> SendSMS(SendSMSRequest request,
            CallContext context = default);

        [OperationContract]
        Task<ValidateOTPReply> ValidateOTP(ValidateOTPRequest request,
            CallContext context = default);

        [OperationContract]
        Task<SendEmailReply> SendEmail(SendEmailRequest request,
            CallContext context = default);

        [OperationContract]
        Task<EmailVerifyReply> SendOTPOnEmail(SendEmailRequest request,
        CallContext context = default);

        [OperationContract]
        Task<ValidateEmailReply> EmailValidateOTP(ValidateEmailRequest request,
           CallContext context = default);

        [OperationContract]
        Task<ValidateEmailReply> ExistValidOTP(string MobileNo,
         CallContext context = default);


    }
}
