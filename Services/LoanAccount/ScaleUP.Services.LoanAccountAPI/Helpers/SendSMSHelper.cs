using ScaleUP.Services.LoanAccountAPI.Persistence;
using System.Net;
using System.Web;

namespace ScaleUP.Services.LoanAccountAPI.Helpers
{
    public class SendSMSHelper
    {
        private readonly LoanAccountApplicationDbContext _context;

        public SendSMSHelper() { }
        public static bool SendSMS(string smsUrl, string mobile, string message, string routeId, string DLTId)
        {
            bool result = false;
            try
            {
                if (string.IsNullOrEmpty(routeId)) routeId = "1";
                if (!string.IsNullOrEmpty(mobile) && !string.IsNullOrEmpty(message))
                {
                    string path = smsUrl.Replace("[[Mobile]]", mobile).Replace("[[DLTId]]", DLTId).Replace("[[RouteId]]", routeId).Replace("[[Message]]", HttpUtility.UrlEncode(message));

                    var webRequest = (HttpWebRequest)WebRequest.Create(path);
                    webRequest.Method = "GET";
                    webRequest.ContentType = "application/json";
                    webRequest.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:28.0) Gecko/20100101 Firefox/28.0";
                    webRequest.ContentLength = 0;
                    webRequest.Credentials = CredentialCache.DefaultCredentials;
                    webRequest.Accept = "*/*";
                    using (var webResponse = (HttpWebResponse)webRequest.GetResponse())
                    {
                        if (webResponse.StatusCode == HttpStatusCode.OK)
                            result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                string error = ex.InnerException != null ? ex.ToString() + Environment.NewLine + ex.InnerException.ToString() : ex.ToString();

            }

            return result;


        }
    }
}
