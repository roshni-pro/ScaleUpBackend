using ScaleUP.ApiGateways.Aggregator.DTOs;
using ScaleUP.ApiGateways.Aggregator.Services.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.ApiGateways.Aggregator.Services;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts;
using Error = ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.Error;
using Microsoft.AspNetCore.Authorization;
using ScaleUP.Global.Infrastructure.Constants;

namespace ScaleUP.ApiGateways.Aggregator.Managers
{
    public class eNachAggManager
    {
        private ILeadService _iLeadService;
        private ILeadService _leadService;
        private IKycService _iKycService;
        private KYCUserDetailManager _kYCUserDetailManager;

        public eNachAggManager(ILeadService iLeadService, ILeadService leadService, IKycService iKycService, KYCUserDetailManager kYCUserDetailManager)
        {
            _leadService = leadService;
            _iLeadService = iLeadService;
            _iKycService = iKycService;
            _kYCUserDetailManager = kYCUserDetailManager;
        }

        public async Task<eNachSendReqAggDTO> eNachSendeMandateRequest(eNachBankDetailAggDc EMandateBankDetail)
        {
            eNachSendReqAggDTO res = new eNachSendReqAggDTO();

            GRPCRequest<long> leadreq = new GRPCRequest<long>();
            leadreq.Request = EMandateBankDetail.LeadMasterId;
            var leadreply = await _leadService.GetLeadUser(leadreq);

            if (leadreply.Status)
            {
                //01
                GRPCRequest<long> resq = new GRPCRequest<long>();
                resq.Request = EMandateBankDetail.LeadMasterId;
                var resp = await _iLeadService.CheckENachPreviousReq(resq);
                if (resp != null && resp.Response == "Yes")
                {
                    res.status = false;
                    res.error = "Your previous request is under process , please try after 5 minutes.";
                    return res;
                }


                //if (resultSaveBank.Response)
                {
                    List<string> MasterCodeList = new List<string>
                    {
                          KYCMasterConstants.PersonalDetail
                    };

                    LeadPersonalDetailDTO leadPersonalDetailDTO = null;
                    var personalDetail = await _kYCUserDetailManager.GetLeadDetailAll(leadreply.Response.UserName,leadreply.Response.Product, MasterCodeList, IsGetBankStatementDetail: false, IsGetCreditBureau: false, IsGetAgreement: false);

                    //KYCPersonalDetailRequest kycPersonalReq = new KYCPersonalDetailRequest
                    //{
                    //    UserId = leadreply.Response
                    //};
                    //var personalDetail = await _iKycService.GetKYCPersonalDetail(kycPersonalReq);

                    if (personalDetail != null && personalDetail.PersonalDetail != null && personalDetail.PersonalDetail.FirstName != null)
                    {

                        SendeMandateRequestDc SendeMandateRequest = new SendeMandateRequestDc
                        {
                            Customer_AccountNo = EMandateBankDetail.AccountNo,
                            CreatedBy = EMandateBankDetail.Name,
                            CheckSum = "",
                            Customer_EmailId = personalDetail.PersonalDetail.EmailId,
                            Customer_Mobile = personalDetail.PersonalDetail.AlternatePhoneNo,
                            Customer_Name = personalDetail.PersonalDetail.FirstName,
                            LeadId = EMandateBankDetail.LeadMasterId,
                            LeadNo = "",
                            MsgId = "",
                            Short_Code = "",
                            UtilCode = "",

                            Customer_InstructedMemberId = EMandateBankDetail.IfscCode,
                            Channel = EMandateBankDetail.Channel,
                            Filler5 = EMandateBankDetail.AccountType // Account Type: Value should be “S” for Savings, “C” for Current or “O” “Other”.
                        };
                        GRPCRequest<SendeMandateRequestDc> eNachSendeMandateRequest = new GRPCRequest<SendeMandateRequestDc> { Request = SendeMandateRequest };

                        //02
                        var response = await _iLeadService.eNachSendeMandateRequestToHDFCAsync(eNachSendeMandateRequest);

                        if (response.request.MsgId != "")
                        {
                            eNachBankDetailDc eNachBankDetailDc = new eNachBankDetailDc
                            {
                                AccountNo = EMandateBankDetail.AccountNo,
                                AccountType = EMandateBankDetail.AccountType,
                                BankName = EMandateBankDetail.BankName,
                                Channel = EMandateBankDetail.Channel,
                                CreatedBy = EMandateBankDetail.Name, //EMandateBankDetail.CreatedBy,
                                IfscCode = EMandateBankDetail.IfscCode,
                                LeadMasterId = EMandateBankDetail.LeadMasterId,
                                Name = EMandateBankDetail.Name,
                                RelationshipTypes = EMandateBankDetail.RelationshipTypes,
                                MsgId = response.request.MsgId

                            };
                            GRPCRequest<eNachBankDetailDc> eNachAddReq = new GRPCRequest<eNachBankDetailDc> { Request = eNachBankDetailDc };

                            //03
                            var resultSaveBank = await _iLeadService.eNachAddAsync(eNachAddReq);
                        }


                        res.request = response.request;
                        res.status = response.status;
                        res.url = response.url;

                    }

                }
                //return res;

            }


            return res;
        }

        [AllowAnonymous]
        public async Task<bool> eNachAddResponse(eNachRespDocAggDC eNachResponseDC)
        {
            eNachResponseDocDC eNach = new eNachResponseDocDC
            {
                Status = eNachResponseDC.Status,
                MsgId = eNachResponseDC.MsgId,
                RefId = eNachResponseDC.RefId,
                Errors = eNachResponseDC.Errors != null && eNachResponseDC.Errors.Any() ? eNachResponseDC.Errors.Select(x => new Error
                {
                    Error_Message = x.Error_Message,
                    Error_Code = x.Error_Code
                }).ToList() : new List<Error>(),
                Filler1 = eNachResponseDC.Filler1,
                Filler2 = eNachResponseDC.Filler2,
                Filler3 = eNachResponseDC.Filler3,
                Filler4 = eNachResponseDC.Filler4,
                Filler5 = eNachResponseDC.Filler5,
                Filler6 = eNachResponseDC.Filler6,
                Filler7 = eNachResponseDC.Filler7,
                Filler8 = eNachResponseDC.Filler8,
                Filler9 = eNachResponseDC.Filler9,
                Filler10 = eNachResponseDC.Filler10,
            };
            GRPCRequest<eNachResponseDocDC> eNachAddReq = new GRPCRequest<eNachResponseDocDC> { Request = eNach };
            var response = await _iLeadService.eNachAddResponse(eNachAddReq);
            return response.Response;
        }
    }
}
