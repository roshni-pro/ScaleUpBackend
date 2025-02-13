﻿using Microsoft.EntityFrameworkCore;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Media.DataContracts;
using ScaleUP.Services.MediaAPI.Constants;
using ScaleUP.Services.MediaAPI.Helper;
using ScaleUP.Services.MediaAPI.Persistence;
using ScaleUP.Services.MediaDTO.Transaction;
using ScaleUP.Services.MediaDTO.Transaction.DocumentConverter;
using ScaleUP.Services.MediaModels.Transaction;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Wkhtmltopdf.NetCore;
using static ScaleUP.Services.MediaAPI.Controllers.DocController;

namespace ScaleUP.Services.MediaAPI.Manager
{
    public class MediaGrpcManager
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _hostingEnvironment;
        public MediaGrpcManager(ApplicationDbContext context, IConfiguration configuration, IHostEnvironment hostingEnvironment)
        {
            _context = context;
            configuration = _configuration;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<DocReply> GetMediaDetail(DocRequest request)
        {
            DocReply docReply = new DocReply();
            docReply.Status = false;
            var doc = _context.DocInfos.FirstOrDefault(x => x.Id == request.DocId && x.IsActive && !x.IsDeleted);
            string path = doc.RelativePath;
            if (!string.IsNullOrEmpty(path) && !path.Contains("https://") && !path.Contains("http://"))
            {
                path = EnvironmentConstants.MediaServiceBaseURL + doc.RelativePath + doc.Name + "." + doc.FileExtension;
            }
            if (doc != null)
            {
                docReply.Status = true;
                docReply.ImagePath = path;
                docReply.Id = doc.Id;
            }
            return docReply;
        }



        public async Task<DocReply> SaveFileAzure(FileUploadRequest request)
        {

            DocReply docReply = new DocReply();

            var filestream = new MemoryStream(request.FileStream);
            string fileName = Guid.NewGuid().ToString();
            string fileExt = request.FileExtensionWithoutDot;
            string filefullName = fileName + "." + fileExt;
            //string filePath = "";
            //using (var ms = new MemoryStream())
            //{
            //    //await fileDetails.FileDetails.CopyToAsync(ms);
            //    //ms.Seek(0, SeekOrigin.Begin);
            //    filePath = await AzureBlobService.UploadFile(ms, filefullName);
            //}
            string filePath = await AzureBlobService.UploadFile(filestream, filefullName);

            //string uploadFilePath = Path.Combine(_hostingEnvironment.ContentRootPath, "wwwroot", "Uploads", request.IsValidForLifeTime ? "LifeTime" : "Ordinary", string.IsNullOrEmpty(request.SubFolderName) ? "" : request.SubFolderName);

            //if (!Directory.Exists(uploadFilePath))
            //{
            //    Directory.CreateDirectory(uploadFilePath);
            //}

            DocInfo docInfo = new DocInfo
            {
                IsActive = true,
                IsDeleted = false,
                IsValidForLifeTime = request.IsValidForLifeTime,
                FileExtension = fileExt,
                Name = fileName,
                RelativePath = filePath,  //"/Uploads/" + (request.IsValidForLifeTime ? "LifeTime" : "Ordinary") + "/" + (string.IsNullOrEmpty(request.SubFolderName) ? "" : $"{request.SubFolderName}/"),
                ValidityInDays = request.IsValidForLifeTime ? null : request.ValidityInDays,
            };

            //string filePath = Path.Combine(uploadFilePath, docInfo.Name + "." + docInfo.FileExtension);

            //FileStream file = File.Create(filePath);
            //file.Write(request.FileStream, 0, request.FileStream.Length);
            //file.Close();

            _context.DocInfos.Add(docInfo);
            _context.SaveChanges();

            if (docInfo.Id > 0)
            {
                // var imageObj = await GetFilePathPrivate((int)docInfo.Id);
                docReply.Status = true;
                docReply.ImagePath = docInfo.RelativePath;
                docReply.Id = docInfo.Id;
            }
            else
            {
                docReply.Status = false;
                docReply.ImagePath = "";
                docReply.Id = 0;
            }
            return docReply;
        }


        public async Task<ImagePath> GetFilePathPrivateAzure(int id)
        {
            ImagePath imagePath = new ImagePath();
            if (id < 1)
            {
                return imagePath;
            }

            try
            {
                var docInfo = await _context.DocInfos.FirstOrDefaultAsync(x => x.Id == id);

                //string path = "";
                if (docInfo != null)
                {
                    //AppSettingHelper.ThisHostBaseURL() +
                    imagePath.Path = docInfo.RelativePath;
                    //EnvironmentConstants.MediaServiceBaseURL + docInfo.RelativePath + docInfo.Name + "." + docInfo.FileExtension;
                    return imagePath;
                }
                else
                {
                    return imagePath;
                }

            }
            catch (Exception)
            {
                throw;
            }
        }



        public async Task<DocReply> SaveFileLocal(FileUploadRequest request)
        {

            DocReply docReply = new DocReply();
            string uploadFilePath = Path.Combine(_hostingEnvironment.ContentRootPath, "wwwroot", "Uploads", request.IsValidForLifeTime ? "LifeTime" : "Ordinary", string.IsNullOrEmpty(request.SubFolderName) ? "" : request.SubFolderName);

            if (!Directory.Exists(uploadFilePath))
            {
                Directory.CreateDirectory(uploadFilePath);
            }

            DocInfo docInfo = new DocInfo
            {
                IsActive = true,
                IsDeleted = false,
                IsValidForLifeTime = request.IsValidForLifeTime,
                FileExtension = request.FileExtensionWithoutDot,
                Name = Guid.NewGuid().ToString(),
                RelativePath = "/Uploads/" + (request.IsValidForLifeTime ? "LifeTime" : "Ordinary") + "/" + (string.IsNullOrEmpty(request.SubFolderName) ? "" : $"{request.SubFolderName}/"),
                ValidityInDays = request.IsValidForLifeTime ? null : request.ValidityInDays,
            };

            string filePath = Path.Combine(uploadFilePath, docInfo.Name + "." + docInfo.FileExtension);

            FileStream file = File.Create(filePath);
            file.Write(request.FileStream, 0, request.FileStream.Length);
            file.Close();

            _context.DocInfos.Add(docInfo);
            _context.SaveChanges();

            if (docInfo.Id > 0)
            {
                var imageObj = await GetFilePathPrivate((int)docInfo.Id);
                docReply.Status = true;
                docReply.ImagePath = imageObj.Path;
                docReply.Id = docInfo.Id;
            }
            else
            {
                docReply.Status = false;
                docReply.ImagePath = "";
                docReply.Id = 0;
            }
            return docReply;
        }


        public async Task<ImagePath> GetFilePathPrivateLocal(int id)
        {
            ImagePath imagePath = new ImagePath();
            if (id < 1)
            {
                return imagePath;
            }

            try
            {
                var docInfo = await _context.DocInfos.FirstOrDefaultAsync(x => x.Id == id);

                //string path = "";
                if (docInfo != null)
                {
                    //AppSettingHelper.ThisHostBaseURL() +
                    imagePath.Path = EnvironmentConstants.MediaServiceBaseURL + docInfo.RelativePath + docInfo.Name + "." + docInfo.FileExtension;
                    return imagePath;
                }
                else
                {
                    return imagePath;
                }

            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<DocReply> SaveFile(FileUploadRequest request)
        {
            if (string.IsNullOrEmpty(EnvironmentConstants.EnvironmentName) || EnvironmentConstants.EnvironmentName.ToLower() == "development")
            {
                return await SaveFileLocal(request);
            }
            else
            {
                return await SaveFileAzure(request);
            }
        }

        public async Task<ImagePath> GetFilePathPrivate(int id)
        {

            if (string.IsNullOrEmpty(EnvironmentConstants.EnvironmentName) || EnvironmentConstants.EnvironmentName.ToLower() == "development")
            {
                return await GetFilePathPrivateLocal(id);
            }
            else
            {
                return await GetFilePathPrivateAzure(id);
            }
        }



        public async Task<List<DocReply>> GetMediaDetails(MultiDocRequest request)
        {

            List<DocReply> docReplyList = null;
            var query = from d in _context.DocInfos
                        join r in request.DocIdList on d.Id equals r
                        select d;

            var docs = query.ToList();
            if (docs != null && docs.Count > 0)
            {
                docReplyList = new List<DocReply>();
                foreach (var doc in docs)
                {
                    DocReply docReply = new DocReply();
                    docReplyList.Add(docReply);
                    string path = doc.RelativePath;
                    if (!string.IsNullOrEmpty(path) && !path.Contains("https://") && !path.Contains("http://"))
                    {
                        path = EnvironmentConstants.MediaServiceBaseURL + doc.RelativePath + doc.Name + "." + doc.FileExtension;
                    }
                    if (doc != null)
                    {
                        docReply.Status = true;
                        docReply.ImagePath = path;
                        docReply.Id = doc.Id;
                    }
                }
            }


            return docReplyList;
        }

        public async Task<GRPCReply<GRPCPdfResponse>> HtmlToPdf(GRPCRequest<GRPCHtmlConvertRequest> convertRequest)
        {
            //convertRequest.HtmlContent = "<!DOCTYPE html>\r\n<html>\r\n\r\n<head>\r\n    <title>MITC</title>\r\n    <link rel=\"stylesheet\" href=\"https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0/css/bootstrap.min.css\">\r\n    <style>\r\n\r\n        .main-Panel {\r\n            padding: 80px;\r\n            line-height: 20px;\r\n        }\r\n\r\n        input {\r\n            border-top: none;\r\n            border-left: none;\r\n            border-right: none;\r\n            margin: 0px 5px;\r\n        }\r\n\r\n        h2 {\r\n            font-size: 20px;\r\n        }\r\n\r\n        h6 {\r\n            text-align: center !important;\r\n            font-weight: 700 !important;\r\n            padding: 30px;\r\n            font-size: 18px;\r\n        }\r\n\r\n        h3 {\r\n            font-size: 18px;\r\n        }\r\n\r\n        .agreement_section h3 {\r\n            text-align: left;\r\n            font-size: 20px;\r\n            display: block;\r\n            margin: 25px 0px;\r\n            font-weight: 700;\r\n        }\r\n\r\n        .agreement_section h2 {\r\n            text-align: center;\r\n            font-size: 25px;\r\n            display: block;\r\n            margin: 25px 0px;\r\n            font-weight: 700;\r\n        }\r\n\r\n        .agreement_section h1 {\r\n            font-size: 20px;\r\n            display: block;\r\n            margin: 25px 0px;\r\n            font-weight: 700;\r\n        }\r\n\r\n        p {\r\n            font-size: 16px;\r\n            line-height: 30px;\r\n        }\r\n\r\n        table tr > td {\r\n            border: 1px solid #ccc;\r\n            padding: 10px 20px !important;\r\n        }\r\n\r\n            table tr > td:first-child {\r\n                width: auto;\r\n                min-width: 250px;\r\n                max-width: 550px;\r\n            }\r\n        .table th, .table td {\r\n            padding: 0.75rem;\r\n            vertical-align: top;\r\n            border: 1px solid #dee2e6 !important;\r\n        }\r\n        .table-responsive {\r\n            display: block;\r\n            width: 100%;\r\n            overflow-x: auto;\r\n            -webkit-overflow-scrolling: touch;\r\n            border: solid 1px gray;\r\n        }\r\n\r\n        .gstSpan-box {\r\n            display: inline-block;\r\n        }\r\n\r\n            .gstSpan-box input {\r\n                border: 1px solid #ccc;\r\n                width: 35px;\r\n                margin: 0px;\r\n                padding: 2px;\r\n            }\r\n\r\n        .notedtedPoint {\r\n            border: 1px solid #ccc;\r\n            padding: 10px;\r\n            line-height: 35px;\r\n        }\r\n        .image {\r\n            margin-top: 45px;\r\n            margin-left: 150px;\r\n            height: 50px;\r\n            width: 120px;\r\n        }\r\n        img {\r\n            text-align: center !important;\r\n            justify-content: center !important;\r\n            margin-left: 40% !important;\r\n        }\r\n        .image_ratio {\r\n            height: 112px !important;\r\n            width: 200px !important;\r\n        }\r\n    </style>\r\n</head>\r\n\r\n<body>\r\n    <div class=\"container main-Panel\">\r\n        <form>\r\n            <header class=\"text-center\">\r\n                <h2>MOST IMPORTANT TERMS & CONDITIONS (MITC)</h2>\r\n                <h3>Please read carefully before confirming</h3>\r\n                <h2>ACKNOWLEDGEMENT FORM</h2><br>\r\n\r\n                <p>Date : {{ApplicationDate}}\r\n            </header><br>\r\n\r\n            <section class=\"topInputContent\">\r\n                <p>\r\n                    I/We refer to Application Reference No. {{LeadId}} for grant of Credit Facility for INR\r\n                    {{CreditLimit}}\r\n                    and the Agreement\r\n                    executed by me / us in favour of Shop Kirana E-Trading Pvt. Ltd. I/We have been provided the\r\n                    following\r\n                    information and\r\n                    agree with the same and have accordingly filled up the attached Document:\r\n                </p>\r\n            </section>\r\n            <section class=\"mt-5\">\r\n                <div class=\"table-responsive\">\r\n                    <table class=\"table\">\r\n                        <tr>\r\n                            <td>Name of the Customer</td>\r\n                            <td>:</td>\r\n                            <td>{{NameOfBorrower}}</td>\r\n                        </tr>\r\n                        <tr>\r\n                            <td>GST No.(if applicable)</td>\r\n                            <td>:</td>\r\n                            <td>\r\n                                {{GstNumber}}\r\n                            </td>\r\n                        </tr>\r\n                        <tr>\r\n                            <td>Credit Amount (Credit Limit)</td>\r\n                            <td>:</td>\r\n                            <td>INR {{CreditLimit}}/- (In words - {{CreditAmountWord}} )</td>\r\n                        </tr>\r\n                        <tr>\r\n                            <td>Validity of the Credit Limit (in Months)</td>\r\n                            <td>:</td>\r\n                            <td>12 Months</td>\r\n                        </tr>\r\n                        <tr>\r\n                            <td>Convenience Fees on the Transaction Amount</td>\r\n                            <td>:</td>\r\n                            <td>{{TransactionConvenienceFeeRate}} % per transaction amount + GST</td>\r\n                        </tr>\r\n                        <tr>\r\n                            <td>Processing Fee Inclusive of GST</td>\r\n                            <td>:</td>\r\n                            <td>{{ProcessingFees}} INR</td>\r\n                        </tr>\r\n                        <tr>\r\n                            <td>Discount on PF inclusive of GST</td>\r\n                            <td>:</td>\r\n                            <td>{{DiscountAmount}} INR</td>\r\n                        </tr>\r\n                        <tr>\r\n                            <td>Net PF inclusive of GST</td>\r\n                            <td>:</td>\r\n                            <td>{{NetPF}} INR</td>\r\n                        </tr>\r\n                        <tr>\r\n                            <td>Late Fees (applicable for payment default)</td>\r\n                            <td>:</td>\r\n                            <td>{{DelayPenaltyFeeRate}}% per month accrued on daily basis applicable on the unpaid amount.</td>\r\n                        </tr>\r\n                        <tr>\r\n                            <td>Bounce Charges</td>\r\n                            <td>:</td>\r\n                            <td>{{BounceCharge}} + GST</td>\r\n                        </tr>\r\n                        <tr>\r\n                            <td>Place of arbitration and jurisdiction</td>\r\n                            <td>:</td>\r\n                            <td>Indore, Madhya Pradesh</td>\r\n                        </tr>\r\n                        <tr>\r\n                            <td>Taxes & Levies</td>\r\n                            <td>:</td>\r\n                            <td>\r\n                                All the charges indicated above or elsewhere in the Agreement shall be exclusive of all\r\n                                taxes and statutory levies as\r\n                                may be applicable on same including without limitation tax on goods and services with\r\n                                cess.\r\n                            </td>\r\n                        </tr>\r\n                        <tr>\r\n                            <td>\r\n                                Additional Terms and\r\n                                conditions\r\n                            </td>\r\n                            <td>:</td>\r\n                            <td>\r\n                                Covenants mentioned in Agreement For Supply Of Goods On Credit shall be applicable in\r\n                                addition to the terms mentioned in\r\n                                this MITC applicable to the Customer for availing Credit Facility. Covenants mentioned\r\n                                in Agreement For Supply Of Goods\r\n                                On Credit shall prevail in case of inconsistency.\r\n                            </td>\r\n                        </tr>\r\n\r\n                    </table>\r\n                </div>\r\n            </section>\r\n\r\n            <section class=\"notedtedPoint\">\r\n                <ul>\r\n                    <li>Please do not sign the above document, if blank.</li>\r\n                    <li>\r\n                        Please do not pay cash to anyone for the processing of your application or payments towards the\r\n                        outstanding. In case\r\n                        you are asked, please refuse and contact the head office of the company.\r\n                    </li>\r\n                    <li>\r\n                        For all payments made to any representative of the company towards any fee / charges, please ask\r\n                        for valid receipt.\r\n                    </li>\r\n                    <li>\r\n                        If any verbal communication made to you is in contradiction to the above terms & conditions,\r\n                        please contact us at\r\n                        7828112112 (mon-sat 11:00 a.m. To 7:00 p.m. (except public holidays) or write to us at\r\n                        Info@shopkirana.com\r\n                    </li>\r\n                </ul>\r\n            </section>\r\n            <img src=\"{{img}}\" class=\"image\" />\r\n            <br />\r\n            <br />\r\n            <br /><br />\r\n            <section>\r\n                <h6>CHARGES SHEET</h6>\r\n                <div class=\"table-responsive\">\r\n                    <table class=\"table\">\r\n                        <tr>\r\n                            <td>Processing Fee</td>\r\n                            <td>{{ProcessingFees}} % + GST (Non Refundable)</td>\r\n                        </tr>\r\n                        <tr>\r\n                            <td>Convenience  Fees</td>\r\n                            <td>{{TransactionConvenienceFeeRate}} %+ GST</td>\r\n                        </tr>\r\n                        <tr>\r\n                            <td>Late Payment Charges</td>\r\n                            <td>{{DelayPenaltyFeeRate}} % Per Month</td>\r\n                        </tr>\r\n                        <tr>\r\n                            <td>Bounce Charges</td>\r\n                            <td>{{BounceCharge}} + GST</td>\r\n                        </tr>\r\n                    </table>\r\n                </div>\r\n\r\n                <p>\r\n                    I acknowledge and understand that all the terms and conditions mentioned in MITC and Agreement for\r\n                    Supply of Goods on\r\n                    Credit are applicable on me/us and I/we have read and understood the same before marking my/our\r\n                    signatures.\r\n                </p>\r\n\r\n                <div class=\"row\">\r\n                    <div class=\"col-md-6 col-sm-6 col-12\">\r\n                        <label>Customer's Name:</label>\r\n                        {{NameOfBorrower}}\r\n                    </div>\r\n\r\n                    <!--<div class=\"col-md-6 col-sm-6 col-12 text-right\">\r\n                Signature {{Signature}}\r\n            </div>-->\r\n                </div><br>\r\n                <p>(Name of Authorised Signatory for Proprietorship, Partnership & Companies)</p>\r\n\r\n            </section>\r\n\r\n            <section class=\"agreement_section\">\r\n                <h2>AGREEMENT FOR SUPPLY OF GOODS ON CREDIT</h2>\r\n                <p>\r\n                    THIS AGREEMENT FOR SUPPLY OF GOODS ON CREDIT (THE \"AGREEMENT\") MADE ON  Date : {{ApplicationDate}}\r\n                    <!--THIS <b>Insert Date</b> Day of\r\n            <b>\r\n                Month\r\n                & Year\r\n            </b>-->\r\n                    between Shop\r\n                    Kirana E-Trading Pvt. Ltd., a Company incorporated under the Companies Act, 1956, having its\r\n                    Registered Office at Office\r\n                    1501,15th Floor, SKYE Corporate Park,Plot No 25, Scheme No 78, Part II,Sector B Indore 452010,\r\n                    Madhya Pradesh.\r\n                </p>\r\n\r\n\r\n                <p>\r\n                    (hereinafter referred to as THE COMPANY and/or SELLER and/or PARTY TO THE FIRST PART, which\r\n                    expression shall, unless\r\n                    repugnant to the context or meaning thereof, be deemed to include its successors and permitted\r\n                    assigns) of the ONE PART <br>\r\n\r\n                    AND\r\n                </p>\r\n\r\n                <p>\r\n                    Shri/SMT/MS/MRS {{NameOfBorrower}} son/ daughter of {{FatherName}}, an individual/sole\r\n                    proprietor/, of Indian Inhabitant\r\n                    currently residing at {{address1}}\r\n                </p>\r\n\r\n                <p>\r\n                    (Hereinafter called as CUSTOMER and/or PARTY TO THE SECOND PART, which expression shall include his\r\n                    / her heirs,\r\n                    executors, administrator, its successors and permitted assigns in law) of the OTHER PART:\r\n                </p>\r\n                <p>\r\n                    The Company and the Customer hereinafter referred to individually as \"Party\" and collectively as\r\n                    \"Parties\" <br>\r\n                    Whereas, the Company is engaged, inter-alia, in the business of manufacturing, sale, distribution,\r\n                    marketing and supply\r\n                    of grocery items under the brand name \"ShopKirana\". The Company is also engaged, inter-alia, in the\r\n                    business of sale,\r\n                    distribution, marketing and supply of grocery items of various other companies who has exclusive\r\n                    right to prepare,\r\n                    package and sell the goods and exclusive right to manufacture and sell grocery and other products\r\n                    under the Trademarks\r\n                    owned by them. Hereinafter, referred to as the GOODS.\r\n                </p>\r\n                <p>\r\n                    Whereas the Party to the Second Part is a customer of the Company and has applied to the Company for\r\n                    the provision of\r\n                    credit in respect of the payment of Goods supplied to the Customer by the Company.<br>\r\n                    Whereas, The Customer agrees to be bound by the attached Terms and Conditions of the Agreement and on such\r\n                    promise the Company\r\n                    is ready and willing to provide the customer the Goods on credit.<br>\r\n                    Whereas, the Customer is aware that the Company may alter the Terms and Conditions of the Agreement in\r\n                    accordance with the Terms\r\n                    and Conditions of Trade in which event the Customer agrees that the varied terms shall apply to\r\n                    Goods supplied after notification. The Customer further\r\n                    acknowledges that he/she has read this Agreement and the Terms and Conditions of Trade and has\r\n                    received copy of this document.\r\n                    <br />\r\n                    <!--<img src=\"{{img}}\" style=\"margin-top:45px;margin-left:150px;height:50px;width:120px;\" />-->\r\n                    <img src=\"{{img}}\" class=\"image\"/>\r\n                    <br /><br /><br />\r\n                    Now Therefore, in consideration of the mutual covenants and promises contained herein and for good\r\n                    and valuable\r\n                    consideration, the parties hereto having the intention to be legally bound, agrees as follows:\r\n                </p>\r\n                <h1>1. BASIS OF CONTRACT</h1>\r\n                <p>\r\n                    1.1. These Conditions apply to the Contract to the exclusion of any other terms that the Customer\r\n                    seeks to impose or\r\n                    incorporate, or which are implied by trade, custom, practice or course of dealing. For the avoidance\r\n                    of doubt, these\r\n                    conditions supersede all previous terms and conditions and shall replace any terms and conditions\r\n                    previously notified to\r\n                    the Customer.<br>\r\n                    1.2. The order constitutes an offer by the Customer to purchase the Goods and/or Services in\r\n                    accordance with these\r\n                    Conditions. The Customer is responsible for ensuring that the terms of the Order and any applicable\r\n                    Specification are\r\n                    complete and accurate.<br>\r\n                    1.3. Orders placed by Customer leading to a contract which are not expressed to be subject to these\r\n                    conditions shall\r\n                    still be subject to them.<br>\r\n                    1.4. The order shall only be deemed to be accepted when the Company issues a written acceptance of\r\n                    the Order, at which\r\n                    point the Contract shall come into existence.\r\n                    <br />\r\n                    1.5. The Agreement constitutes the entire agreement between the Parties. The Customer acknowledges\r\n                    that it has not relied\r\n                    on any statement, promise or representation made or given by or on behalf of the Company which is\r\n                    not set out in the\r\n                    Agreement.<br>\r\n                    1.6. Any samples, drawings, descriptive matter, or advertising produced by the Company and any\r\n                    descriptions or\r\n                    illustrations contained in the Company's catalogues or brochures are produced for the sole purpose\r\n                    of giving an\r\n                    approximate idea of the Goods described in them. They shall not form part of the Agreement or have\r\n                    any contractual force.\r\n                    <br />\r\n                    1.7. A quotation for the Goods given by the Company is not binding or capable of acceptance and\r\n                    shall not constitute an\r\n                    offer.<br>\r\n                    1.8. Any application for a Credit by the Customer, Agreement for the Supply of Goods on Credit\r\n                    entered into by the\r\n                    Customer or any other written or verbal instructions received by the Company from the Customer for\r\n                    the purchase of Goods\r\n                    and/or the Customer's acceptance of Goods supplied by the Company shall constitute acceptance of the\r\n                    Terms and\r\n                    Conditions of Agreement contained herein as they may be updated from time to time.\r\n\r\n                </p>\r\n                <h3>2. PLACEMENT AND ACCEPTANCE OF ORDERS AND ENTITLEMENT TO CREDIT</h3>\r\n                <p>\r\n                    2.1. Orders will be on such forms as the Company may require from time to time.<br>\r\n                    2.2. The Company may terminate Credit supply pursuant to these Terms and Conditions of Trade at any\r\n                    time by written\r\n                    notice. Such termination shall not release the Customer from any obligation already incurred at the\r\n                    time of termination\r\n                    or any further liability in respect of that obligation.<br>\r\n                    2.3. The Company may at any time refuse to accept any Order by the Customer or decline any request\r\n                    for a Credit supply\r\n                    to the Customer (whether by way of termination of Credit supply or not) and shall not be required to\r\n                    specify any reason.\r\n\r\n                </p>\r\n                <h3>3. CREDIT LIMIT AND DURATION</h3>\r\n                <p>\r\n                    3.1. The Company may set a credit limit for the Customer. Changes in the Customer's credit limit may\r\n                    be made from time\r\n                    to time and will be notified to the Customer<br>\r\n                    3.2. The Customer understands and agrees that the credit is provided for the period of not more than\r\n                    14day(s), unless\r\n                    otherwise provided by the Company. The Customer is liable to pay for the goods supplied on or before\r\n                    the end of credit\r\n                    period.<br>\r\n                    3.3. The Company may refuse to accept orders for Goods or may suspend or withhold delivery of Goods\r\n                    if the Customer has exceeded its credit limit or if the completion of a Contract for such Goods would result in the\r\n                    Customer exceeding its credit limit <br />\r\n                    <!--<img src=\"{{img}}\" style=\"margin-top:45px;margin-left:150px;height:50px;width:120px;\" />-->\r\n                    <img src=\"{{img}}\" class=\"image\" />\r\n                </p>\r\n\r\n                <br /><br />\r\n                <h3>4. SECURITY</h3>\r\n                <p>\r\n                    Without prejudice to such other rights as the Company may have pursuant to these Terms and\r\n                    Conditions of Trade, the\r\n                    Company reserves the right to request from the Customer such security as the Company may from time\r\n                    to time think\r\n                    desirable to secure to the Company all sums due to the Company and may refuse to supply further to\r\n                    the Customer until\r\n                    such security is given.\r\n                </p>\r\n                <h3>5. TERMS OF PAYMENT</h3>\r\n                <p>\r\n                    5.1. Unless otherwise specified in writing by the Company payment for all Goods shall be made no\r\n                    later than 14 days from\r\n                    the date of transaction.<br>\r\n                    5.2. The Company may, at its sole discretion, require payment of a deposit by the Customer prior to\r\n                    processing of any\r\n                    Order or Delivery of any Goods.<br>\r\n                    5.3. In addition to the cost of the Goods the Customer agrees to pay to the Company convenience Fees\r\n                    to avail the credit\r\n                    facility. The convenience Fees shall be 0.75% of the transaction amount excluding applicable GST as\r\n                    agreed between the\r\n                    Parties. The Customer acknowledges that this fee is not the interest amount charged for availing the\r\n                    credit facility of\r\n                    the Company.<br>\r\n                    5.4. Late fees shall be charged on any monies not paid by the due date at the rate of 2.5% per month\r\n                    excluding applicable GST as agreed between the Parties which will be\r\n                    calculated and charged on a daily basis until payment is made in full.\r\n                    <br>5.5. All costs incurred by the Company, as a result of a default by the Customer including but\r\n                    not\r\n                    limited to\r\n                    administration charges, debt collection costs and legal costs as between solicitor and client, shall\r\n                    be payable by the\r\n                    Customer in addition to any other charges pursuant to this clause 4.\r\n                    <br>5.6. Payment shall be made by ECS (Direct Debit from Bank Account) or by any other method as the\r\n                    Company may deem fit\r\n                    from time to time. The Customer understands that all ECS (Direct Debit from Bank Account)\r\n                    instructions are irrevocable\r\n                    and there is no facility for giving stop payment instruction in respect of such remittances.\r\n                    <br> 5.7. The Customer shall also be charged with NACH bounce charges as applicable in case of\r\n                    bounce\r\n                    payments.<br>\r\n                    5.8. The Company may notify the Customer at any time regarding the fact that it is going to stop\r\n                    supplying Goods to the\r\n                    Customer on credit. This shall be without prejudice to the Customer's obligation to pay amounts\r\n                    owing to the Company.\r\n                </p>\r\n                <h3>6. TAXES AND DUTIES</h3>\r\n                <p>\r\n                    6.1 Unless expressly included in any Quotation, GST and other taxes and duties assessed or levies in\r\n                    connection with the\r\n                    supply of the Goods;<br>\r\n                    i. are not included in the price of the Goods;<br>\r\n                    ii. shall be paid in addition to the price of the Goods; and shall be the responsibility of the\r\n                    Customer.<br>\r\n                    iii. Where the payment of any taxes, duties or levies is the responsibility of the Company at law,\r\n                    the price of Goods\r\n                    shall be increased by the amount of such taxes or duties.\r\n                </p>\r\n                <h3>7. DELIVERY OF GOODS</h3>\r\n                <p>\r\n                    7.1 Transportation\r\n                    <br />\r\n                    (a) Delivery of the Goods shall take place at the time of dispatch or collection from the Company's\r\n                    Premises unless the\r\n                    Company agrees in writing to deliver the Goods to the Customer's Premises in which case Delivery\r\n                    shall take place at the\r\n                    time of delivery to the Customer's Premises.<br>\r\n                    <!--<img src=\"{{img}}\" style=\"margin-top:45px;margin-left:150px;height:50px;width:120px;\" />-->\r\n                    <img src=\"{{img}}\" class=\"image\" />\r\n                    <br /><br /><br />\r\n                    (b) If the Company agrees to deliver the Goods to the Customer's Premises then the costs of delivery\r\n                    shall be in\r\n                    addition to the consideration of the Agreement unless delivery is specified to be included in the Price. The Customer shall\r\n                    make all arrangements\r\n                    necessary to take possession of the Goods at the Customer's Premises and if the Customer is unable\r\n                    to take delivery of\r\n                    the Goods the Company shall be entitled to charge a reasonable fee for re-delivery.\r\n                    <br> (c) Delivery of the Goods to the Customer's Premises is deemed to be delivery to the Customer.\r\n                    <br> (d) If the Company delivers Goods to the Customer then unless delivery is included in the Price\r\n                    of\r\n                    the Goods the\r\n                    delivery fee shall be invoiced by the Company and paid for by the Customer in accordance with the\r\n                    provisions in these\r\n                    Terms and Conditions of the Agreement.\r\n                    <br> (e) The Company may at its discretion charge the Customer for delivery costs incurred by the\r\n                    Carrier\r\n                    and these charges\r\n                    may be subject to change.\r\n                    <br> (f) The Customer shall have no right of action against the Company in respect of any loss\r\n                    sustained\r\n                    by reason of any\r\n                    delivery occasioned by delays in transit or delays caused by accidents, strikes or any other event\r\n                    or occurrence outside\r\n                    of the Company's reasonable control.\r\n                    <br> (g) Time shall in no case be of the essence in respect of the delivery of Goods. The Company\r\n                    shall\r\n                    not be responsible\r\n                    for any delay in the delivery of Goods and the Customer shall not be entitled to cancel orders\r\n                    because of any such\r\n                    delay. Dates for delivery of Goods are given in good faith and are not to be treated as a condition\r\n                    of sale or purchase.\r\n                    Delivery of Goods by the Company to a Carrier is deemed to be delivery to the Customer.\r\n                    <br />\r\n                    7.2. Inspection\r\n                    <br />\r\n                    (a) The Customer must inspect the Goods upon arrival and reports the errors in shipment promptly.\r\n                    <br> (b) If the Customer fails to notify the Company of any such defects, the shipment shall be\r\n                    deemed\r\n                    accepted.\r\n\r\n                </p>\r\n                <h3>8. PASSING OF RISK</h3>\r\n                <p>\r\n                    Risk in the Goods passes to the Customer upon the earlier of:<br>\r\n                    (a) delivery of the Goods to the Customer; or<br>\r\n                    (b) collection of the Goods from the Company or agent of the Company by the Customer's agent,\r\n                    carrier or courier.\r\n                </p>\r\n                <h3>9. RETENTION OF TITLE</h3>\r\n                <p>\r\n                    9.1. The Goods delivered by the Company to the Customer remains the sole and absolute property of\r\n                    the Company as legal\r\n                    and equitable owner until all money due to the Company under this Agreement has been paid by the\r\n                    Customer, but the Goods\r\n                    will be at the Customer's risk from the time of delivery to it;\r\n                    <br> 9.2. The Customer acknowledges that it is in possession of all the Goods as bailee for the\r\n                    Company\r\n                    under the terms of\r\n                    this Agreement until such time as title in the Goods passes to the Customer and that this bailment\r\n                    continues in relation\r\n                    to each item of the Goods until the Price has been paid in full;\r\n                    <br> 9.3. The Customer's right to possession of the Goods will cease if it does anything or fails to\r\n                    do\r\n                    anything which would\r\n                    result in an Insolvency event in relation to the Customer including any act or omission which would\r\n                    entitle a receiver\r\n                    or any other person to take possession of any assets or which would entitle any person to present a\r\n                    petition for the\r\n                    winding up of the Customer.\r\n                    <br> 9.4. The Company may for the purpose of examination or recovery of the Goods enter upon any\r\n                    premises where the Goods are\r\n                    located or where it is reasonably thought to be located and the Customer agrees to indemnify the\r\n                    Company for any, cost,\r\n                    loss or damage incurred by the Company as a result of entry onto such premises;\r\n                    <br> 9.5. The Customer warrants that at the time of entering into this Agreement no Criminal\r\n                    proceedings\r\n                    are pending and no\r\n                    Insolvency Event has occurred in relation to the Customer and it knows of no circumstances which\r\n                    would entitle any\r\n                    creditor to appoint a receiver or to petition for winding up or to exercise any other rights over or\r\n                    against its assets.<br />\r\n                    <!--<img src=\"{{img}}\" style=\"margin-top:45px;margin-left:150px;height:50px;width:120px;\" />-->\r\n                    <img src=\"{{img}}\" class=\"image\" />\r\n                </p>\r\n\r\n                <br />\r\n                <h3>10. SALE OF GOODS BY THE CUSTOMER</h3>\r\n                <p>\r\n                    10.1 Where Goods in respect of which title has not passed to the Customer are sold by the Customer\r\n                    in the ordinary\r\n                    course of business, the book debt created on the sale and the proceeds of sale when received shall\r\n                    be held by the\r\n                    Customer for the Company.\r\n                    <br> 10.2 Where any proceeds of sale are placed in the Customer's bank account, the funds in the\r\n                    Customer's bank account\r\n                    shall be deemed to be held on trust for the Company to the extent of proceeds of sale.\r\n                    <br> 10.3 Where any payments are made from the Customer's bank account otherwise than to the Company\r\n                    payment shall be deemed\r\n                    to have been made from all other funds in the Customer's bank account and not from funds held on\r\n                    trust for the Company.\r\n                    <br> 10.4 The trust obligation imposed by this clause and the Company's entitlements under this\r\n                    clause\r\n                    10 shall continue for\r\n                    so long as the Company is unpaid for all Goods supplied to the Customer.\r\n                </p>\r\n                <h3>11. PAYMENT ALLOCATION</h3>\r\n                <p>\r\n                    The Company may, in its discretion, allocate any payment received from the Customer towards any\r\n                    invoice that the Company\r\n                    determines and may do so at the time of receipt or at any time afterwards. On any default by the\r\n                    Customer the Company\r\n                    may re-allocate any payments previously received and allocated. In the absence of any payment\r\n                    allocation by the Company,\r\n                    payment shall be deemed to be allocated in such manner as preserves the maximum value of the\r\n                    Company's purchase money\r\n                    security interest in the Goods.\r\n                </p>\r\n                <h3>12. CUSTOMER'S WARRANTY, UNDERTAKING & DECLARATION</h3>\r\n                <p>\r\n                    (a) The Customer hereby warrants the correctness of each and every statement and particulars herein\r\n                    contained and\r\n                    undertake to carry out the proposals herein set forth.\r\n                    <br> (b) The Customer hereby warrants that his/her CIBIL score is satisfactory and grants an\r\n                    exclusive\r\n                    right to the Company to\r\n                    check his/her past credit history for the purpose of this agreement.\r\n                    <br> (c) The Customer agrees and undertakes to notify the Company in writing of any circumstances\r\n                    affecting the correctness of\r\n                    any particulars set forth in the agreement immediately after the occurrence of any such\r\n                    circumstances.\r\n                    <br> (d) The Customer shall upon request of the Company, allow the Company and any nominee, servant\r\n                    or\r\n                    agent of the Company to\r\n                    inspect the Customer's premises and vehicles and other assets and the Customer's books of accounts\r\n                    for ensuring that the\r\n                    Customerhas duly complied with the terms of the agreement.\r\n                    <br> (e) The Customer will furnish the Company with all such information as the Company may\r\n                    reasonably\r\n                    require for the\r\n                    Company's satisfaction as due compliance with the terms of this agreement and all such periodical\r\n                    reports and\r\n                    information at such times, in such form and containing such particulars as the Company may call for.\r\n                    <br />\r\n                    (f) During the currency of this agreement, the Customer shall not, without the prior permission\r\n                    in\r\n                    writing of the\r\n                    Company-\r\n                    <br />\r\n                    i. Effect any change in the Customer's capital structure;<br>\r\n                    ii. Formulate any Scheme of Amalgamation or Reconstruction;<br>\r\n                    iii. Implement any Scheme of Expansion/Diversification/Modernisation other than incurring routine\r\n                    capital expenditure;<br>\r\n                    iv. Make any corporate investments or investment by way of share capital or debentures or lend or\r\n                    advance funds to or\r\n                    place deposits with, any other concern except give normal trade credits or place on security\r\n                    deposits in the normal\r\n                    course of business or make advances to employees, provided that the Customer may make such\r\n                    investments by way of\r\n                    deposits or advances that are required statutorily to be made as per the existing laws of the\r\n                    country or the rules or\r\n                    regulations or guidelines issued from time to time by the Authorities concerned.\r\n                    <br> v. Undertake guarantee obligation on behalf of any third party or any other company/firm etc.\r\n                    <!-- <img src=\"{{img}}\" style=\"margin-top:45px;margin-left:150px;height:50px;width:120px;\" />-->\r\n                    <img src=\"{{img}}\" class=\"image\" />\r\n                    <br /><br />\r\n                    (g) The Customer agrees that it will maintain adequate Books of Accounts which would correctly\r\n                    reflect its financial\r\n                    position and scale of operations and would not radically change the Accounting System without prior\r\n                    consent of the\r\n                    Company.<br>\r\n                    (h) Save and except to the extent already disclosed in writing by the Customer to the Company, the\r\n                    Customer hereby\r\n                    warrants and undertakes as follows:\r\n                    i. The Customerhas obtained all necessary statutory permission/sanction to avail the credit facility\r\n                    from the Company.<br>\r\n                    ii. There are no mortgages, charges, lispendens or liens or other encumbrances on Customer.\r\n                    <br> iii. The Customer is not a party to litigation of a material character and that the Customer is\r\n                    not\r\n                    aware of any facts\r\n                    likely to give rise to such litigation or to material claims against the Customer.\r\n                    <br> iv. The Customer shall carry on the business efficiently, properly and profitably and such\r\n                    business\r\n                    shall be continued\r\n                    to such activity as have been notified to the Company and shall keep all the licenses, leases,\r\n                    contracts, engagements\r\n                    essential for carrying on the activity, renewed from time to time and shall not allow any\r\n                    interruptions or disturbances\r\n                    to happen to the business.\r\n                    <br> v. The Customer shall maintain proper books of accounts and such other registers, books,\r\n                    documents,\r\n                    relating to the\r\n                    business as may be statutorily required or as may be required by the Company or as may be necessary\r\n                    and/or generally\r\n                    kept in the business of the kind carried on by the Customer and shall get the accounts, books duly\r\n                    audited. The Customer\r\n                    shall, if so required by the Company, allow the Company to inspect such books duly audited.\r\n\r\n                </p>\r\n                <h3>13. EVENTS AND CONSEQUENCES OF DEFAULT</h3>\r\n                <p>\r\n                    13.1 All amounts payable in respect of any credit supply (s) shall become immediately due to the\r\n                    Company and the Company\r\n                    may at its option cancel any Order, suspend or terminate these Terms and Conditions of Trade and/or\r\n                    exercise any of the\r\n                    remedies available to it under these Terms and Conditions of Trade in the event that:\r\n                    <br> i. the Customer fails to pay any monies to the Company on due date or breaches any other\r\n                    obligation\r\n                    herein; or\r\n                    <br> ii. a receiver is appointed over any of the assets or undertaking of the Customer\r\n                    <br> iii. an application for the appointment of a liquidator is filed against the Customer or any of\r\n                    the\r\n                    conditions necessary\r\n                    to render the Customer liable to have a liquidator exist, or a liquidator is appointed;\r\n                    <br> iv. the Customer suspends payments to its creditors or makes or attempts to make an arrangement\r\n                    or\r\n                    composition or scheme\r\n                    with its creditors; or\r\n                    <br> v. the Customer becomes insolvent or is presumed to be unable to pay its debts as they fall due\r\n                    or\r\n                    commits any act of\r\n                    bankruptcy; or\r\n                    <br> vi. the Customer defaults in any payment or commits any act of bankruptcy or any act which\r\n                    would\r\n                    render it liable to be\r\n                    wound up or if a resolution is passed or proceedings are filed for the winding up of the Customer.\r\n                    <br> vii. All costs incurred by the Company as a result of any breach by the Customer of any\r\n                    obligation\r\n                    of the Customer\r\n                    pursuant to these Terms and Conditions of Trade including, but not limited to, administration\r\n                    charges, debt collection\r\n                    costs and legal costs (on a solicitor/client basis) shall be payable by the Customer.\r\n\r\n                </p>\r\n                <h3>14. CONFIDENTIALITY</h3>\r\n                <p>\r\n                    14.1 If a separate Non-disclosure agreement is in effect between the Company and the Customer, the\r\n                    terms of such\r\n                    Non-disclosure agreement will govern in the event of any conflict between such agreement and this\r\n                    provision. During the\r\n                    term of this Agreement and at all times thereafter, (a) the Customer will hold in trust, keep\r\n                    confidential, and not\r\n                    disclose, directly or indirectly, to any third party or make any use of Confidential Information (as\r\n                    defined below) for\r\n                    any purpose, except as required by law or as may be necessary in the performance of the Customer's\r\n                    duties and\r\n                    obligations under this Agreement, and (b) all Confidential Information will remain the exclusive\r\n                    property of the\r\n                    Company, and will not be removed from the premises of Company or its customers, or be duplicated by\r\n                    any means, without\r\n                    the prior written consent of the Company.\r\n                    <br /><br />\r\n                    <img src=\"{{img}}\" class=\"image\" />\r\n                    <br /><br />\r\n                    For purposes of this Agreement, the term \"CONFIDENTIAL\r\n                    INFORMATION\" means any and all information that the Customer may\r\n                    receive from the Company: (i) relating to the technology\r\n                    embodied in the\r\n                    Goods, (ii) regarding the business, finances and/or operations of the Company, or (iii) any\r\n                    information marked or\r\n                    labelled \"Confidential\" or \"Proprietary\" by Company prior to its\r\n                    transmittal to, or receipt by, the\r\n                    Customer. All such\r\n                    information (as referenced above in this Section 14.1) to which the Customer had access prior to the\r\n                    execution of this\r\n                    Agreement is also included in Confidential Information. These obligations shall survive the\r\n                    expiration or termination of\r\n                    this Agreement.\r\n                    <br />\r\n                    14.2 Upon the termination of this Agreement (regardless of the reason for termination), the Customer\r\n                    will immediately:\r\n                    (i) return to the Company or destroy all Confidential Information (including copies), and any other\r\n                    material (including\r\n                    handwritten or computer-generated notes), made or derived from Confidential Information, which is in\r\n                    the Customer's\r\n                    possession or which the Customer has delivered to others, and (ii) destroy any Confidential\r\n                    Information stored on\r\n                    magnetic, optical or other medium after providing this Confidential Information to the Company.\r\n                </p>\r\n                <h3>15. NON-SOLICITATION; CONFLICTING ACTIVITIES</h3>\r\n                <p>\r\n                    15.1 Non-solicitation. As a material inducement for the Company to enter into this Agreement, the\r\n                    Customer covenants\r\n                    that during the Term and for a period of eighteen (18) months immediately following the termination\r\n                    of this agreement\r\n                    for any reason, whether with or without cause, the Customer shall not either directly or indirectly\r\n                    solicit, induce,\r\n                    recruit or encourage any of Company's employees or independent contractors to leave their employment\r\n                    or engagements with\r\n                    the Company, or take away such employees or independent contractors, or attempt to solicit, induce,\r\n                    recruit, encourage\r\n                    or take away Company's employees or independent contractors, either for the benefit of the Customer\r\n                    or for any other\r\n                    person or entity.<br>\r\n                    15.2 Conflicting Activities. In addition, as a material inducement for the Company to enter into\r\n                    this Agreement, the\r\n                    Customer agrees that during the Term of this agreement, it will not directly or indirectly,\r\n                    individually, in partnership\r\n                    or in conjunction with any person, association or company, in any capacity whatsoever directly or\r\n                    indirectly, (i)\r\n                    promote, deliver, sell or solicit orders for any products which, in the opinion of the Company, are\r\n                    in competition with\r\n                    the Goods, or (ii) otherwise engage in business activities that are in competition with the business\r\n                    of the Company\r\n                    (\"CONFLICTING ACTIVITIES\"). The Customer also agrees to immediately disclose to the Company in\r\n                    writing all details\r\n                    regarding the occurrence and nature of any Conflicting Activities. The provisions of this Section\r\n                    will survive the\r\n                    termination of this agreement and will bind the Customer, its successors, and it's permitted\r\n                    assigns.\r\n                </p>\r\n                <h3>16. INDEMNITY</h3>\r\n                <p>\r\n                    16.1 The Customer hereby agrees to indemnify, defend and hold harmless the Company, its affiliates\r\n                    and all officers,\r\n                    directors, employees and agents thereof (hereinafter referred to as \"Indemnities\") from all\r\n                    liabilities, claims,\r\n                    damages, losses, costs, expenses, demands, suits and actions (including without limitation\r\n                    attorneys' fees, expenses and\r\n                    settlement costs) (collectively, \"Damages\") arising out of or related to the conduct of the\r\n                    Customer's operations,\r\n                    including without limitation damages arising out of or related to damage or injury to property or\r\n                    persons, or to any\r\n                    representations of the Customer not authorized hereunder.<br>\r\n                    16.2 The Customer shall indemnify (and keep indemnified) the Company, its board members, employees,\r\n                    contractors and\r\n                    agents in respect of all claims, demands, actions, suits and damages for loss, damage or injury\r\n                    (including indirect or\r\n                    consequential loss) resulting from:\r\n                    <br> (a) any acts of the Customer, including any unlawful or wilfully wrong act or omission;\r\n                    <br>(b) any breach of this agreement and any tort or negligence by the Customer in connection with\r\n                    this\r\n                    agreement;\r\n                    <br> (c) all breaches of Intellectual Property Rights by the Customer or any third party to whom the\r\n                    Customer provided access\r\n                    either deliberately or inadvertently; and\r\n                    <br> (d) any legal costs, charges and expenses arising in respect of paragraphs (a) - (d) above,\r\n                    <!--<img src=\"{{img}}\" style=\"margin-top:45px;margin-left:150px;height:50px;width:120px;\" />-->\r\n                    <img src=\"{{img}}\" class=\"image\" />\r\n                    <br /><br />\r\n                    16.3 The Customer shall indemnify and keep the Company indemnified against any breach of the\r\n                    conditions contained herein\r\n                    and against all loss, harm, damage, injury and all costs, charges and expenses that the Company may\r\n                    bear, suffer or\r\n                    incur on account of any breach or non-observation or non-performance of any of the terms and\r\n                    conditions contained in\r\n                    this agreement.<br>\r\n                    16.4 The Customer shall at all times indemnify and keep indemnified the Company against all actions\r\n                    proceedings claims\r\n                    and demands made against it by the central and/or state government and/or municipal/ local and/or\r\n                    other authorities\r\n                    and/or by any customer of the Good and/or any other third party as a result of or in consequence of\r\n                    any act or omission\r\n                    of whatsoever nature of the Customer, his servants or agents, including, without prejudice to the\r\n                    generality of the\r\n                    forgoing, any accident or loss or damage arising out of the storage, handling and/or delivery of the\r\n                    Good whether or not\r\n                    such act or omission or accident or loss or damage was due to any negligence, want of care or skill\r\n                    or any misconduct of\r\n                    the Customer, his servants or his agents.<br>\r\n                    16.5 This clause will survive termination of this Agreement.\r\n                </p>\r\n                <h3>17. GOVERNING LAW AND DISPUTE RESOLUTION</h3>\r\n                <p>\r\n                    17.1 Any dispute, controversy or claim arising out of or in relation to this agreement, or the\r\n                    breach, termination or\r\n                    invalidity thereof, shall be settled amicably by negotiation between the Parties.\r\n                    <br> 17.2 Any and all controversies, claims, or disputes arising out of, relating to, or resulting\r\n                    from\r\n                    this Agreement shall\r\n                    be subject to binding arbitration according to Indian Law. Any arbitration will be administered by\r\n                    such dispute shall be\r\n                    resolved pursuant to the Indian Arbitration and Conciliation Act, 1996 and any amendments of it\r\n                    thereof.\r\n                    <br>17.3 The terms of this Agreement have been executed and delivered and shall be interpreted,\r\n                    construed and enforced in\r\n                    accordance with the laws of India. The venue for arbitration shall be Indore, Madhya Pradesh.\r\n                    <br> 17.4 Any party to the dispute shall be entitled to serve a notice invoking this clause and\r\n                    making a\r\n                    reference to a sole\r\n                    arbitrator, which shall be appointed by the Company. Each party shall bear its own costs and\r\n                    expenses, including\r\n                    attorney's fees, incurred in connection with any Arbitration. The decision of the arbitrator shall\r\n                    be binding and in\r\n                    writing.\r\n                </p>\r\n\r\n                <h3>18. NOTICE</h3>\r\n                <p>\r\n                    18.1 Notice or Communication. Any notice or communication required or permitted hereunder (other than\r\n                    Administrative\r\n                    Notice) shall be in writing and shall be sent by registered mail, return receipt requested, postage\r\n                    prepaid and\r\n                    addressed to the addresses set forth below or to such changed address as any party entitled to\r\n                    notice shall have\r\n                    communicated in writing to the other party. Notices and communications to Company shall be sent to:\r\n                </p>\r\n                <strong>\r\n                    ShopKirana E Trading Pvt Ltd, 1501,15th Floor, SKYE Corporate Park, Plot No 25, Scheme No 78, Part II,\r\n                    Sector B Indore 452010, Madhya Pradesh.\r\n                </strong>\r\n                <p>\r\n                    Notices and communications to the Customer shall be sent to address shown on first page of this\r\n                    Agreement. Any notices or\r\n                    communications to either party hereunder shall be deemed to have been given when deposited in the\r\n                    mail, addressed to the\r\n                    then current address of such party.<br>\r\n                    18.2 Date of Effectiveness. Any such notice or communication so mailed shall be deemed delivered and\r\n                    effective\r\n                    seventy-two (72) hours after mailing.\r\n                </p>\r\n                <h3>19. WAIVER</h3>\r\n                <p>\r\n                    Failure by either Party to insist in any one or more instances on a strict performance of any of the\r\n                    provisions of this\r\n                    Agreement shall not constitute a waiver or relinquishment of the right to enforce the provisions of\r\n                    this Agreement in\r\n                    future instances, but this right shall continue and remain in full force and effect.\r\n                </p>\r\n                <!-- <img src=\"{{img}}\" style=\"margin-top:45px;margin-left:150px;height:50px;width:120px;\" />-->\r\n                <img src=\"{{img}}\" class=\"image\" />\r\n                <br /><br />\r\n                <h3>20.\tSEVERABILITY</h3>\r\n                <p>\r\n                    If any part of this agreement is found to be invalid or unenforceable, that part will be severed from\r\n                    this agreement and the remainder of the agreement shall remain in full force\r\n                </p>\r\n                <h3>21. ENTIRETY</h3>\r\n                <p>\r\n                    This Agreement and any Annexes embody the entire agreement between the Parties and supersede all\r\n                    prior agreements and\r\n                    understandings, if any, relating to the subject matter of this Agreement.\r\n                </p>\r\n\r\n                <h3>22. FINAL CLAUSES</h3>\r\n                <p>\r\n                    22.1 This agreement will enter into force upon signature by both Parties and shall remain in force\r\n                    until completion of\r\n                    all obligations of the Parties under this Agreement.<br>\r\n                    22.2 Amendments to this agreement may be made by mutual agreement in writing between the Parties.\r\n                </p>\r\n\r\n                <h3>23. COUNTERPARTS</h3>\r\n                <p>\r\n                    This Agreement may be executed in one or more counterparts, each of which shall be deemed an\r\n                    original, but all of which\r\n                    together shall constitute one and the same instrument.\r\n                </p>\r\n                <p>\r\n                    <b>IN WITNESS WHEREOF</b> the parties hereto have hereunto set and subscribed their respective hands\r\n                    the day and year first hereinabove written.\r\n                </p>\r\n                <p>\r\n                    <b>SIGNED AND DELIVERED</b> by within named\r\n                    THE COMPANY,\r\n                    Party to the Second Part,\r\n                    <b>SHOPKIRANA E TRADING COMPANY PVT.LTD.</b>\r\n                    through, Its Legal Officer\r\n                    <b>MR. ANAND MEENA</b>\r\n                </p>\r\n                <p>AND</p>\r\n                <p>\r\n                    <b>SIGNED AND DELIVERED</b> by within named\r\n                    THE CUSTOMER,\r\n                    Party to the Second Part,\r\n                    Mr./Ms./Mrs. {{NameOfBorrower}}\r\n\r\n                </p>\r\n            </section>\r\n            <br /><br />\r\n            <!--<br /><br /><br /><br /><br /><br /><br /><br /><br /><br /><br /><br /><br /><br /><br /><br /><br /><br /><br /><br /><br /><br /><br /><br />-->\r\n            <!--<img src=\"{{img}}\" style=\"margin-top:45px;margin-left:150px;height:50px;width:120px;\" />-->\r\n            <img src=\"{{img}}\" class=\"image\" />\r\n            <!--<div style=\"position:relative;\">\r\n       <img src=\"D:\\Project\\SKCreditLending\\CL_service\\trunk\\SkCreditLending\\Templates\\DirectLogo.gif\" style=\"margin-top:45px;margin-left:150px;height:50px;width:120px;position:absolute;right:50px;bottom:10px;\" />\r\n    </div>-->\r\n        </form>\r\n    </div>\r\n</body>\r\n\r\n</html>";
            GRPCReply<GRPCPdfResponse> pdfResponse = new GRPCReply<GRPCPdfResponse>();
            string pdfPath = "";
            string pdfFileName = $"{Guid.NewGuid().ToString()}.pdf";

            //GRPCPdfResponse res = new GRPCPdfResponse();
            pdfResponse.Response = new GRPCPdfResponse();
            pdfResponse.Response.PdfUrl = $"{EnvironmentConstants.MediaServiceLocalBaseURL}Uploads/ConvertedPDF/{pdfFileName}";

           
            string uploadFilePath = Path.Combine(_hostingEnvironment.ContentRootPath, "wwwroot", "Uploads", "ConvertedPDF");
            string rotavitaPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Rotativa");

            //string libraryPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Path.Combine(rotavitaPath, "Windows", "wkhtmltopdf.exe") : ((!RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) ? Path.Combine(rotavitaPath, "Linux", "wkhtmltopdf") : Path.Combine(rotavitaPath, "Mac", "wkhtmltopdf"));

            if (!Directory.Exists(uploadFilePath))
            {
                Directory.CreateDirectory(uploadFilePath);
            }

            pdfPath = Path.Combine(uploadFilePath, pdfFileName);

            // WkhtmlDriver.Convert(); //

            GRPCHtmlConvertRequest req = new GRPCHtmlConvertRequest();
            var byteArray = WkhtmlDriver.Convert(rotavitaPath, "", convertRequest.Request.HtmlContent); //_generatePdf.GetPDF(convertRequest.HtmlContent);

            System.IO.File.WriteAllBytes(pdfPath, byteArray);

            return pdfResponse;
        }
    }


    public abstract class WkhtmlDriverNew
    {
        public static byte[] Convert(string wkhtmlPath, string switches, string html)
        {
            string text = (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Path.Combine(wkhtmlPath, "Windows", "wkhtmltopdf.exe") : ((!RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) ? Path.Combine(wkhtmlPath, "Linux", "wkhtmltopdf") : Path.Combine(wkhtmlPath, "Mac", "wkhtmltopdf")));
            if (!File.Exists(text))
            {
                throw new Exception("wkhtmltopdf not found, searched for " + text);
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                switches = "-q " + switches + " -";
                if (!string.IsNullOrEmpty(html))
                {
                    switches += " -";
                    html = SpecialCharsEncode(html);
                }

                using Process process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = text,
                    Arguments = switches,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true
                };
                process.Start();
                if (!string.IsNullOrEmpty(html))
                {
                    using StreamWriter streamWriter = process.StandardInput;
                    streamWriter.WriteLine(html);
                }

                using MemoryStream memoryStream = new MemoryStream();
                using (Stream stream = process.StandardOutput.BaseStream)
                {
                    byte[] array = new byte[4096];
                    int count;
                    while ((count = stream.Read(array, 0, array.Length)) > 0)
                    {
                        memoryStream.Write(array, 0, count);
                    }
                }

                string message = process.StandardError.ReadToEnd();
                if (memoryStream.Length == 0L)
                {
                    throw new Exception(message);
                }

                process.WaitForExit();
                return memoryStream.ToArray();
            }

            switches = "-q " + switches;
            if (!string.IsNullOrEmpty(html))
            {
                html = SpecialCharsEncode(html);
            }

            Guid guid = Guid.NewGuid();
            File.WriteAllText($"{guid}.html", html);
            switches += $" {guid}.html {guid}.pdf";
            using Process process2 = new Process();
            process2.StartInfo = new ProcessStartInfo
            {
                FileName = text,
                Arguments = switches,
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            process2.Start();
            using MemoryStream memoryStream2 = new MemoryStream();
            string message2 = process2.StandardError.ReadToEnd();
            process2.WaitForExit();
            using (FileStream fileStream = new FileStream($"{guid}.pdf", FileMode.Open, FileAccess.Read))
            {
                fileStream.CopyTo(memoryStream2);
            }

            File.Delete($"{guid}.pdf");
            File.Delete($"{guid}.html");
            if (memoryStream2.Length == 0L)
            {
                throw new Exception(message2);
            }

            return memoryStream2.ToArray();
        }

        private static string SpecialCharsEncode(string text)
        {
            char[] array = text.ToCharArray();
            StringBuilder stringBuilder = new StringBuilder(text.Length + (int)((double)text.Length * 0.1));
            char[] array2 = array;
            foreach (char value in array2)
            {
                int num = System.Convert.ToInt32(value);
                if (num > 127)
                {
                    stringBuilder.AppendFormat("&#{0};", num);
                }
                else
                {
                    stringBuilder.Append(value);
                }
            }

            return stringBuilder.ToString();
        }
    }
}
