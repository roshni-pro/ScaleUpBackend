using Newtonsoft.Json;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Media.DataContracts;
using ScaleUP.Services.LeadAPI.Manager;
using ScaleUP.Services.LeadAPI.Persistence;
using ScaleUP.Services.LeadDTO.Constant;
using ScaleUP.Services.LeadDTO.ThirdApiConfig;
using System.Net;
using System.Net.Http.Headers;

namespace ScaleUP.Services.LeadAPI.Helper
{
    public class GenerateNBFCAgreementHelper
    {
        private readonly LeadApplicationDbContext _context;
        private ThirdPartyApiConfigManager thirdPartyApiConfigManager;
        public GenerateNBFCAgreementHelper(LeadApplicationDbContext context)
        {
            _context = context;
            thirdPartyApiConfigManager = new ThirdPartyApiConfigManager(_context);
        }

        private static string[] units = { "Zero", "One", "Two", "Three","Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven",
                                          "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
        private static string[] tens = { "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

        public async Task<GRPCReply<FileUploadRequest>> GenerateAgreement(GRPCRequest<NBFCAgreement> data)
        {
            GRPCReply<FileUploadRequest> res = new GRPCReply<FileUploadRequest>();

            FileUploadRequest fileObj = new FileUploadRequest();

            string htmldata = "";
            string replacetext = "";
            string path = "";
            string newfilepath = "";

            path = data.Request.CustomerAgreementURL;

            

            byte[] file = null;

            using (var wc = new WebClient())
            {
                file = wc.DownloadData(path);
            }

            if (file.Length > 0)
            {
                htmldata = System.Text.Encoding.UTF8.GetString(file);

                if (!string.IsNullOrEmpty(htmldata))
                {
                    string amount = ConvertNumbertoWords(data.Request.CreditLimit);

                    double gstRate = data.Request.GSTRate;
                    double CreditLimit = data.Request.CreditLimit;
                    double ProcessingFees = data.Request.ProcessingFees;
                    double PFGST = 0;

                    if (data.Request.ProcessingFeeType == "Percentage")
                    {
                        ProcessingFees = Math.Round(((CreditLimit * ProcessingFees) / 100), 2);
                        PFGST = Math.Round((ProcessingFees * gstRate) / 100, 2);
                    }
                    else
                    {
                        PFGST = Math.Round((ProcessingFees * gstRate) / 100, 2);
                    }

                    data.Request.ProcessingFees = ProcessingFees;
                    data.Request.NetPF = ProcessingFees + PFGST;

                    replacetext = $"{data.Request.NameOfBorrower} ";
                    htmldata = htmldata.Replace("{{NameOfBorrower}}", replacetext);

                    replacetext = $"{data.Request.LeadCode} ";
                    htmldata = htmldata.Replace("{{LeadId}}", replacetext);

                    replacetext = $"{data.Request.FatherName} ";
                    htmldata = htmldata.Replace("{{FatherName}}", replacetext);

                    if (data.Request.GstNumber != null && data.Request.GstNumber != "")
                    {
                        replacetext = $"{data.Request.GstNumber} "; //Rate of Interest
                    }
                    else
                    {
                        replacetext = "NA"; //Rate of Interest
                    }

                    htmldata = htmldata.Replace("{{GstNumber}}", replacetext);

                    replacetext = $" {data.Request.CreditLimit} ";
                    htmldata = htmldata.Replace("{{CreditLimit}}", replacetext);

                    replacetext = amount;
                    htmldata = htmldata.Replace("{{CreditAmountWord}}", replacetext);

                    replacetext = $"{data.Request.TransactionConvenienceFeeRate} ";
                    htmldata = htmldata.Replace("{{TransactionConvenienceFeeRate}}", replacetext);

                    replacetext = $"{data.Request.ProcessingFees} ";
                    htmldata = htmldata.Replace("{{ProcessingFees}}", replacetext);

                    replacetext = $"{data.Request.DiscountAmount} ";
                    htmldata = htmldata.Replace("{{DiscountAmount}}", replacetext);

                    replacetext = $"{data.Request.address1} ";
                    htmldata = htmldata.Replace("{{address1}}", replacetext);

                    replacetext = $"{data.Request.address2} ";
                    htmldata = htmldata.Replace("{{address2}}", replacetext);

                    replacetext = $"{data.Request.NetPF} ";
                    htmldata = htmldata.Replace("{{NetPF}}", replacetext);

                    replacetext = $"{data.Request.DelayPenaltyFeeRate} ";
                    htmldata = htmldata.Replace("{{DelayPenaltyFeeRate}}", replacetext);

                    replacetext = $"{data.Request.BounceCharge} ";
                    htmldata = htmldata.Replace("{{BounceCharge}}", replacetext);

                    replacetext = $"{data.Request.ApplicationDate} ";
                    htmldata = htmldata.Replace("{{ApplicationDate}}", replacetext);

                    replacetext = $"{data.Request.CompanyLogo} ";
                    htmldata = htmldata.Replace("{{img}}", replacetext);


                }
            }
            if (!string.IsNullOrEmpty(htmldata))
            {
                byte[] pdf = null;

             
                if (pdf != null && pdf.Length > 0)
                {
                    fileObj.FileStream = pdf;
                    fileObj.IsValidForLifeTime = true;
                    fileObj.ValidityInDays = 1;
                    fileObj.SubFolderName = "CustomerAgreement";
                    fileObj.FileExtensionWithoutDot = "pdf";
                    fileObj.html = htmldata;

                    res.Status = true;
                    res.Message = "success";
                    res.Response = fileObj;
                }
                else
                {
                    fileObj.FileStream = pdf;
                    fileObj.IsValidForLifeTime = true;
                    fileObj.ValidityInDays = 1;
                    fileObj.SubFolderName = "CustomerAgreement";
                    fileObj.FileExtensionWithoutDot = "pdf";
                    fileObj.html = htmldata;

                    res.Status = false;
                    res.Message = "Success";
                    res.Response = fileObj;
                }

            }
            return res;
        }

        public static string ConvertNumbertoWords(double amount)
        {
            Int64 amount_int = (Int64)amount;
            Int64 amount_dec = (Int64)Math.Round((amount - (double)(amount_int)) * 100);
            if (amount_dec == 0)
            {

                string rrs = ConvertInNumber(amount_int) + " Only.";
                return rrs;
            }
            else
            {
                string res = ConvertInNumber(amount_int) + " Point " + ConvertInNumber(amount_dec) + " Only.";
                return res;
            }
        }
        public static string ConvertInNumber(Int64 i)
        {

            if (i < 20)
            {
                return units[i];
            }
            if (i < 100)
            {
                return tens[i / 10] + ((i % 10 > 0) ? " " + ConvertInNumber(i % 10) : "");
            }
            if (i < 1000)
            {
                return units[i / 100] + " Hundred"
                        + ((i % 100 > 0) ? " And " + ConvertInNumber(i % 100) : "");
            }
            if (i < 100000)
            {
                return ConvertInNumber(i / 1000) + " Thousand "
                + ((i % 1000 > 0) ? " " + ConvertInNumber(i % 1000) : "");
            }
            if (i < 10000000)
            {
                return ConvertInNumber(i / 100000) + " Lakh "
                        + ((i % 100000 > 0) ? " " + ConvertInNumber(i % 100000) : "");
            }
            if (i < 1000000000)
            {
                return ConvertInNumber(i / 10000000) + " Crore "
                        + ((i % 10000000 > 0) ? " " + ConvertInNumber(i % 10000000) : "");
            }
            return ConvertInNumber(i / 1000000000) + " Arab "
                    + ((i % 1000000000 > 0) ? " " + ConvertInNumber(i % 1000000000) : "");
        }

    }
}
public class HtmltoPdfDc
{
    public string Html { get; set; }
}
public class Response
{
    public byte[] result { get; set; }
}