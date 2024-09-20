using Elasticsearch.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.KYCActivity;
using ScaleUP.Services.KYCAPI.KYCFactory;
using ScaleUP.Services.KYCAPI.Persistence;
using ScaleUP.Services.KYCDTO.Transacion;
using System.Net;
using System.Text;

namespace ScaleUP.Services.KYCAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FileViewerController : ControllerBase
    {
        private IHostEnvironment _hostingEnvironment;
        private readonly ApplicationDbContext _context;


        public FileViewerController(IHostEnvironment hostingEnvironment, ApplicationDbContext context)
        {
            _hostingEnvironment = hostingEnvironment;
            _context = context;
        }

        [HttpGet]
        [Route("GetThirdPartyAPIConfigCode")]
        public async Task<List<DropDown>> GetThirdPartyAPIConfigCode()
        {
            return await _context.ThirdPartyAPIConfigs.Where(x => x.IsActive && !x.IsDeleted).Select(x => new DropDown
            {
                Id = x.Id,
                Label = x.Code
            }).ToListAsync();

        }


        [HttpPost]
        [Route("GetFileContent")]
        public async Task<List<ReqResVM>> GetFileContent(FileContentInput input)
        {
            var data = await _context.ApiRequestResponse
                .Where(x => x.UserId == input.UserId && x.APIConfigId == input.ThirdPartyAPIConfigId)
                .OrderByDescending(x => x.Id)
                .Select(x => new ReqResVM
                {
                    Request = x.Request,
                    Response = x.Response,
                    ProcessedResponse = x.ProcessedResponse,
                    ProcessedResponseData = "",
                    RequestData = "",
                    ResponseData = "",
                    CreatedDate = x.Created
                }).ToListAsync();

            if(data != null && data.Any())
            {
                foreach (var item in data)
                {
                    item.ProcessedResponseData = GetStreamFromUrl(item.ProcessedResponse);
                    item.RequestData = GetStreamFromUrl(item.Request);
                    item.ResponseData = GetStreamFromUrl(item.Response);
                }
            }
            return data;
        }

        public class DropDown
        {
            public long Id { get; set; }
            public string Label { get; set; }
        }

        public class FileContentInput
        {
            public string UserId { get; set; }
            public long ThirdPartyAPIConfigId { get; set; }
        }

        public class ReqResVM{
            public string Request{ get; set; }
            public string Response{ get; set; }
            public string ProcessedResponse{ get; set; }
            public DateTime CreatedDate { get; set; }

            public string RequestData { get; set; }
            public string ResponseData { get; set; }
            public string ProcessedResponseData { get; set; }

        }


        private static string GetStreamFromUrl(string url)
        {
            //byte[] imageData = null;
            //using (var wc = new System.Net.WebClient())
            //    imageData = wc.DownloadData(url);
            //string text = System.Text.Encoding.UTF8.GetString(imageData);
            //return text;

            try
            {
                var webRequest = WebRequest.Create(url);

                using (var response = webRequest.GetResponse())
                using (var content = response.GetResponseStream())
                using (var reader = new StreamReader(content, Encoding.Unicode))
                {
                    var strContent = reader.ReadToEnd();

                    return strContent;


                }
            }
            catch(Exception ex)
            {

            }

            return "";
        }
    }


   

}
