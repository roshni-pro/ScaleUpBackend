
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.Internal;
using Nito.AsyncEx;
using ScaleUp.Services.CommunicationAPI.Persistence;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Communication.DataContracts;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Services.CommunicationAPI.Helper;
using ScaleUP.Services.CommunicationAPI.Manager;

using System.Data;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace ScaleUP.Services.CommunicationAPI.Controllers
{
    [ApiController]
    public class CommunicationControllers : BaseController
    {
        private readonly CommunicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly CommunicationManager _CommunicationManager;
        private IHostEnvironment _hostingEnvironment;
        public CommunicationControllers(CommunicationDbContext context, IConfiguration configuration, IHostEnvironment hostingEnvironment, CommunicationManager commManager)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment; 
            _configuration = configuration;
            _CommunicationManager = commManager;
        }

        [HttpGet]
        [Route("SendWhatsappMessage")]
       
        public async Task<SendWhatsappReply> SendWhatsappMessage(int TemplateId, string TemplateJson, string ToNumber)
        {
            //string ExcelSavePath = HttpContext.Request.Path("~/UploadedWhatsAppExcel");
            //string ExcelSavePath = _hostingEnvironment.ContentRootPath;
            string FolderPath = _hostingEnvironment.ContentRootPath+"\\"+ "WhatsAppExcel";


            if (!Directory.Exists(FolderPath))
                Directory.CreateDirectory(FolderPath);
            var fileName = "WhatsAppExcelData_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xlsx";
            string filePath = FolderPath + "\\" + fileName;
            var res= await _CommunicationManager.SendWhatsapp(TemplateId, TemplateJson, ToNumber, filePath);
            return res;


        }

        [HttpPost]
        [Route("SendSMS")]
        [AllowAnonymous]
        public Task<SendSMSReply> SendSMS(SendSMSRequest request)
        {
            var sendSMSReply = AsyncContext.Run(() => _CommunicationManager.SendSMS(request));
            return Task.FromResult(sendSMSReply);
        }

    }
}
