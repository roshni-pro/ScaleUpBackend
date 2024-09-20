using System.Net;

namespace ScaleUP.Services.CommunicationAPI.Helper
{
    public static class WhatsappMessageHelper
    {


        public static bool SendWhastapp (string filePath)
        {
            string WhasAppAPIAurl = "https://automate.botbaba.io/webhook/WA360Broadcast";



            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            WebRequest request = WebRequest.Create(WhasAppAPIAurl);
            request.Method = "POST";
            byte[] byteArray = File.ReadAllBytes(filePath);
            request.ContentType = "application/octet-stream";
            request.ContentLength = byteArray.Length;
            request.Headers.Add("Content-Disposition", $"filename*=UTF-8''{Path.GetFileName(filePath)}");
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            WebResponse response = request.GetResponse();
            var status = ((HttpWebResponse)response).StatusDescription;


          
            return true;

        }
    }
}
