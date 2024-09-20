using Microsoft.AspNetCore.Mvc;
using ScaleUp.Services.CommunicationAPI.Persistence;
using System.Data;
using ScaleUP.Services.CommunicationAPI.Helper;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Communication.DataContracts;
using ScaleUP.Services.CommunicationModels;
using ScaleUP.Services.CommunicationAPI.Constants;

namespace ScaleUP.Services.CommunicationAPI.Manager
{
    public class CommunicationManager
    {
        private readonly IConfiguration _configuration;

        private readonly CommunicationDbContext _context;

        public CommunicationManager(CommunicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;

        }

        public async Task<SendWhatsappReply> SendWhatsapp(int TemplateId, string TemplateJson, string ToNumber, string filePath)
        {
            SendWhatsappReply sendWhatsappReply = new SendWhatsappReply();
            //filePath = string.Empty;
            if (TemplateId > 0 && ToNumber != null)
            {
                //WhatsAppTemplateManager whatsAppTemplateManager = new WhatsAppTemplateManager();
                List<WhatsAppTemplateExcelDc> excelDataList = new List<WhatsAppTemplateExcelDc>();
                WhatsAppTemplateExcelDc excelData = new WhatsAppTemplateExcelDc
                {
                    API_KEY = _configuration.GetValue<string>("AppSettings:WhatsAppAPIKeyValue"),
                    From_Number = _configuration.GetValue<string>("AppSettings:WhatsAppAPIFromNumber"),
                    Template_JSON = TemplateJson,
                    To_Number = ToNumber
                };
                excelDataList.Add(excelData);
                DataTable dashboardDt = ClassToDataTable.CreateDataTable<WhatsAppTemplateExcelDc>(excelDataList);

                if (excelDataList != null && excelDataList.Count > 0)
                {
                    dashboardDt = new DataTable();
                    dashboardDt.Columns.Add("From Number");
                    dashboardDt.Columns.Add("API_KEY");
                    dashboardDt.Columns.Add("To Number");
                    dashboardDt.Columns.Add("Template JSON");
                    foreach (var item in excelDataList)
                    {
                        DataRow row = dashboardDt.NewRow();
                        row[0] = item.From_Number;
                        row[1] = item.API_KEY;
                        row[2] = item.To_Number;
                        row[3] = item.Template_JSON;
                        dashboardDt.Rows.Add(row);
                    }
                }

                ExcelGeneratorHelper.DataTable_To_Excel(dashboardDt, "UploadedWhatsAppExcel", filePath);

                if (!string.IsNullOrEmpty(filePath))
                {
                    bool result = WhatsappMessageHelper.SendWhastapp(filePath);
                }
            }
            return sendWhatsappReply;
        }

        public async Task<SendSMSReply> SendSMS(SendSMSRequest request)
        {
            SendSMSReply sendSMSReply = new SendSMSReply();
            sendSMSReply.Status = false;
            sendSMSReply.Message = "SMS not send.";
            await ExistValidOTP(request.MobileNo);
           SendOTPDetails _sendOTPDetail = new SendOTPDetails
            {
                MobileNo = request.MobileNo,
                SMS = request.SMS,
                OTP = request.OTP,
                Comment = "",
                ExpiredInMin = request.ExpiredInMin,
                IsSent = false,
                IsValidated = true,
                IsActive = true,
                IsDeleted = false
            };
            _context.SendOTPDetails.Add(_sendOTPDetail);
            int rowchanged = _context.SaveChanges();
            if (rowchanged > 0)
            {
                var smsUrl = EnvironmentConstants.SMSUrl; //_configuration.GetValue<string>("AppSettings:SMSUrl");
                if (SendSMSHelper.SendSMS(smsUrl, _sendOTPDetail.MobileNo, _sendOTPDetail.SMS, request.routeId, request.DLTId))
                {
                    sendSMSReply.Status = true;
                    sendSMSReply.Message = "SMS send successfully.";
                    _sendOTPDetail.IsSent = true;
                    _context.Entry(_sendOTPDetail).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    _context.SaveChanges();
                }
            }

            return sendSMSReply;
        }

        public async Task<ValidateOTPReply> ValidateOTP(ValidateOTPRequest request)
        {
            Global.Infrastructure.Helper.DateConvertHelper dateConvertHelper = new Global.Infrastructure.Helper.DateConvertHelper();


            ValidateOTPReply validateOTPReply = new ValidateOTPReply();
            validateOTPReply.Status = false;
            validateOTPReply.Message = "OTP not valid.";
            var sendOtpDetail = _context.SendOTPDetails.FirstOrDefault(x => x.IsSent && x.MobileNo == request.MobileNo && x.OTP == request.OTP && x.IsActive && x.IsValidated);
            if (sendOtpDetail != null)
            {
                sendOtpDetail.IsValidated = false;
                _context.Entry(sendOtpDetail).State = EntityState.Modified;
                _context.SaveChanges();
                validateOTPReply.Status = true;
                validateOTPReply.Message = "OTP valid.";

                var currentCreated = sendOtpDetail.Created.Minute;
                var dt = dateConvertHelper.GetIndianStandardTime().Minute;
                var diffcr = dt - currentCreated;
                var cmp = diffcr > sendOtpDetail.ExpiredInMin.Value;
                //if (!(sendOtpDetail.ExpiredInMin.HasValue
                //    && sendOtpDetail.Created.AddMinutes(sendOtpDetail.ExpiredInMin.Value).Subtract(sendOtpDetail.Created).TotalMinutes > sendOtpDetail.ExpiredInMin.Value))
                if (sendOtpDetail.ExpiredInMin.HasValue
                && diffcr > sendOtpDetail.ExpiredInMin.Value)
                {
                    validateOTPReply.Status = false;
                    validateOTPReply.Message = "OTP expired. Please resend again";
                }
            }

            return validateOTPReply;
        }

