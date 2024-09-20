using AutoMapper.Execution;
using Google.Protobuf.WellKnownTypes;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Newtonsoft.Json;
using ScaleUP.ApiGateways.Aggregator.DTOs.DSA;
using ScaleUP.ApiGateways.Aggregator.Services;
using ScaleUP.ApiGateways.Aggregator.Services.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Communication.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.DSA;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Media.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts.ArthMate;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts.BlackSoil;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Global.Infrastructure.Constants.AccountTransaction;
using ScaleUP.Global.Infrastructure.Constants.Product;
using System;
using System.Collections.Generic;
using System.Net;

namespace ScaleUP.ApiGateways.Aggregator.Managers.NBFC
{
    public class ArthMateManager
    {
        private ILeadService _iLeadService;
        private IProductService _iproductService;
        private ILoanAccountService _iloanAccountService;
        private ICompanyService _iCompanyService;
        private ICommunicationService _icommunicationService;
        private IMediaService _iMediaService;

        public ArthMateManager(ILeadService iLeadService, IProductService productService, ILoanAccountService loanAccountService, ICompanyService iCompanyService, ICommunicationService icommunicationService, IMediaService imediaService)
        {
            _iLeadService = iLeadService;
            _iproductService = productService;
            _iloanAccountService = loanAccountService;
            _iCompanyService = iCompanyService;
            _icommunicationService = icommunicationService;
            _iMediaService = imediaService;

        }
        public async Task<bool> ArthMateAscoreCallback(string requestcontent)
        {
            if (requestcontent != null)
            {
                var req = JsonConvert.DeserializeObject<AScoreWebhookDc>(requestcontent);
                if (req != null)
                {
                    GRPCRequest<AScoreWebhookDc> _request = new GRPCRequest<AScoreWebhookDc> { Request = req };
                    var gRPCReply = await _iLeadService.ArthMateAScoreCallback(_request);
                }
            }

            return true;
        }

