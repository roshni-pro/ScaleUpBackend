using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScaleUP.Services.LeadAPI.Persistence;
using System.Net;
using System.Text;

namespace ScaleUP.Services.LeadAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FileViewerController : ControllerBase
    {
        private IHostEnvironment _hostingEnvironment;
        private readonly LeadApplicationDbContext _context;

        public FileViewerController(IHostEnvironment hostingEnvironment, LeadApplicationDbContext context)
        {
            _hostingEnvironment = hostingEnvironment;
            _context = context;
        }

        [HttpGet]
        [Route("GetThirdPartyAPIConfigCode")]
        public async Task<List<DropDown>> GetThirdPartyAPIConfigCode()
        {
            return await _context.ThirdPartyApiConfigs.Where(x => x.IsActive && !x.IsDeleted).Select(x => new DropDown
            {
                Id = x.Id,
                Label = x.Code
            }).ToListAsync();

        }


        [HttpPost]
        [Route("GetFileContent")]
        public async Task<List<ReqResVM>> GetFileContent(FileContentInput input)
        {
            var query = from re in _context.ThirdPartyRequests
                        join l in _context.Leads on re.LeadId equals l.Id
                        where re.ThirdPartyApiConfigId == input.ThirdPartyAPIConfigId
                        && l.UserName == input.UserId
                        select new ReqResVM
                        {
                            Request = re.Request,
                            Response = re.Response,
                            ProcessedResponse = re.ProcessedResponse,
                            ProcessedResponseData = "",
                            RequestData = "",
                            ResponseData = "",
                            CreatedDate = re.Created
                        };

            var data = query.ToList();

            if (data != null && data.Any())
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

        public class ReqResVM
        {
            public string Request { get; set; }
            public string Response { get; set; }
            public string ProcessedResponse { get; set; }
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
            catch (Exception ex)
            {

            }

            return "";
        }
    }
}