        public async Task<SendEmailReply> SendEmail(SendEmailRequest request)
        {
            SendEmailReply sendEmailReply = new SendEmailReply();
            request.From = EnvironmentConstants.MasterEmail; //_configuration.GetValue<string>("AppSettings:FromEmailAddress");
            var MasterEmail = EnvironmentConstants.MasterEmail;// _configuration.GetValue<string>("AppSettings:MasterEmail");
            var MasterPassword = EnvironmentConstants.MasterPassword; //_configuration.GetValue<string>("AppSettings:MasterPassword");
            if (MasterEmail != null && MasterPassword != null)
            {
                if (EmailHelper.SendMail(MasterEmail, MasterPassword, request))
                {
                    sendEmailReply.Status = true;
                    sendEmailReply.Message = "Email Send Successfully.";
                }
            }
            return sendEmailReply;
        }

        public async Task<EmailVerifyReply> SendOTPOnEmail(SendEmailRequest request)
        {
            EmailVerifyReply emailVerifyReply = new EmailVerifyReply();
            request.From = EnvironmentConstants.MasterEmail; //_configuration.GetValue<string>("AppSettings:FromEmailAddress");
            var MasterEmail = EnvironmentConstants.MasterEmail;// _configuration.GetValue<string>("AppSettings:MasterEmail");
            var MasterPassword = EnvironmentConstants.MasterPassword; //_configuration.GetValue<string>("AppSettings:MasterPassword");
            if (MasterEmail != null && MasterPassword != null)
            {
                if (EmailHelper.SendMail(MasterEmail, MasterPassword, request))
                {
                    var existEmailOtp = _context.SendEmailOtpDetails.FirstOrDefault(x => x.IsValidated && x.IsSent && x.Email == request.To && x.IsActive && !x.IsDeleted);
                    if (existEmailOtp != null)
                    {
                        existEmailOtp.IsValidated = false;
                        _context.SaveChanges();
                    }
                    SendEmailOtpDetails _sendEmailOtpDetails = new SendEmailOtpDetails
                    {
                        Email = request.To,
                        Message = request.Message,
                        OTP = request.OTP,
                        Comment = "",
                        ExpiredInMin = 10,
                        IsSent = true,
                        IsValidated = true,
                        IsActive = true,
                        IsDeleted = false
                    };
                    _context.SendEmailOtpDetails.Add(_sendEmailOtpDetails);
                    int rowchanged = _context.SaveChanges();
                    if (rowchanged > 0)
                    {
                        emailVerifyReply.Status = true;
                        emailVerifyReply.Message = "Email Send Successfully.";
                    }
                }
            }
            return emailVerifyReply;
        }

        public async Task<ValidateEmailReply> EmailValidateOTP(ValidateEmailRequest request)
        {
            ValidateEmailReply validateEmailReply = new ValidateEmailReply();
            validateEmailReply.Status = false;
            validateEmailReply.Message = "OTP not valid.";
            var sendOtpDetail = _context.SendEmailOtpDetails.FirstOrDefault(x => x.IsSent && x.Email == request.Email && x.OTP == request.OTP && x.IsActive && x.IsValidated);
            if (sendOtpDetail != null)
            {
                sendOtpDetail.IsValidated = false;
                _context.Entry(sendOtpDetail).State = EntityState.Modified;
                _context.SaveChanges();
                validateEmailReply.Status = true;
                validateEmailReply.Message = "OTP valid.";
            }

            return validateEmailReply;
        }


        public async Task<ValidateEmailReply> ExistValidOTP(string MobileNo)
        {
            ValidateEmailReply validateEmailReply = new ValidateEmailReply();
            validateEmailReply.Status = false;
            validateEmailReply.Message = "OTP not valid.";
            var sendOtpDetails = _context.SendOTPDetails.Where(x => x.IsSent && x.MobileNo == MobileNo && x.IsActive && x.IsValidated);
            if (sendOtpDetails != null && sendOtpDetails.Any())
            {
                foreach (var sendOtpDetail in sendOtpDetails)
                {
                    sendOtpDetail.IsValidated = false;
                    _context.Entry(sendOtpDetail).State = EntityState.Modified;
                }
                _context.SaveChanges();

            }
            validateEmailReply.Status = true;
            validateEmailReply.Message = "OTP valid.";
            return validateEmailReply;
        }


    }
    public class SendWhatsappReply
    {
        public bool Status { get; set; }

        public string Message { get; set; }
    }
    public class WhatsAppTemplateExcelDc
    {

        public string From_Number { get; set; }

        public string API_KEY { get; set; }

        public string To_Number { get; set; }
        public string Template_JSON { get; set; }



    }
}