        public async Task<bool> CompositeDisbursement(string requestcontent)
        {
            bool result = false;

            if (requestcontent != null)
            {
                GRPCRequest<ArthmatDisbursementWebhookRequest> _request = new GRPCRequest<ArthmatDisbursementWebhookRequest>
                {
                    Request = new ArthmatDisbursementWebhookRequest { Data = requestcontent }
                };
                var gRPCReply = await _iLeadService.CompositeDisbursement(_request);
                if (gRPCReply.Status)
                {
                    long leadid = Convert.ToInt64(gRPCReply.Response.Loan.LeadMasterId);
                    var res = await GetDisbursementAPI(leadid, gRPCReply.Response.leaddc.LeadCreatedUserId ?? "");
                    result = true;
                }
            }

            return result;
        }
        public async Task<GRPCReply<AgreementResponseDc>> ArthmateGenerateAgreement(long leadId, bool IsSubmit)
        {
            GRPCReply<AgreementResponseDc> reply = new GRPCReply<AgreementResponseDc>();
            var leadReply = await _iLeadService.GetLeadAllCompanyAsync(new GRPCRequest<LeadCompanyConfigProdRequest> { Request = new LeadCompanyConfigProdRequest { AnchorCompanyId = 0, LeadId = leadId } });
            if (leadReply != null)
            {
                var latestGST = await _iCompanyService.GetLatestGSTRate(GstConstant.Gst18);
                var LeadProdReply = await _iLeadService.GetLeadProductId(leadId);
                //var companyConfigReply = await _iproductService.GetLeadCompanyConfig(new CompanyConfigProdRequest
                //{
                //    AnchorCompanyId = LeadProdReply.Response.AnchorCompanyId ?? 0,
                //    NBFCCompanyId = leadReply.Response.NBFCCompanyId,
                //    ProductId = LeadProdReply.Response.ProductId
                //});

                List<long> nbfcids = new List<long>();
                nbfcids.Add(leadReply.Response.NBFCCompanyId);

                var productIds = LeadProdReply.Response.ProductId;

                var nbfcConfig = await _iproductService.GetProductNBFCConfigs(new GRPCRequest<GetProductNBFCConfigRequestDc>
                {
                    Request = new GetProductNBFCConfigRequestDc { NBFCCompanyIds = nbfcids, ProductId = productIds }
                });

                if (nbfcConfig != null && nbfcConfig.Response.Any())
                {
                    ProductCompanyConfigDc companyConfigDc = new ProductCompanyConfigDc();
                    //companyConfigDc.PF = companyConfigReply.AnchorCompanyConfig.ProcessingFeeRate;
                    companyConfigDc.ODCharges = 0;
                    companyConfigDc.BounceCharge = nbfcConfig.Response[0].BounceCharge;
                    companyConfigDc.GST = latestGST.Response;
                    companyConfigDc.PenalPercent = nbfcConfig.Response[0].PenalPercent;
                    companyConfigDc.ODdays = 0;
                    //companyConfigDc.InterestRate = companyConfigReply.AnchorCompanyConfig.AnnualInterestRate ?? 0;
                    companyConfigDc.IseSignEnable = nbfcConfig.Response[0].IseSignEnable;
                    //companyConfigDc.MaxInterestRate = companyConfigReply.AnchorCompanyConfig.MaxInterestRate ?? 0;
                    companyConfigDc.PlatFormFee = nbfcConfig.Response[0].PlatFormFee;

                    companyConfigDc.ProductSlabConfigs = nbfcConfig.Response[0].ProductSlabConfigs;

                    ArthmateAgreementRequest arthmateAgreementRequest = new ArthmateAgreementRequest();
                    arthmateAgreementRequest.AgreementURL = nbfcConfig.Response[0].CustomerAgreementURL;
                    arthmateAgreementRequest.LeadId = leadId;
                    arthmateAgreementRequest.IsSubmit = IsSubmit;
                    arthmateAgreementRequest.ProductCompanyConfig = companyConfigDc;

                    GRPCRequest<ArthmateAgreementRequest> request = new GRPCRequest<ArthmateAgreementRequest> { Request = arthmateAgreementRequest };
                    reply = await _iLeadService.ArthmateGenerateAgreement(request);

                    if (IsSubmit && reply.Status)
                    {
                        GRPCRequest<GRPCHtmlConvertRequest> convertRequest = new GRPCRequest<GRPCHtmlConvertRequest> { Request = new GRPCHtmlConvertRequest { HtmlContent = reply.Response.HtmlContent } };
                        var mediaResponse = await _iMediaService.HtmlToPdf(convertRequest);
                        if (mediaResponse != null && mediaResponse.Response != null && !string.IsNullOrEmpty(mediaResponse.Response.PdfUrl))
                        {
                            GRPCRequest<ArthMateLeadAgreementDc> AgreementRrequest = new GRPCRequest<ArthMateLeadAgreementDc>();
                            ArthMateLeadAgreementDc obj = new ArthMateLeadAgreementDc();
                            obj.LeadId = leadId;
                            obj.AgreementPdfUrl = mediaResponse.Response.PdfUrl;
                            obj.ProductCompanyConfig = companyConfigDc;
                            AgreementRrequest.Request = obj;

                            var res = await _iLeadService.SaveArthMateNBFCAgreement(AgreementRrequest);
                            AgreementResponseDc Agreement = new AgreementResponseDc();
                            Agreement.HtmlContent = res.Response;
                            Agreement.IseSignEnable = reply.Response.IseSignEnable;
                            reply.Response = Agreement;
                        }
                    }
                }
            }
            return reply;
        }

        //new api for eSign

        public async Task<GRPCReply<string>> eSignGenerateAgreement(long leadId, long productId, DSAProfileTypeResponse Type, bool IsSubmit)
        {
            GRPCReply<string> reply = new GRPCReply<string>();
            GetesignRequest getesignRequest = new GetesignRequest();
            getesignRequest.LeadId = leadId;
            getesignRequest.ProductId = productId;
            getesignRequest.AgreementType = Type.DSAType;

            GRPCReply<eSignReply> EsignReply = await _iproductService.GeteSignAgreementData(getesignRequest); //esignAgreemnt URl from ProductQa`s table 

            if (EsignReply != null)
            {
                reply = await _iLeadService.EsignGenerateAgreement(new GRPCRequest<EsignAgreementRequest>
                {
                    Request = new EsignAgreementRequest
                    {
                        AgreementURL = EsignReply.Response.AgreementURL,
                        LeadId = leadId,
                        IsSubmit = IsSubmit,
                        ConnectorPersonalDetail = Type.ConnectorPersonalDetail,
                        DSAPersonalDetail = Type.DSAPersonalDetail,
                        SalesAgentCommissions = Type.SalesAgentCommissions!=null && Type.SalesAgentCommissions.Any() ? Type.SalesAgentCommissions:new List<BuildingBlocks.GRPC.Contracts.Product.DataContracts.DSA.SalesAgentCommissionList>()
                    }


                });
            }
            else
            {
                reply = new GRPCReply<string>();
                reply.Message = "Esign Reply Data Not Found!";
                reply.Status = false;
            }

            return reply;
        }

        public async Task<CompanyConfigReply> LeadCompanyConfig(long leadId, long TranscompanyId = 0)
        {
            CompanyConfigReply companyConfigReply = new CompanyConfigReply();
            var leadRequest = new GRPCRequest<LeadCompanyConfigProdRequest>();
            leadRequest.Request = new LeadCompanyConfigProdRequest
            {
                AnchorCompanyId = TranscompanyId,
                LeadId = leadId
            };
            var leadReply = await _iLeadService.GetLeadAllCompanyAsync(leadRequest);

            if (leadReply != null && leadReply.Status == true)
            {
                companyConfigReply = await _iproductService.GetLeadCompanyConfig(new CompanyConfigProdRequest
                {
                    AnchorCompanyId = TranscompanyId > 0 ? TranscompanyId : leadReply.Response.AnchorCompanyId,
                    NBFCCompanyId = leadReply.Response.NBFCCompanyId,
                    ProductId = leadReply.Response.ProductId,
                });

                var defaultcompany = await _iCompanyService.GetDefaultConfigNBFCCompany(new GRPCRequest<List<long>> { Request = new List<long> { companyConfigReply.NBFCCompanyConfig.CompanyId } });
                companyConfigReply.IsDefaultNBFC = defaultcompany?.Response?.Any() ?? false;
            }

            return companyConfigReply;
        }
        public async Task<bool> SendStampRemainderEmailJob()
        {
            var leadReply = await _iLeadService.GetStampRemainderEmailData();
            if (leadReply != null && leadReply.Status)
            {
                var emailreply = await _icommunicationService.SendEmail(new SendEmailRequest
                {
                    From = leadReply.Response.From ?? "",
                    To = leadReply.Response.To,
                    BCC = leadReply.Response.Bcc ?? "",
                    File = "",
                    OTP = "",
                    Subject = "Arthmate Stamp Remainder",
                    Message = $@"Hello <br>
                                only {leadReply.Response.StampCount} Stamps are left for Arthmate Agreement <Br>
                                Please Update Asap <br>
                                Regards:- ScaleUp"
                });
            }
            return true;
        }

        public async Task<GRPCReply<bool>> GetDisbursementAPI(long LeadId, string LeadCreatedUserId = null)
        {
            GRPCReply<bool> res = new GRPCReply<bool>();
            var gRPCReply = await _iLeadService.GetDisbursementAPI(new GRPCRequest<long> { Request = LeadId });
            if (gRPCReply.Status && gRPCReply.Response.leaddc != null && gRPCReply.Response.leaddc.LeadId > 0)
            {
                var leadrequest = new GRPCRequest<long>();
                leadrequest.Request = Convert.ToInt64(gRPCReply.Response.Loan.LeadMasterId);
                long leadid = Convert.ToInt64(gRPCReply.Response.Loan.LeadMasterId);

                var com = await _iCompanyService.GetCompanyDataById(new GRPCRequest<long> { Request = gRPCReply.Response.leaddc.OfferCompanyId ?? 0 });
                var CompanyDetail = await LeadCompanyConfig(leadid);
                if (CompanyDetail != null)
                {
                    gRPCReply.Response.leaddc.LeadCreatedUserId = LeadCreatedUserId;
                    var pro = await _iproductService.GetProductDataById(new GRPCRequest<long> { Request = gRPCReply.Response.leaddc.ProductId });
                    var anchorcompany = await _iCompanyService.GetCompanyDataById(new GRPCRequest<long> { Request = CompanyDetail.AnchorCompanyConfig.CompanyId });
                    if (pro != null && anchorcompany != null)
                    {
                        ArthMateLoanDataResDc postloan = new ArthMateLoanDataResDc();
                        postloan.Loan = gRPCReply.Response.Loan;
                        gRPCReply.Response.leaddc.NBFCCompanyId = CompanyDetail.AnchorCompanyConfig.CompanyId;
                        gRPCReply.Response.leaddc.AnchorName = anchorcompany.Response.CompanyName;
                        gRPCReply.Response.leaddc.ProductType = pro.Response.ProductType;
                        gRPCReply.Response.leaddc.NBFCCompanyCode = com.Response.IdentificationCode;
                        postloan.leaddc = gRPCReply.Response.leaddc;
                        postloan.arthmateDisbursementdc = gRPCReply.Response.arthmateDisbursementdc;
                        var response = await _iloanAccountService.PostArthMateDataLeadToLoan(postloan);
                        if (response.Status)
                        {
                            long LoanAccountId = response.Response;




                            #region Save Loan BankDetails
                            var leadBankDetail = await _iLeadService.GetLeadBankDetailByLeadId(new GRPCRequest<long> { Request = gRPCReply.Response.leaddc.LeadId });
                            if (leadBankDetail.Status && leadBankDetail.Response != null && leadBankDetail.Response.Any())
                            {
                                List<LeadBankDetailResponse> BankDetailList = new List<LeadBankDetailResponse>();

                                foreach (var item in leadBankDetail.Response)
                                {
                                    var bank = new LeadBankDetailResponse
                                    {
                                        LoanAccountId = LoanAccountId,
                                        AccountHolderName = item.AccountHolderName,
                                        AccountNumber = item.AccountNumber,
                                        AccountType = item.AccountType,
                                        BankName = item.BankName,
                                        Type = item.Type,
                                        IFSCCode = item.IFSCCode,
                                        LeadId = item.LeadId
                                    };
                                    BankDetailList.Add(bank);
                                }
                                GRPCRequest<List<LeadBankDetailResponse>> reqest = new GRPCRequest<List<LeadBankDetailResponse>> { Request = BankDetailList };
                                await _iloanAccountService.SaveLoanBankDetails(reqest);
                            }
                            #endregion

                            var CustUniqueCode = "";

                            foreach (var company in gRPCReply.Response.leaddc.LeadCompanies)
                            {
                                var resp = await _iloanAccountService.SaveLoanAccountCompanyLead(new GRPCRequest<SaveLoanAccountCompanyLeadRequestDC>
                                {
                                    Request = new SaveLoanAccountCompanyLeadRequestDC
                                    {
                                        LoanAccountId = LoanAccountId,
                                        CompanyId = company.CompanyId,
                                        LeadProcessStatus = company.LeadProcessStatus,
                                        UserUniqueCode = company.UserUniqueCode != null ? company.UserUniqueCode : "",
                                        AnchorName = anchorcompany.Response.CompanyName,
                                        LogoURL = anchorcompany.Response.LogoURL,
                                    }
                                });
                                CustUniqueCode = company.UserUniqueCode != null ? company.UserUniqueCode : "";
                            }
                            var UpdateCurrentActivityLeadrequest = new GRPCRequest<long>();
                            UpdateCurrentActivityLeadrequest.Request = leadid;
                            var results = await _iLeadService.UpdateCurrentActivity(UpdateCurrentActivityLeadrequest);//As Per Discussed with Sudeep Sir And Harry Sir

                            res.Message = "success";
                            res.Status = true;
                            res.Response = true;
                        }
                        else
                        {
                            res.Message = response.Message;
                            res.Status = response.Status;
                            res.Response = false;
                        }
                    }
                    else
                    {
                        res.Message = "Product or Anchor company Detail not found.";
                        res.Status = false;
                        res.Response = false;
                    }
                }
                else
                {
                    res.Message = "Company Detail not found.";
                    res.Status = false;
                    res.Response = false;
                }
            }
            else
            {
                res.Message = "Lead Detail not found.";
                res.Status = false;
                res.Response = false;
            }
            return res;
        }
        public async Task<GRPCReply<string>> GetOfferEmiDetailsDownloadPdf(GRPCRequest<EmiDetailReqDc> request)
        {
            GRPCReply<string> reply = new GRPCReply<string>();
            reply = await _iLeadService.GetOfferEmiDetailsDownloadPdf(request);
            return reply;
        }

        public async Task<GRPCReply<string>> AadhaarOtpVerify(GRPCRequest<SecondAadharXMLDc> request)
        {
            GRPCReply<string> res = new GRPCReply<string>();
            var leadReply = await _iLeadService.GetLeadAllCompanyAsync(new GRPCRequest<LeadCompanyConfigProdRequest> { Request = new LeadCompanyConfigProdRequest { AnchorCompanyId = 0, LeadId = request.Request.LeadMasterId } });
            if (leadReply != null)
            {
                var latestGST = await _iCompanyService.GetLatestGSTRate(GstConstant.Gst18);
                var LeadProdReply = await _iLeadService.GetLeadProductId(request.Request.LeadMasterId);

                //var companyConfigReply = await _iproductService.GetLeadCompanyConfig(new CompanyConfigProdRequest
                //{
                //    AnchorCompanyId = LeadProdReply.Response.AnchorCompanyId ?? 0,
                //    NBFCCompanyId = leadReply.Response.NBFCCompanyId,
                //    ProductId = LeadProdReply.Response.ProductId
                //});

                List<long> nbfcids = new List<long>();
                nbfcids.Add(leadReply.Response.NBFCCompanyId);

                var productIds = LeadProdReply.Response.ProductId;

                var nbfcConfig = await _iproductService.GetProductNBFCConfigs(new GRPCRequest<GetProductNBFCConfigRequestDc>
                {
                    Request = new GetProductNBFCConfigRequestDc { NBFCCompanyIds = nbfcids, ProductId = productIds }
                });

                if (nbfcConfig != null && nbfcConfig.Response.Any())
                {
                    var Anchorconfig = nbfcConfig.Response[0].ProductSlabConfigs.Where(x => x.MinLoanAmount <= request.Request.loan_amt && x.MaxLoanAmount >= request.Request.loan_amt).ToList();
                    if (Anchorconfig != null && Anchorconfig.Any())
                    {
                        ProductCompanyConfigDc companyConfigDc = new ProductCompanyConfigDc();
                        companyConfigDc.PF = Anchorconfig.First(x => x.SlabType == SlabTypeConstants.PF).MaxValue;
                        companyConfigDc.ODCharges = nbfcConfig.Response[0].ODCharges;
                        companyConfigDc.BounceCharge = nbfcConfig.Response[0].BounceCharge;
                        companyConfigDc.GST = latestGST.Response;
                        companyConfigDc.PenalPercent = nbfcConfig.Response[0].PenalPercent;
                        companyConfigDc.ODdays = nbfcConfig.Response[0].ODdays;
                        companyConfigDc.InterestRate = Anchorconfig.FirstOrDefault(x => x.SlabType == "ROI").MaxValue;
                        companyConfigDc.IseSignEnable = nbfcConfig.Response[0].IseSignEnable;
                        companyConfigDc.MaxInterestRate = nbfcConfig.Response[0].MaxInterestRate; // must be remove headcode 4
                        request.Request.ProductCompanyConfig = companyConfigDc;

                        res = await _iLeadService.AadhaarOtpVerify(request);
                    }
                }
            }
            return res;
        }
        //public async Task<bool> eSginCallback(string requestcontent)
        //{
        //    if (requestcontent != null)
        //    {
        //        var req = JsonConvert.DeserializeObject<eSignWebhookResponseDc>(requestcontent);
        //        if (req != null)
        //        {
        //            eSignDocumentStatusDc obj = new eSignDocumentStatusDc();
        //            obj.DocumentId = req.documentId;
        //            obj.LeadId = 0;
        //            GRPCRequest<eSignDocumentStatusDc> request = new GRPCRequest<eSignDocumentStatusDc> { Request = obj };

        //             await _dsaManager.eSignDocumentsAsync(request);

        //            //string htmldata = "";
        //            //byte[] file = Convert.FromBase64String(req.files[0]);

        //            //if (file.Length > 0)
        //            //{

        //            //    FileUploadRequest f = new FileUploadRequest();
        //            //    f.FileStream = file;
        //            //    f.SubFolderName = "eSignAgreementBusinessLoan";
        //            //    f.FileExtensionWithoutDot = "pdf";
        //            //    GRPCRequest<FileUploadRequest> request = new GRPCRequest<FileUploadRequest> { Request = f };

        //            //    var res = await _iMediaService.SaveFile(request);
        //            //    FileDc fileDc = new FileDc();
        //            //    fileDc.filePath = res.Response.ImagePath;
        //            //    fileDc.DocId = res.Response.Id ?? 0;

        //            //    //eSignWebhookResponseDc eSignWebhookResponseDc = new eSignWebhookResponseDc();
        //            //    req.fileDc = fileDc;
        //            //    GRPCRequest<eSignWebhookResponseDc> _request = new GRPCRequest<eSignWebhookResponseDc> { Request = req };
        //            //    var gRPCReply = await _iLeadService.eSignCallback(_request);

        //            //}
        //        }
        //    }

        //    return true;
        //}
        public async Task<GRPCReply<string>> GetLeadDetailForDisbursement(NBFCDisbursementPostdc obj)
        {
            GRPCReply<string> response = new GRPCReply<string>();
            var reply = await _iLeadService.GetLeadDetailForDisbursement(obj);

            if (reply.Status && reply.Response != null && reply.Response.DisbursementDetail != null && reply.Response.leadinfo != null && reply.Response.leadinfo.LeadId > 0 && reply.Response.EMISchedule != null && reply.Response.EMISchedule.Count > 0)
            {
                var LeadProdReply = await _iLeadService.GetLeadProductId(obj.LeadId);
                var CompanyDetail = await LeadCompanyConfig(obj.LeadId);
                var pro = await _iproductService.GetProductDataById(new GRPCRequest<long> { Request = reply.Response.leadinfo.ProductId });
                var anchorcompany = await _iCompanyService.GetCompanyDataById(new GRPCRequest<long> { Request = CompanyDetail.AnchorCompanyConfig.CompanyId });

                reply.Response.leadinfo.ProductType = pro.Response.ProductType;
                reply.Response.leadinfo.AnchorName = anchorcompany.Response.CompanyName;
                reply.Response.nbfcUTR = obj.UTR;
                reply.Response.DisbursementDate = obj.DisbursementDate;
                List<LeadCompany> LeadCompanyList = new List<LeadCompany>();
                foreach (var company in reply.Response.leadinfo.LeadCompanies)
                {
                    company.CompanyId = company.CompanyId;
                    company.LeadProcessStatus = company.LeadProcessStatus;
                    company.UserUniqueCode = company.UserUniqueCode != null ? company.UserUniqueCode : "";
                    company.AnchorName = anchorcompany.Response.CompanyName;
                    company.LogoURL = anchorcompany.Response.LogoURL;
                    company.CustUniqueCode = company.UserUniqueCode != null ? company.UserUniqueCode : "";
                    //LeadCompanyList.Add(leadcompany);
                }
                reply.Response.OtherCharges = obj.OtherCharges;
                reply.Response.InsuranceAmount = obj.InsuranceAmount;
                reply.Response.FirstEMIDate = obj.FirstEMIDate;

                DateTime FirstEMIDate = obj.FirstEMIDate;
                DateTime DisbursalDate = obj.DisbursementDate;
                int diffday = (FirstEMIDate - DisbursalDate).Days;
                int BrokenPeriod = diffday != 0 ? (diffday - 30) : 0;
                double? loanAmount = reply.Response.DisbursementDetail.LoanAmount;
                double? roi = reply.Response.DisbursementDetail.InterestRate / 100;
                double cal = ((loanAmount * roi) / 365) ?? 0;
                double BPI = Convert.ToDouble(cal * BrokenPeriod);

                reply.Response.brokenPeriodinterestAmount = Math.Round(BPI, 2);

                GRPCRequest<LeadDetailForDisbursement> req = new GRPCRequest<LeadDetailForDisbursement> { Request = reply.Response };

                var result = await _iloanAccountService.PostNBFCDisbursement(req);
                if (result.Status)
                {
                    GRPCRequest<long> request = new GRPCRequest<long> { Request = obj.LeadId };
                    var res = await _iLeadService.CurrentActivityCompleteForNBFC(request);
                    //response.Response = result.Response;
                    response.Status = result.Status;
                    response.Message = result.Message;
                }
            }
            else
            {
                response.Status = false;
                response.Message = "somthing went wrong";
            }
            return response;
        }
    }
}
