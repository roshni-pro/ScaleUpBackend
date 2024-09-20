using Google.Protobuf.WellKnownTypes;
using IdentityServer4.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.AyeFinSCF;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.NBFC;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.Interfaces.NBFC;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts.BlackSoil;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Global.Infrastructure.Constants.Lead;
using ScaleUP.Global.Infrastructure.Constants.NBFC;
using ScaleUP.Global.Infrastructure.Constants.Product;
using ScaleUP.Global.Infrastructure.Enum;
using ScaleUP.Global.Infrastructure.Helper;
using ScaleUP.Global.Infrastructure.MassTransit;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using ScaleUP.Services.LeadAPI.Helper.NBFC;
using ScaleUP.Services.LeadAPI.Manager;
using ScaleUP.Services.LeadAPI.Persistence;
using ScaleUP.Services.LeadDTO.Constant;
using ScaleUP.Services.LeadDTO.Lead;
using ScaleUP.Services.LeadDTO.NBFC.ArthMate.Request;
using ScaleUP.Services.LeadDTO.NBFC.ArthMate.Response;
using ScaleUP.Services.LeadDTO.NBFC.AyeFinanceSCF.Request;
using ScaleUP.Services.LeadDTO.NBFC.BlackSoil;
using ScaleUP.Services.LeadModels;
using ScaleUP.Services.LeadModels.ArthMate;
using ScaleUP.Services.LeadModels.LeadNBFC;
using System.Drawing.Imaging;
using System.Linq;
using static MassTransit.ValidationResultExtensions;

namespace ScaleUP.Services.LeadAPI.NBFCFactory.Implementation
{
    public class BlackSoilLeadNBFCService : ILeadNBFCService
    {
        private readonly LeadApplicationDbContext _context;
        private readonly LeadNBFCSubActivityManager _leadNBFCSubActivityManager;
        private readonly LeadCommonRequestResponseManager _leadCommonRequestResponseManager;
        private readonly BlackSoilUpdateManager _blackSoilUpdateManager;

        private readonly LeadHistoryManager _leadHistoryManager;
        private readonly IMassTransitService _massTransitService;

        public BlackSoilLeadNBFCService(LeadApplicationDbContext context, LeadNBFCSubActivityManager leadNBFCSubActivityManager, LeadCommonRequestResponseManager leadCommonRequestResponseManager, BlackSoilUpdateManager blackSoilUpdateManager
          , LeadHistoryManager leadHistoryManager, IMassTransitService massTransitService)
        {
            _context = context;
            _leadNBFCSubActivityManager = leadNBFCSubActivityManager;
            _leadCommonRequestResponseManager = leadCommonRequestResponseManager;
            _blackSoilUpdateManager = blackSoilUpdateManager;
            _leadHistoryManager = leadHistoryManager;
            _massTransitService = massTransitService;

        }

        //public async Task<ICreateLeadNBFCResponse> CreateLead(UserDetailsReply userDetail, long leadid, bool isTest = false)
        //{
        //    LeadNBFCSubActivityDTO api = null;
        //    BlackSoilCreateLeadInput input = null;

        //    #region create input
        //    string businessAddress = "";
        //    if (!string.IsNullOrEmpty(userDetail.BuisnessDetail.CurrentAddress.AddressLineOne))
        //    {
        //        businessAddress = userDetail.BuisnessDetail.CurrentAddress.AddressLineOne;
        //    }
        //    if (!string.IsNullOrEmpty(userDetail.BuisnessDetail.CurrentAddress.AddressLineTwo))
        //    {
        //        businessAddress = businessAddress + ", " + userDetail.BuisnessDetail.CurrentAddress.AddressLineTwo;
        //    }
        //    if (!string.IsNullOrEmpty(userDetail.BuisnessDetail.CurrentAddress.AddressLineThree))
        //    {
        //        businessAddress = businessAddress + ", " + userDetail.BuisnessDetail.CurrentAddress.AddressLineThree;
        //    }
        //    if (!string.IsNullOrEmpty(userDetail.BuisnessDetail.CurrentAddress.CityName))
        //    {
        //        businessAddress = businessAddress + ", " + userDetail.BuisnessDetail.CurrentAddress.CityName;
        //    }
        //    if (!string.IsNullOrEmpty(userDetail.BuisnessDetail.CurrentAddress.StateName))
        //    {
        //        businessAddress = businessAddress + ", " + userDetail.BuisnessDetail.CurrentAddress.StateName;
        //    }


        //    string currentAddress = "";
        //    if (!string.IsNullOrEmpty(userDetail.PersonalDetail.CurrentAddress.AddressLineOne))
        //    {
        //        currentAddress = userDetail.PersonalDetail.CurrentAddress.AddressLineOne;
        //    }
        //    if (!string.IsNullOrEmpty(userDetail.PersonalDetail.CurrentAddress.AddressLineTwo))
        //    {
        //        currentAddress = currentAddress + ", " + userDetail.PersonalDetail.CurrentAddress.AddressLineTwo;
        //    }
        //    if (!string.IsNullOrEmpty(userDetail.PersonalDetail.CurrentAddress.AddressLineThree))
        //    {
        //        currentAddress = currentAddress + ", " + userDetail.PersonalDetail.CurrentAddress.AddressLineThree;
        //    }
        //    if (!string.IsNullOrEmpty(userDetail.PersonalDetail.CurrentAddress.CityName))
        //    {
        //        currentAddress = currentAddress + ", " + userDetail.PersonalDetail.CurrentAddress.CityName;
        //    }
        //    if (!string.IsNullOrEmpty(userDetail.PersonalDetail.CurrentAddress.StateName))
        //    {
        //        currentAddress = currentAddress + ", " + userDetail.PersonalDetail.CurrentAddress.StateName;
        //    }


        //    string aadharImage = await FileSaverHelper.GetStreamFromUrlBase64(userDetail.aadharDetail.FrontImageUrl);
        //    string panImage = await FileSaverHelper.GetStreamFromUrlBase64(userDetail.panDetail.FrontImageUrl);


        //    input = new BlackSoilCreateLeadInput
        //    {
        //        aadhaar_file = aadharImage,
        //        business_address = new BlackSoilCreateLeadAddressInput
        //        {
        //            address_line = businessAddress,
        //            pincode = userDetail.BuisnessDetail.CurrentAddress.ZipCode.ToString(),
        //            city = userDetail.BuisnessDetail.CurrentAddress.CityName,
        //            state = userDetail.BuisnessDetail.CurrentAddress.StateName
        //        },
        //        person_address = new BlackSoilCreateLeadAddressInput
        //        {
        //            address_line = currentAddress,
        //            pincode = userDetail.PersonalDetail.CurrentAddress.ZipCode.ToString(),
        //            city = userDetail.PersonalDetail.CurrentAddress.CityName,
        //            state = userDetail.PersonalDetail.CurrentAddress.StateName
        //        },
        //        business_type = userDetail.BuisnessDetail.BusEntityType.ToLower(),
        //        dob = userDetail.panDetail.DOB.ToString("yyyy-MM-dd"),
        //        full_name = userDetail.panDetail.NameOnCard,
        //        gender = userDetail.PersonalDetail.Gender == "M" ? "male" : "female",
        //        mobile = userDetail.PersonalDetail.MobileNo,
        //        name = userDetail.panDetail.NameOnCard,
        //        pan_file = panImage,
        //        referral_code = "SHOP0154",
        //        aadhaar = userDetail.aadharDetail.UniqueId
        //    };

        //    #endregion

        //    #region logic to get credentials
        //    if (isTest)
        //    {
        //        api = new LeadNBFCSubActivityDTO
        //        {
        //            TAPIKey = "shopkirana@saraloan.in",
        //            TAPISecretKey = "shopkirana@saraloan.in",
        //            APIUrl = "https://stag.saraloan.in/api/v1/core/businesses/",
        //            Sequence = 1,

        //        };

        //    }
        //    else
        //    {
        //        LeadNBFCSubActivityManager manager = new LeadNBFCSubActivityManager(_context);
        //        string subactivityCode = SubActivityConstants.CreateLead;
        //        var list = await manager.GetLeadNBFCSubActivity(new LeadNBFCSubActivityRequest
        //        {
        //            Code = subactivityCode,
        //            LeadId = leadid,
        //            CompanyIdentificationCode = CompanyIdentificationCodeConstants.BlackSoil
        //        });

        //        if (list != null && list.Count > 0)
        //        {
        //            api = list.FirstOrDefault();
        //            if(api.Status == LeadNBFCApiConstants.Completed || api.Status == LeadNBFCApiConstants.CompletedWithError)
        //            {
        //                return new CreateLeadNBFCResponse
        //                {
        //                    IsSuccess = false,
        //                    Message = "api already completed/completed with error"
        //                };
        //            }
        //        }
        //        else
        //        {
        //            return new CreateLeadNBFCResponse
        //            {
        //                IsSuccess = false,
        //                Message = "not found any api configuration for same"
        //            };
        //        }
        //    }


        //    #endregion

        //    BlackSoilNBFCHelper helper = new BlackSoilNBFCHelper();

        //    var response = await helper.CreateLead(input, api.APIUrl, api.TAPIKey, api.TAPISecretKey);
        //    if (response.IsSuccess)
        //    {
        //        var BlackSoilUpdate = new BlackSoilUpdate();
        //        var deserializeobject = JsonConvert.DeserializeObject<BlackSoilCreateLeadResponse>(response.Response);
        //        if (deserializeobject != null)
        //        {
        //            BlackSoilUpdate.BusinessId = deserializeobject.id;
        //            BlackSoilUpdate.BusinessUpdateUrl = deserializeobject.update_url;

        //            BlackSoilUpdate.BusinessAddressId = deserializeobject.business_address.id;
        //            BlackSoilUpdate.BusinessUpdateUrl = deserializeobject.business_address.update_url;

        //            BlackSoilUpdate.PersonId = deserializeobject.person.id;
        //            BlackSoilUpdate.PersonUpdateUrl = deserializeobject.person.update_url;

        //            BlackSoilUpdate.PersonAddressId = deserializeobject.person_address.id;
        //            BlackSoilUpdate.PersonAddressUpdateUrl = deserializeobject.person_address.update_url;


        //            BlackSoilUpdate.PanId = deserializeobject.pan.id;
        //            BlackSoilUpdate.PanUpdateUrl = deserializeobject.pan.update_url;

        //            BlackSoilUpdate.AadhaarId = deserializeobject.aadhaar.id;
        //            BlackSoilUpdate.AadhaarUpdateUrl = deserializeobject.aadhaar.update_url;

        //            BlackSoilUpdate.Created = DateTime.Now;
        //            BlackSoilUpdate.IsActive = true;
        //            BlackSoilUpdate.IsDeleted = false;
        //            _context.BlackSoilUpdates.Add(BlackSoilUpdate);
        //        }
        //    }

        //    _context.CommonAPIRequestResponses.Add(response);
        //    _context.SaveChanges();

        //    return new CreateLeadNBFCResponse
        //    {
        //        IsSuccess = true,
        //        Message = "BlackSoilLeadNBFCService"
        //    };
        //}

        #region generate offer private methods
        private async Task<ICreateLeadNBFCResponse> CreateLead(long leadid, bool isTest = false)
        {

            CreateLeadNBFCResponse createLeadResponse = null;

            LeadNBFCSubActivityDTO api = null;
            BlackSoilCreateLeadInput input = null;

            PersonalDetail personalDetail = _context.PersonalDetails.Where(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted).FirstOrDefault();
            BusinessDetail businessDetail = _context.BusinessDetails.Where(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted).FirstOrDefault();

            #region create input
            string businessAddress = "";
            if (!string.IsNullOrEmpty(businessDetail.AddressLineOne))
            {
                businessAddress = businessDetail.AddressLineOne;
            }
            if (!string.IsNullOrEmpty(businessDetail.AddressLineTwo))
            {
                businessAddress = businessAddress + ", " + businessDetail.AddressLineTwo;
            }
            if (!string.IsNullOrEmpty(businessDetail.AddressLineThree))
            {
                businessAddress = businessAddress + ", " + businessDetail.AddressLineThree;
            }
            if (!string.IsNullOrEmpty(businessDetail.CityName))
            {
                businessAddress = businessAddress + ", " + businessDetail.CityName;
            }
            if (!string.IsNullOrEmpty(businessDetail.StateName))
            {
                businessAddress = businessAddress + ", " + businessDetail.StateName;
            }

            string currentAddress = "";
            if (!string.IsNullOrEmpty(personalDetail.CurrentAddressLineOne))
            {
                currentAddress = personalDetail.CurrentAddressLineOne;
            }
            if (!string.IsNullOrEmpty(personalDetail.CurrentAddressLineTwo))
            {
                currentAddress = currentAddress + ", " + personalDetail.CurrentAddressLineTwo;
            }
            if (!string.IsNullOrEmpty(personalDetail.CurrentAddressLineThree))
            {
                currentAddress = currentAddress + ", " + personalDetail.CurrentAddressLineThree;
            }
            if (!string.IsNullOrEmpty(personalDetail.CurrentCityName))
            {
                currentAddress = currentAddress + ", " + personalDetail.CurrentCityName;
            }
            if (!string.IsNullOrEmpty(personalDetail.CurrentStateName))
            {
                currentAddress = currentAddress + ", " + personalDetail.CurrentStateName;
            }

            string panImage = await FileSaverHelper.GetStreamFromUrlBase64(personalDetail.PanFrontImage);

            #region aadharCombineImage
            List<string> files = new List<string> { personalDetail.AadharFrontImage, personalDetail.AadharBackImage };
            var combineBitmap = FileSaverHelper.CombineBitmap(files.ToArray());
            System.IO.MemoryStream ms = new MemoryStream();
            combineBitmap.Save(ms, ImageFormat.Jpeg);
            byte[] byteImage = ms.ToArray();

            string aadharImage = Convert.ToBase64String(byteImage);
            #endregion
            //string aadharImage = await FileSaverHelper.GetStreamFromUrlBase64(personalDetail.AadharFrontImage);
            input = new BlackSoilCreateLeadInput
            {
                aadhaar_file = aadharImage,
                business_address = new BlackSoilCreateLeadAddressInput
                {
                    address_line = businessAddress,
                    pincode = businessDetail.ZipCode.ToString(),
                    city = businessDetail.CityName,
                    state = businessDetail.StateName
                },
                person_address = new BlackSoilCreateLeadAddressInput
                {
                    address_line = currentAddress,
                    pincode = personalDetail.CurrentZipCode.ToString(),
                    city = personalDetail.CurrentCityName,
                    state = personalDetail.CurrentStateName
                },
                business_type = businessDetail.BusEntityType.ToLower(),
                dob = personalDetail.DOB.ToString("yyyy-MM-dd"),
                full_name = personalDetail.PanNameOnCard,
                gender = personalDetail.Gender == "M" ? "male" : "female",
                mobile = personalDetail.MobileNo,
                name = businessDetail.BusinessName,
                pan_file = panImage,
                referral_code = "",
                aadhaar = personalDetail.AadhaarMaskNO.ToString(),
            };



            #endregion

            #region logic to get credentials
            if (isTest)
            {
                api = new LeadNBFCSubActivityDTO
                {
                    TAPIKey = "shopkirana@saraloan.in",
                    TAPISecretKey = "shopkirana@saraloan.in",
                    APIUrl = "https://stag.saraloan.in/api/v1/core/businesses/",
                    Sequence = 1,

                };

            }
            else
            {
                string subactivityCode = SubActivityConstants.CreateLead;
                var list = await _leadNBFCSubActivityManager.GetLeadNBFCSubActivity(new LeadNBFCSubActivityRequest
                {
                    Code = subactivityCode,
                    LeadId = leadid,
                    CompanyIdentificationCode = CompanyIdentificationCodeConstants.BlackSoil
                });

                if (list != null && list.Count > 0)
                {
                    api = list.FirstOrDefault();
                    if (api.Status == LeadNBFCApiConstants.Completed || api.Status == LeadNBFCApiConstants.CompletedWithError)
                    {
                        return new CreateLeadNBFCResponse
                        {
                            IsSuccess = true,
                            Message = "api already completed/completed with error"
                        };
                    }
                }
                else
                {
                    return new CreateLeadNBFCResponse
                    {
                        IsSuccess = true,
                        Message = "not found any api configuration for same"
                    };
                }
            }

            _leadNBFCSubActivityManager.UpdateStatus(api.LeadNBFCApiId, LeadNBFCApiConstants.Inprogress);
            _leadNBFCSubActivityManager.UpdateSubActivityStatus(api.LeadNBFCSubActivityId, LeadNBFCSubActivityConstants.Inprogress);
            #endregion
            input.referral_code = api.ReferrelCode;

            BlackSoilNBFCHelper helper = new BlackSoilNBFCHelper();

            var response = await helper.CreateLead(input, api.APIUrl, api.TAPIKey, api.TAPISecretKey, api.LeadNBFCApiId, leadid);
            if (response.IsSuccess)
            {
                var BlackSoilUpdate = new BlackSoilUpdate();
                var deserializeobject = JsonConvert.DeserializeObject<BlackSoilCreateLeadResponse>(response.Response);
                if (deserializeobject != null)
                {
                    BlackSoilUpdate.BusinessId = deserializeobject.id;
                    BlackSoilUpdate.BusinessUpdateUrl = deserializeobject.update_url ?? "";

                    BlackSoilUpdate.BusinessAddressId = deserializeobject.business_address.id;
                    BlackSoilUpdate.BusinessAddressUpdateUrl = deserializeobject.business_address.update_url ?? "";

                    BlackSoilUpdate.PersonId = deserializeobject.person.id;
                    BlackSoilUpdate.PersonUpdateUrl = deserializeobject.person.update_url ?? "";

                    BlackSoilUpdate.PersonAddressId = deserializeobject.person_address.id;
                    BlackSoilUpdate.PersonAddressUpdateUrl = deserializeobject.person_address.update_url ?? "";


                    BlackSoilUpdate.PanId = deserializeobject.pan.id;
                    BlackSoilUpdate.PanUpdateUrl = deserializeobject.pan.update_url ?? "";

                    BlackSoilUpdate.AadhaarId = deserializeobject.aadhaar.id;
                    BlackSoilUpdate.AadhaarUpdateUrl = deserializeobject.aadhaar.update_url ?? "";

                    BlackSoilUpdate.BussinessCode = deserializeobject.business_id ?? "";

                    BlackSoilUpdate.Created = DateTime.Now;
                    BlackSoilUpdate.IsActive = true;
                    BlackSoilUpdate.IsDeleted = false;
                    BlackSoilUpdate.LeadId = leadid;
                    _context.BlackSoilUpdates.Add(BlackSoilUpdate);
                }

                createLeadResponse = new CreateLeadNBFCResponse
                {
                    IsSuccess = true,
                    Message = "Create lead successfully"
                };
            }
            else
            {
                createLeadResponse = new CreateLeadNBFCResponse
                {
                    IsSuccess = false,
                    Message = "Error while creting lead"
                };
            }
            _context.BlackSoilCommonAPIRequestResponses.Add(response);
            _context.SaveChanges();

            string status = LeadNBFCApiConstants.Completed;
            if (!response.IsSuccess)
            {
                status = LeadNBFCApiConstants.Error;
            }
            _leadNBFCSubActivityManager.UpdateStatus(api.LeadNBFCApiId, status, response.Id);
            _leadNBFCSubActivityManager.UpdateSubActivityStatus(api.LeadNBFCSubActivityId, status);

            await _leadCommonRequestResponseManager.SaveLeadCommonRequestResponse(new LeadCommonRequestInput { CommonRequestResponseId = response.Id, LeadId = leadid });

            return createLeadResponse;
        }

        private async Task<ICreateLeadNBFCResponse> SendToLosInner(LeadNBFCSubActivityDTO api, long leadid, long nbfccompanyid, bool isTest = false)
        {
            CreateLeadNBFCResponse result = null;
            var leadOffer = _context.LeadOffers.Where(x => x.LeadId == leadid && x.NBFCCompanyId == nbfccompanyid && x.IsActive && !x.IsDeleted).FirstOrDefault();
            if (leadOffer != null)
            {
                var blacksoilupdate = _context.BlackSoilUpdates.Where(x => x.LeadId == leadid && !x.IsDeleted && x.IsActive).FirstOrDefault();
                if (blacksoilupdate != null)
                {
                    #region logic to get credentials
                    if (isTest)
                    {
                        api = new LeadNBFCSubActivityDTO
                        {
                            TAPIKey = "shopkirana@saraloan.in",
                            TAPISecretKey = "shopkirana@saraloan.in",
                            APIUrl = "https://stag.saraloan.in/api/v1/core/businesses/{{BUSINESS_ID}}/send_to_los/",
                            Sequence = 2,
                        };
                    }
                    else
                    {


                        if (api != null)
                        {
                            if (api.Status == LeadNBFCApiConstants.Completed || api.Status == LeadNBFCApiConstants.CompletedWithError)
                            {
                                return new CreateLeadNBFCResponse
                                {
                                    IsSuccess = true,
                                    Message = "api already completed/completed with error"
                                };
                            }
                        }
                        else
                        {
                            return new CreateLeadNBFCResponse
                            {
                                IsSuccess = true,
                                Message = "not found any api configuration for same"
                            };
                        }
                    }
                    _leadNBFCSubActivityManager.UpdateSubActivityStatus(api.LeadNBFCSubActivityId, LeadNBFCSubActivityConstants.Inprogress);
                    
                    //------------------S : Make log---------------------
                    #region Make History
                    string doctypeSendToLos = "SendToLosInprogress";

                    var resultHistory = await _leadHistoryManager.GetLeadHistroy(leadid, doctypeSendToLos);
                    LeadUpdateHistoryEvent histroyEvent = new LeadUpdateHistoryEvent
                    {
                        LeadId = leadid,
                        UserID = resultHistory.UserId,
                        UserName = "",
                        EventName = doctypeSendToLos,//context.Message.KYCMasterCode, //result.EntityIDofKYCMaster.ToString(),
                        Narretion = resultHistory.Narretion,
                        NarretionHTML = resultHistory.NarretionHTML,
                        CreatedTimeStamp = resultHistory.CreatedTimeStamp
                    };
                    await _massTransitService.Publish(histroyEvent);
                    #endregion
                    //------------------E : Make log---------------------

                    _leadNBFCSubActivityManager.UpdateStatus(api.LeadNBFCApiId, LeadNBFCApiConstants.Inprogress);
                    #endregion
                    api.APIUrl = api.APIUrl.Replace("{{BUSINESS_ID}}", blacksoilupdate.BusinessId.ToString());


                    BlackSoilNBFCHelper helper = new BlackSoilNBFCHelper();

                    var response = await helper.SendToLos(api.APIUrl, api.TAPIKey, api.TAPISecretKey, api.LeadNBFCApiId, leadid);
                    _context.BlackSoilCommonAPIRequestResponses.Add(response);
                    _context.SaveChanges();
                    string status = LeadNBFCApiConstants.Completed;
                    if (!response.IsSuccess)
                    {
                        status = LeadNBFCApiConstants.Error;
                    }

                    _leadNBFCSubActivityManager.UpdateStatus(api.LeadNBFCApiId, status, response.Id);
                    await _leadCommonRequestResponseManager.SaveLeadCommonRequestResponse(new LeadCommonRequestInput { CommonRequestResponseId = response.Id, LeadId = leadid });


                    return new CreateLeadNBFCResponse
                    {
                        IsSuccess = response.IsSuccess,
                        Message = "Error from third api call"
                    };


                    //_leadNBFCSubActivityManager.UpdateSubActivityStatus(api.LeadNBFCSubActivityId, status);
                }
                else
                {
                    return new CreateLeadNBFCResponse
                    {
                        IsSuccess = false,
                        Message = "businessId not found in BlackSoilUpdates"
                    };
                }



            }
            return new CreateLeadNBFCResponse
            {
                IsSuccess = true,
                Message = "BlackSoilLeadNBFCService"
            };
        }

        private async Task<ICreateLeadNBFCResponse> CreateBusinessPurchaseInvoice(LeadNBFCSubActivityDTO api, long leadid, long nbfccompanyid, bool isTest = false)
        {
            CreateLeadNBFCResponse result = null;

            var query = from h in _context.LeadCompanyBuyingHistorys
                        join c in _context.CompanyLead on h.CompanyLeadId equals c.Id
                        join l in _context.Leads on c.LeadId equals l.Id
                        where h.CompanyLeadId == c.Id && h.IsActive == true && h.IsDeleted == false && c.LeadProcessStatus == 2
                        && c.IsActive == true && h.IsActive == true && l.Id == leadid
                        group h by new { h.MonthFirstBuyingDate.Month, h.MonthFirstBuyingDate.Year } into grp
                        select new BlackSoilCreateBusinessPurchaseInvoiceInput
                        {
                            MonthFirstBuyingDate = new DateTime(grp.Key.Year, grp.Key.Month, 01),
                            TotalMonthInvoice = grp.Sum(x => x.TotalMonthInvoice),
                            MonthTotalAmount = grp.Sum(x => x.MonthTotalAmount),
                        };
            var queryresult = query.ToList();
            if (queryresult != null && queryresult.Any())
            {
                var blacksoilupdate = _context.BlackSoilUpdates.Where(x => x.LeadId == leadid && !x.IsDeleted && x.IsActive).FirstOrDefault();

                if (blacksoilupdate != null)
                {
                    #region logic to get credentials
                    if (isTest)
                    {
                        api = new LeadNBFCSubActivityDTO
                        {
                            TAPIKey = "shopkirana@saraloan.in",
                            TAPISecretKey = "shopkirana@saraloan.in",
                            APIUrl = "https://stag.saraloan.in/api/v1/core/businesses/{{BUSINESS_ID}}/send_to_los/",
                            Sequence = 2,
                        };
                    }
                    else
                    {


                        if (api != null)
                        {
                            if (api.Status == LeadNBFCApiConstants.Completed || api.Status == LeadNBFCApiConstants.CompletedWithError)
                            {
                                return new CreateLeadNBFCResponse
                                {
                                    IsSuccess = true,
                                    Message = "api already completed/completed with error"
                                };
                            }
                        }
                        else
                        {
                            return new CreateLeadNBFCResponse
                            {
                                IsSuccess = true,
                                Message = "not found any api configuration for same"
                            };
                        }
                    }
                    _leadNBFCSubActivityManager.UpdateSubActivityStatus(api.LeadNBFCSubActivityId, LeadNBFCSubActivityConstants.Inprogress);

                    _leadNBFCSubActivityManager.UpdateStatus(api.LeadNBFCApiId, LeadNBFCApiConstants.Inprogress);
                    #endregion
                    api.APIUrl = api.APIUrl.Replace("{{BUSINESS_ID}}", blacksoilupdate.BusinessId.ToString());


                    BlackSoilNBFCHelper helper = new BlackSoilNBFCHelper();

                    bool Issuccess = false;
                    BlackSoilCommonAPIRequestResponse response = null;
                    foreach (var item in queryresult)
                    {
                        response = await helper.CreateBusinessPurchaseInvoice(item, api.APIUrl, api.TAPIKey, api.TAPISecretKey, api.LeadNBFCApiId, leadid);
                        if (response != null && response.IsSuccess)
                        {
                            Issuccess = true;
                        }
                        _context.BlackSoilCommonAPIRequestResponses.Add(response);
                        _context.SaveChanges();
                    }
                    string status = LeadNBFCApiConstants.Completed;
                    if (!Issuccess)
                    {
                        status = LeadNBFCApiConstants.Error;
                    }
                    _leadNBFCSubActivityManager.UpdateStatus(api.LeadNBFCApiId, status, response.Id);
                    await _leadCommonRequestResponseManager.SaveLeadCommonRequestResponse(new LeadCommonRequestInput { CommonRequestResponseId = response.Id, LeadId = leadid });
                    return new CreateLeadNBFCResponse
                    {
                        IsSuccess = Issuccess,
                        Message = "Error from third api call"
                    };
                }
                else
                {
                    return new CreateLeadNBFCResponse
                    {
                        IsSuccess = false,
                        Message = "Configuration not found."
                    };
                }
            }
            else
            {
                _leadNBFCSubActivityManager.UpdateStatus(api.LeadNBFCApiId, LeadNBFCApiConstants.Completed, 0);



            }
            return new CreateLeadNBFCResponse
            {
                IsSuccess = true,
                Message = "BlackSoilLeadNBFCService"
            };
        }
        private async Task<ICreateLeadNBFCResponse> CreateBusinessDocument(LeadNBFCSubActivityDTO api, long leadid, long nbfccompanyid, bool isTest = false)
        {
            CreateLeadNBFCResponse result = null;
            var queryresult = await _context.LeadDocumentDetails.Where(x => x.LeadId == leadid &&
            (x.DocumentName == BlackSoilBusinessDocNameConstants.GstCertificate ||
            x.DocumentName == BlackSoilBusinessDocNameConstants.Other ||
            x.DocumentName == BlackSoilBusinessDocNameConstants.UdyogAadhaar ||
            x.DocumentName == BlackSoilBusinessDocNameConstants.Statement
            )
             && x.IsActive && !x.IsDeleted).Select(x => new BlackSoilCreateBusinessDocumentInput
             {
                 doc_name = x.DocumentName,
                 doc_number = x.DocumentNumber,
                 doc_type = x.DocumentType,
                 files = x.FileUrl
             }).ToListAsync();
            if (queryresult != null && queryresult.Any())
            {
                var blacksoilupdate = _context.BlackSoilUpdates.Where(x => x.LeadId == leadid && !x.IsDeleted && x.IsActive).FirstOrDefault();

                if (blacksoilupdate != null)
                {
                    #region logic to get credentials
                    if (isTest)
                    {
                        api = new LeadNBFCSubActivityDTO
                        {
                            TAPIKey = "shopkirana@saraloan.in",
                            TAPISecretKey = "shopkirana@saraloan.in",
                            APIUrl = "https://stag.saraloan.in/api/v1/core/businesses/{{BUSINESS_ID}}/send_to_los/",
                            Sequence = 2,
                        };
                    }
                    else
                    {


                        if (api != null)
                        {
                            if (api.Status == LeadNBFCApiConstants.Completed || api.Status == LeadNBFCApiConstants.CompletedWithError)
                            {
                                return new CreateLeadNBFCResponse
                                {
                                    IsSuccess = true,
                                    Message = "api already completed/completed with error"
                                };
                            }
                        }
                        else
                        {
                            return new CreateLeadNBFCResponse
                            {
                                IsSuccess = true,
                                Message = "not found any api configuration for same"
                            };
                        }
                    }
                    _leadNBFCSubActivityManager.UpdateSubActivityStatus(api.LeadNBFCSubActivityId, LeadNBFCSubActivityConstants.Inprogress);

                    _leadNBFCSubActivityManager.UpdateStatus(api.LeadNBFCApiId, LeadNBFCApiConstants.Inprogress);
                    #endregion
                    api.APIUrl = api.APIUrl.Replace("{{BUSINESS_ID}}", blacksoilupdate.BusinessId.ToString());


                    BlackSoilNBFCHelper helper = new BlackSoilNBFCHelper();

                    bool Issuccess = false;
                    BlackSoilCommonAPIRequestResponse response = null;
                    foreach (var item in queryresult)
                    {
                        response = await helper.CreateBusinessDocument(item, api.APIUrl, api.TAPIKey, api.TAPISecretKey, api.LeadNBFCApiId, leadid);
                        if (response != null && response.IsSuccess)
                        {
                            Issuccess = true;
                        }
                        _context.BlackSoilCommonAPIRequestResponses.Add(response);
                        _context.SaveChanges();
                    }
                    string status = LeadNBFCApiConstants.Completed;
                    if (!Issuccess)
                    {
                        status = LeadNBFCApiConstants.Error;
                    }
                    _leadNBFCSubActivityManager.UpdateStatus(api.LeadNBFCApiId, status, response.Id);
                    await _leadCommonRequestResponseManager.SaveLeadCommonRequestResponse(new LeadCommonRequestInput { CommonRequestResponseId = response.Id, LeadId = leadid });
                    return new CreateLeadNBFCResponse
                    {
                        IsSuccess = Issuccess,
                        Message = "Error from third api call"
                    };
                }
                else
                {
                    return new CreateLeadNBFCResponse
                    {
                        IsSuccess = false,
                        Message = "Configuration not found."
                    };
                }
            }
            else
            {
                _leadNBFCSubActivityManager.UpdateStatus(api.LeadNBFCApiId, LeadNBFCApiConstants.Completed, 0);

            }
            return new CreateLeadNBFCResponse
            {
                IsSuccess = true,
                Message = "BlackSoilLeadNBFCService"
            };
        }
        private async Task<ICreateLeadNBFCResponse> BlackSoilEmailPost(LeadNBFCSubActivityDTO api, long leadid, long nbfccompanyid, bool isTest = false)
        {
            CreateLeadNBFCResponse result = null;
            var blacksoilupdate = _context.BlackSoilUpdates.Where(x => x.LeadId == leadid && !x.IsDeleted && x.IsActive).FirstOrDefault();
            var personalDetail = _context.PersonalDetails.Where(x => x.LeadId == leadid && !x.IsDeleted && x.IsActive).FirstOrDefault();

            if (blacksoilupdate != null && personalDetail != null && !string.IsNullOrEmpty(personalDetail.EmailId))
            {
                #region logic to get credentials
                if (isTest)
                {
                    api = new LeadNBFCSubActivityDTO
                    {
                        TAPIKey = "shopkirana@saraloan.in",
                        TAPISecretKey = "shopkirana@saraloan.in",
                        APIUrl = "https://stag.saraloan.in/api/v1/core/businesses/{{BUSINESS_ID}}/send_to_los/",
                        Sequence = 2,
                    };
                }
                else
                {

                    if (api != null)
                    {
                        if (api.Status == LeadNBFCApiConstants.Completed || api.Status == LeadNBFCApiConstants.CompletedWithError)
                        {
                            return new CreateLeadNBFCResponse
                            {
                                IsSuccess = true,
                                Message = "api already completed/completed with error"
                            };
                        }
                    }
                    else
                    {
                        return new CreateLeadNBFCResponse
                        {
                            IsSuccess = true,
                            Message = "not found any api configuration for same"
                        };
                    }
                }
                _leadNBFCSubActivityManager.UpdateSubActivityStatus(api.LeadNBFCSubActivityId, LeadNBFCSubActivityConstants.Inprogress);

                _leadNBFCSubActivityManager.UpdateStatus(api.LeadNBFCApiId, LeadNBFCApiConstants.Inprogress);
                #endregion
                api.APIUrl = api.APIUrl.Replace("{{PERSON_ID}}", blacksoilupdate.PersonId.ToString());


                BlackSoilNBFCHelper helper = new BlackSoilNBFCHelper();

                bool Issuccess = false;
                var response = await helper.BlackSoilEmailPost(api.APIUrl, personalDetail.EmailId, api.TAPIKey, api.TAPISecretKey, api.LeadNBFCApiId, leadid);
                if (response != null && response.IsSuccess)
                {
                    Issuccess = true;
                }
                _context.BlackSoilCommonAPIRequestResponses.Add(response);
                _context.SaveChanges();

                string status = LeadNBFCApiConstants.Completed;
                if (!Issuccess)
                {
                    status = LeadNBFCApiConstants.Error;
                }
                _leadNBFCSubActivityManager.UpdateStatus(api.LeadNBFCApiId, status, response.Id);
                await _leadCommonRequestResponseManager.SaveLeadCommonRequestResponse(new LeadCommonRequestInput { CommonRequestResponseId = response.Id, LeadId = leadid });
                return new CreateLeadNBFCResponse
                {
                    IsSuccess = Issuccess,
                    Message = "Error from third api call"
                };
            }
            else
            {
                return new CreateLeadNBFCResponse
                {
                    IsSuccess = false,
                    Message = "Configuration not found."
                };
            }

        }

        private async Task<ICreateLeadNBFCResponse> BlackSoilSelfiePost(LeadNBFCSubActivityDTO api, long leadid, long nbfccompanyid, bool isTest = false)
        {
            CreateLeadNBFCResponse result = null;
            var blacksoilupdate = _context.BlackSoilUpdates.Where(x => x.LeadId == leadid && !x.IsDeleted && x.IsActive).FirstOrDefault();
            var personalDetail = _context.PersonalDetails.Where(x => x.LeadId == leadid && !x.IsDeleted && x.IsActive).FirstOrDefault();
            if (blacksoilupdate != null && personalDetail != null && !string.IsNullOrEmpty(personalDetail.SelfieImageUrl))
            {
                #region logic to get credentials
                if (isTest)
                {
                    api = new LeadNBFCSubActivityDTO
                    {
                        TAPIKey = "shopkirana@saraloan.in",
                        TAPISecretKey = "shopkirana@saraloan.in",
                        APIUrl = "https://stag.saraloan.in/api/v1/core/businesses/{{BUSINESS_ID}}/send_to_los/",
                        Sequence = 2,
                    };
                }
                else
                {

                    if (api != null)
                    {
                        if (api.Status == LeadNBFCApiConstants.Completed || api.Status == LeadNBFCApiConstants.CompletedWithError)
                        {
                            return new CreateLeadNBFCResponse
                            {
                                IsSuccess = true,
                                Message = "api already completed/completed with error"
                            };
                        }
                    }
                    else
                    {
                        return new CreateLeadNBFCResponse
                        {
                            IsSuccess = true,
                            Message = "not found any api configuration for same"
                        };
                    }
                }
                _leadNBFCSubActivityManager.UpdateSubActivityStatus(api.LeadNBFCSubActivityId, LeadNBFCSubActivityConstants.Inprogress);

                _leadNBFCSubActivityManager.UpdateStatus(api.LeadNBFCApiId, LeadNBFCApiConstants.Inprogress);
                #endregion
                api.APIUrl = api.APIUrl.Replace("{{PERSON_ID}}", blacksoilupdate.PersonId.ToString());


                BlackSoilNBFCHelper helper = new BlackSoilNBFCHelper();

                bool Issuccess = false;
                var response = await helper.BlackSoilSelfiePost(api.APIUrl, personalDetail.SelfieImageUrl, api.TAPIKey, api.TAPISecretKey, api.LeadNBFCApiId, leadid);
                if (response != null && response.IsSuccess)
                {
                    Issuccess = true;
                }
                _context.BlackSoilCommonAPIRequestResponses.Add(response);
                _context.SaveChanges();

                string status = LeadNBFCApiConstants.Completed;
                if (!Issuccess)
                {
                    status = LeadNBFCApiConstants.Error;
                }
                _leadNBFCSubActivityManager.UpdateStatus(api.LeadNBFCApiId, status, response.Id);
                await _leadCommonRequestResponseManager.SaveLeadCommonRequestResponse(new LeadCommonRequestInput { CommonRequestResponseId = response.Id, LeadId = leadid });
                return new CreateLeadNBFCResponse
                {
                    IsSuccess = Issuccess,
                    Message = "Error from third api call"
                };
            }
            else
            {
                return new CreateLeadNBFCResponse
                {
                    IsSuccess = false,
                    Message = "Configuration not found."
                };
            }
        }

        private async Task<ICreateLeadNBFCResponse> GetCreditLine(LeadNBFCSubActivityDTO api, long leadid, long nbfccompanyid, bool isTest = false)
        {
            ICreateLeadNBFCResponse createLeadResponse = null;
            var blacksoilupdate = await _context.BlackSoilUpdates.Where(x => x.LeadId == leadid && !x.IsDeleted && x.IsActive).FirstOrDefaultAsync();

            if (api != null)
            {
                if (api.Status == LeadNBFCApiConstants.Completed || api.Status == LeadNBFCApiConstants.CompletedWithError)
                {
                    return new CreateLeadNBFCResponse
                    {
                        IsSuccess = true,
                        Message = "api already completed/completed with error"
                    };
                }
            }
            else
            {
                return new CreateLeadNBFCResponse
                {
                    IsSuccess = true,
                    Message = "not found any api configuration for same"
                };
            }
            _leadNBFCSubActivityManager.UpdateStatus(api.LeadNBFCApiId, LeadNBFCApiConstants.Inprogress);

            BlackSoilNBFCHelper helper = new BlackSoilNBFCHelper();
            api.APIUrl = api.APIUrl.Replace("{{BUSINESS_ID}}", blacksoilupdate.BusinessId.ToString());

            var response = await helper.GetCreditLine(api.APIUrl, api.TAPIKey, api.TAPISecretKey, api.LeadNBFCApiId, leadid);
            _context.BlackSoilCommonAPIRequestResponses.Add(response);
            _context.SaveChanges();


            if (response.IsSuccess)
            {
                BlackSoilCreditLine creditLine = JsonConvert.DeserializeObject<BlackSoilCreditLine>(response.Response);
                var cl = creditLine.results.First();

                if (cl.initiated_date == null)
                {
                    //LineApproved Webhook is mandatory to get Credit limit api (by Client)
                    bool IsLineApprovedWebhookRecieved = await _context.BlackSoilWebhookResponses.AnyAsync(x => x.LeadId == leadid && x.IsActive && (x.eventName == BlackSoilWebhookConstant.LineApproved.ToString() || x.eventName == BlackSoilWebhookConstant.LineInitiate.ToString()));
                    if (!IsLineApprovedWebhookRecieved)
                    {
                        createLeadResponse = new CreateLeadNBFCResponse
                        {
                            IsSuccess = false,
                            Message = "This lead has been posted successfully, but the customer did not receive any offer from the lender."
                        };
                        return createLeadResponse;
                    }
                }

                if (creditLine != null && creditLine.results != null && creditLine.results.Any())
                {
                    string status = LeadNBFCApiConstants.Completed;

                    double amount;
                    bool isSuccessDouble = double.TryParse(cl.amount, out amount);
                    if (isSuccessDouble && amount > 0)
                    {
                        var leadoffer = await _context.LeadOffers.Where(x => x.IsActive && x.LeadId == leadid && x.NBFCCompanyId == nbfccompanyid && !x.IsDeleted).FirstOrDefaultAsync();
                        if (leadoffer != null)
                        {
                            leadoffer.Status = cl.initiated_date != null ? LeadOfferConstant.OfferGenerated : LeadOfferConstant.AwaitingInitiated;
                            leadoffer.CreditLimit = amount;
                            _context.SaveChanges();
                            _leadNBFCSubActivityManager.UpdateSubActivityStatus(api.LeadNBFCSubActivityId, LeadNBFCSubActivityConstants.Completed);

                            //------------------S : Make log---------------------
                            #region Make History
                            string doctypeSendToLosComplete = "SendToLosCompleted";

                            var resultHistory = await _leadHistoryManager.GetLeadHistroy(leadid, doctypeSendToLosComplete);
                            LeadUpdateHistoryEvent histroyEvent = new LeadUpdateHistoryEvent
                            {
                                LeadId = leadid,
                                UserID = resultHistory.UserId,
                                UserName = "",
                                EventName = doctypeSendToLosComplete,//context.Message.KYCMasterCode, //result.EntityIDofKYCMaster.ToString(),
                                Narretion = resultHistory.Narretion,
                                NarretionHTML = resultHistory.NarretionHTML,
                                CreatedTimeStamp = resultHistory.CreatedTimeStamp
                            };
                            await _massTransitService.Publish(histroyEvent);
                            #endregion
                            //------------------E : Make log---------------------

                            _leadNBFCSubActivityManager.UpdateStatus(api.LeadNBFCApiId, status, response.Id);
                        }
                        createLeadResponse = new CreateLeadNBFCResponse
                        {
                            IsSuccess = true,
                            Message = "Credit Line updated successfully"
                        };
                    }
                    else
                    {
                        createLeadResponse = new CreateLeadNBFCResponse
                        {
                            IsSuccess = false,
                            Message = "This lead has been posted successfully, but the customer did not receive any offer from the lender"
                        };
                    }

                }
                else
                {
                    createLeadResponse = new CreateLeadNBFCResponse
                    {
                        IsSuccess = false,
                        Message = "response is false from nbfc api call"
                    };
                }


            }
            else
            {
                createLeadResponse = new CreateLeadNBFCResponse
                {
                    IsSuccess = false,
                    Message = "response is false from nbfc api call"
                };
            }

            await _leadCommonRequestResponseManager.SaveLeadCommonRequestResponse(new LeadCommonRequestInput { CommonRequestResponseId = response.Id, LeadId = leadid });

            return createLeadResponse;

        }

        private async Task<ICreateLeadNBFCResponse> SendToLos(long leadid, long nbfccompanyid, bool isTest = false)
        {
            ICreateLeadNBFCResponse response = null;
            string subactivityCode = SubActivityConstants.SendToLos;
            var apiList = await _leadNBFCSubActivityManager.GetLeadNBFCSubActivity(new LeadNBFCSubActivityRequest
            {
                Code = subactivityCode,
                LeadId = leadid,
                CompanyIdentificationCode = CompanyIdentificationCodeConstants.BlackSoil
            });

            if (apiList != null && apiList.Any())
            {
                bool isSuccess = true;
                foreach (var api in apiList)
                {
                    if (isSuccess)
                    {
                        switch (api.Code)
                        {
                            case CompanyApiConstants.BlackSoilEmailPost:
                                response = await BlackSoilEmailPost(api, leadid, nbfccompanyid, isTest);
                                isSuccess = response.IsSuccess;
                                break;
                            case CompanyApiConstants.BlackSoilSelfiePost:
                                response = await BlackSoilSelfiePost(api, leadid, nbfccompanyid, isTest);
                                isSuccess = response.IsSuccess;
                                break;
                            case CompanyApiConstants.CreateBusinessPurchaseInvoice:
                                response = await CreateBusinessPurchaseInvoice(api, leadid, nbfccompanyid, isTest);
                                isSuccess = response.IsSuccess;
                                break;
                            case CompanyApiConstants.CreateBusinessDocument:
                                response = await CreateBusinessDocument(api, leadid, nbfccompanyid, isTest);
                                isSuccess = response.IsSuccess;
                                break;
                            case CompanyApiConstants.BlackSoilSendToLos:
                                response = await SendToLosInner(api, leadid, nbfccompanyid, isTest);
                                isSuccess = response.IsSuccess;
                                break;
                            case CompanyApiConstants.BlackSoilGetCreditLine:
                                response = await GetCreditLine(api, leadid, nbfccompanyid, isTest);
                                isSuccess = response.IsSuccess;
                                break;
                            case CompanyApiConstants.BlackSoilCreateBank:
                                response = await CreateBank(api, leadid, nbfccompanyid);
                                isSuccess = response.IsSuccess;
                                break;
                        }
                    }
                }
            }
            return response;
        }
        #endregion generate offer private methods


        #region PrpareAgreement private method
        private async Task<ICreateLeadNBFCResponse> GenerateAgreement(long leadid, long nbfcCompanyId)
        {
            ICreateLeadNBFCResponse response = null;
            string subactivityCode = SubActivityConstants.PrepareAgreement;
            var apiList = await _leadNBFCSubActivityManager.GetLeadNBFCSubActivity(new LeadNBFCSubActivityRequest
            {
                Code = subactivityCode,
                LeadId = leadid,
                CompanyIdentificationCode = CompanyIdentificationCodeConstants.BlackSoil
            });

            if (apiList != null && apiList.Any())
            {
                bool isSuccess = true;
                foreach (var api in apiList)
                {
                    if (isSuccess)
                    {
                        switch (api.Code)
                        {
                            case CompanyApiConstants.BlackSoilGenerateDocs:
                                response = await GenerateDocs(api, leadid, nbfcCompanyId);
                                isSuccess = response.IsSuccess;
                                break;
                            case CompanyApiConstants.BlackSoilGetLoanAgreement:
                                response = await GetLoanAgreement(api, leadid, nbfcCompanyId);
                                isSuccess = response.IsSuccess;
                                break;
                            case CompanyApiConstants.BlackSoilGetLoanSanctionLetter:
                                response = await GetLoanSanctionLetter(api, leadid, nbfcCompanyId);
                                isSuccess = response.IsSuccess;
                                break;

                            case CompanyApiConstants.BlackSoilAttachStamps:
                                response = await AttachStamps(api, leadid, nbfcCompanyId);
                                isSuccess = response.IsSuccess;
                                break;
                            case CompanyApiConstants.BlackSoilEsignStamps:
                                response = await PostEsignStamps(api, leadid, nbfcCompanyId);
                                isSuccess = response.IsSuccess;
                                break;
                            case CompanyApiConstants.BlackSoilUpdateDocStatus:
                                response = await UpdateDocStatus(api, leadid, nbfcCompanyId);
                                isSuccess = response.IsSuccess;
                                break;
                        }
                    }
                }
            }
            return response;
        }


        private async Task<ICreateLeadNBFCResponse> GetLoanAgreement(LeadNBFCSubActivityDTO api, long leadid, long nbfccompanyid, bool isTest = false)
        {

            CreateLeadNBFCResponse result = null;

            var blacksoilupdate = _context.BlackSoilUpdates.Where(x => x.LeadId == leadid && !x.IsDeleted && x.IsActive).FirstOrDefault();

            if (blacksoilupdate != null)
            {
                if (!blacksoilupdate.ApplicationId.HasValue || blacksoilupdate.ApplicationId.Value == 0)
                {
                    return new CreateLeadNBFCResponse
                    {
                        IsSuccess = false,
                        Message = "ApplicationId not found in our database"
                    };
                }
                #region logic to get credentials
                if (isTest)
                {
                    api = new LeadNBFCSubActivityDTO
                    {
                        TAPIKey = "shopkirana@saraloan.in",
                        TAPISecretKey = "shopkirana@saraloan.in",
                        APIUrl = "https://stag.saraloan.in/api/v1/core/businesses/{{BUSINESS_ID}}/send_to_los/",
                        Sequence = 2,
                    };
                }
                else
                {


                    if (api != null)
                    {
                        if (api.Status == LeadNBFCApiConstants.Completed || api.Status == LeadNBFCApiConstants.CompletedWithError)
                        {
                            return new CreateLeadNBFCResponse
                            {
                                IsSuccess = true,
                                Message = "api already completed/completed with error"
                            };
                        }
                    }
                    else
                    {
                        return new CreateLeadNBFCResponse
                        {
                            IsSuccess = true,
                            Message = "not found any api configuration for same"
                        };
                    }
                }


                _leadNBFCSubActivityManager.UpdateSubActivityStatus(api.LeadNBFCSubActivityId, LeadNBFCSubActivityConstants.Inprogress);

                _leadNBFCSubActivityManager.UpdateStatus(api.LeadNBFCApiId, LeadNBFCApiConstants.Inprogress);
                #endregion
                api.APIUrl = api.APIUrl.Replace("{{APPLICATION_ID}}", blacksoilupdate.ApplicationId.ToString());


                BlackSoilNBFCHelper helper = new BlackSoilNBFCHelper();

                var response = await helper.GenerateAgreement(api.APIUrl, api.TAPIKey, api.TAPISecretKey, api.LeadNBFCApiId, leadid);
                _context.BlackSoilCommonAPIRequestResponses.Add(response);
                _context.SaveChanges();
                string status = LeadNBFCApiConstants.Completed;
                if (response.IsSuccess)
                {

                    response.IsSuccess = false;
                    string json = response.Response;

                    var loanAgreement = JsonConvert.DeserializeObject<BlackSoilGetLoanAgreementDc>(json);

                    if (loanAgreement != null && loanAgreement.count > 0 && loanAgreement.results != null && loanAgreement.results.Any(y => y.doc_name == "loan_agreement"))
                    {
                        response.IsSuccess = true;
                        long sectionLetterId = 0;
                        long loanAgreementId = loanAgreement.results.Where(x => x.doc_name == "loan_agreement").FirstOrDefault().id;
                        await _blackSoilUpdateManager.UpdateDocId(blacksoilupdate.Id, loanAgreementId, sectionLetterId);

                    }
                }
                else
                {
                    status = LeadNBFCApiConstants.Error;
                }
                _leadNBFCSubActivityManager.UpdateStatus(api.LeadNBFCApiId, status, response.Id);
                await _leadCommonRequestResponseManager.SaveLeadCommonRequestResponse(new LeadCommonRequestInput { CommonRequestResponseId = response.Id, LeadId = leadid });
                return new CreateLeadNBFCResponse
                {
                    IsSuccess = response.IsSuccess,
                    Message = ""
                };
            }
            else
            {
                return new CreateLeadNBFCResponse
                {
                    IsSuccess = false,
                    Message = "businessId not found in BlackSoilUpdates"
                };
            }


            return new CreateLeadNBFCResponse
            {
                IsSuccess = true,
                Message = "BlackSoilLeadNBFCService"
            };
        }
        private async Task<ICreateLeadNBFCResponse> GetLoanSanctionLetter(LeadNBFCSubActivityDTO api, long leadid, long nbfccompanyid, bool isTest = false)
        {

            CreateLeadNBFCResponse result = null;

            var blacksoilupdate = _context.BlackSoilUpdates.Where(x => x.LeadId == leadid && !x.IsDeleted && x.IsActive).FirstOrDefault();

            if (blacksoilupdate != null)
            {
                if (!blacksoilupdate.ApplicationId.HasValue || blacksoilupdate.ApplicationId.Value == 0)
                {
                    return new CreateLeadNBFCResponse
                    {
                        IsSuccess = false,
                        Message = "ApplicationId not found in our database"
                    };
                }
                #region logic to get credentials
                if (isTest)
                {
                    api = new LeadNBFCSubActivityDTO
                    {
                        TAPIKey = "shopkirana@saraloan.in",
                        TAPISecretKey = "shopkirana@saraloan.in",
                        APIUrl = "https://stag.saraloan.in/api/v1/core/businesses/{{BUSINESS_ID}}/send_to_los/",
                        Sequence = 2,
                    };
                }
                else
                {


                    if (api != null)
                    {
                        if (api.Status == LeadNBFCApiConstants.Completed || api.Status == LeadNBFCApiConstants.CompletedWithError)
                        {
                            return new CreateLeadNBFCResponse
                            {
                                IsSuccess = true,
                                Message = "api already completed/completed with error"
                            };
                        }
                    }
                    else
                    {
                        return new CreateLeadNBFCResponse
                        {
                            IsSuccess = true,
                            Message = "not found any api configuration for same"
                        };
                    }
                }


                _leadNBFCSubActivityManager.UpdateSubActivityStatus(api.LeadNBFCSubActivityId, LeadNBFCSubActivityConstants.Inprogress);

                _leadNBFCSubActivityManager.UpdateStatus(api.LeadNBFCApiId, LeadNBFCApiConstants.Inprogress);
                #endregion
                api.APIUrl = api.APIUrl.Replace("{{APPLICATION_ID}}", blacksoilupdate.ApplicationId.ToString());


                BlackSoilNBFCHelper helper = new BlackSoilNBFCHelper();

                var response = await helper.GenerateAgreement(api.APIUrl, api.TAPIKey, api.TAPISecretKey, api.LeadNBFCApiId, leadid);
                _context.BlackSoilCommonAPIRequestResponses.Add(response);
                _context.SaveChanges();
                string status = LeadNBFCApiConstants.Completed;
                if (response.IsSuccess)
                {

                    response.IsSuccess = false;
                    string json = response.Response;

                    var loanAgreement = JsonConvert.DeserializeObject<BlackSoilGetLoanAgreementDc>(json);

                    if (loanAgreement != null && loanAgreement.count > 0 && loanAgreement.results != null && loanAgreement.results.Any(y => y.doc_name == "sanction_letter"))
                    {
                        response.IsSuccess = true;
                        long sectionLetterId = loanAgreement.results.Where(x => x.doc_name == "sanction_letter").FirstOrDefault().id;
                        long loanAgreementId = blacksoilupdate.AgreementDocId ?? 0;
                        await _blackSoilUpdateManager.UpdateDocId(blacksoilupdate.Id, loanAgreementId, sectionLetterId);

                    }

                }
                else
                {
                    status = LeadNBFCApiConstants.Error;
                }
                _leadNBFCSubActivityManager.UpdateStatus(api.LeadNBFCApiId, status, response.Id);
                await _leadCommonRequestResponseManager.SaveLeadCommonRequestResponse(new LeadCommonRequestInput { CommonRequestResponseId = response.Id, LeadId = leadid });
                return new CreateLeadNBFCResponse
                {
                    IsSuccess = response.IsSuccess,
                    Message = ""
                };
            }
            else
            {
                return new CreateLeadNBFCResponse
                {
                    IsSuccess = false,
                    Message = "businessId not found in BlackSoilUpdates"
                };
            }


            return new CreateLeadNBFCResponse
            {
                IsSuccess = true,
                Message = "BlackSoilLeadNBFCService"
            };
        }


        private async Task<ICreateLeadNBFCResponse> GenerateDocs(LeadNBFCSubActivityDTO api, long leadid, long nbfccompanyid, bool isTest = false)
        {

            CreateLeadNBFCResponse result = null;

            var blacksoilupdate = _context.BlackSoilUpdates.Where(x => x.LeadId == leadid && !x.IsDeleted && x.IsActive).FirstOrDefault();

            if (blacksoilupdate != null)
            {
                if (!blacksoilupdate.ApplicationId.HasValue || blacksoilupdate.ApplicationId.Value == 0)
                {
                    return new CreateLeadNBFCResponse
                    {
                        IsSuccess = false,
                        Message = "ApplicationId not found in our database"
                    };
                }



                #region logic to get credentials
                if (isTest)
                {
                    api = new LeadNBFCSubActivityDTO
                    {
                        TAPIKey = "shopkirana@saraloan.in",
                        TAPISecretKey = "shopkirana@saraloan.in",
                        APIUrl = "https://stag.saraloan.in/api/v1/core/businesses/{{BUSINESS_ID}}/send_to_los/",
                        Sequence = 2,
                    };
                }
                else
                {


                    if (api != null)
                    {
                        if (api.Status == LeadNBFCApiConstants.Completed || api.Status == LeadNBFCApiConstants.CompletedWithError)
                        {
                            return new CreateLeadNBFCResponse
                            {
                                IsSuccess = true,
                                Message = "api already completed/completed with error"
                            };
                        }
                    }
                    else
                    {
                        return new CreateLeadNBFCResponse
                        {
                            IsSuccess = true,
                            Message = "not found any api configuration for same"
                        };
                    }
                }


                _leadNBFCSubActivityManager.UpdateSubActivityStatus(api.LeadNBFCSubActivityId, LeadNBFCSubActivityConstants.Inprogress);

                _leadNBFCSubActivityManager.UpdateStatus(api.LeadNBFCApiId, LeadNBFCApiConstants.Inprogress);
                #endregion
                api.APIUrl = api.APIUrl.Replace("{{APPLICATION_ID}}", blacksoilupdate.ApplicationId.ToString());


                BlackSoilNBFCHelper helper = new BlackSoilNBFCHelper();

                var response = await helper.GenerateAgreement(api.APIUrl, api.TAPIKey, api.TAPISecretKey, api.LeadNBFCApiId, leadid);
                _context.BlackSoilCommonAPIRequestResponses.Add(response);
                _context.SaveChanges();
                string status = LeadNBFCApiConstants.Completed;
                if (response.IsSuccess)
                {

                    string json = response.Response;

                    var generatedDocs = JsonConvert.DeserializeObject<List<BlackSoilGenerateInBulkDTO>>(json);

                    if (generatedDocs != null && generatedDocs.Any() && generatedDocs.All(x => x.type == "success"))
                    {
                        long sectionLetterId = generatedDocs.Where(x => x.response.doc_name == "sanction_letter").FirstOrDefault().response.id.Value;
                        long loanAgreementId = generatedDocs.Where(x => x.response.doc_name == "loan_agreement").FirstOrDefault().response.id.Value;
                        await _blackSoilUpdateManager.UpdateDocId(blacksoilupdate.Id, loanAgreementId, sectionLetterId);
                    }

                }
                else
                {
                    status = LeadNBFCApiConstants.Error;

                }
                _leadNBFCSubActivityManager.UpdateStatus(api.LeadNBFCApiId, status, response.Id);
                await _leadCommonRequestResponseManager.SaveLeadCommonRequestResponse(new LeadCommonRequestInput { CommonRequestResponseId = response.Id, LeadId = leadid });
                return new CreateLeadNBFCResponse
                {
                    IsSuccess = response.IsSuccess,
                    Message = ""
                };
                //_leadNBFCSubActivityManager.UpdateSubActivityStatus(api.LeadNBFCSubActivityId, status);
            }
            else
            {
                return new CreateLeadNBFCResponse
                {
                    IsSuccess = false,
                    Message = "businessId not found in BlackSoilUpdates"
                };
            }


            return new CreateLeadNBFCResponse
            {
                IsSuccess = true,
                Message = "BlackSoilLeadNBFCService"
            };
        }

        private async Task<ICreateLeadNBFCResponse> AttachStamps(LeadNBFCSubActivityDTO api, long leadid, long nbfccompanyid, bool isTest = false)
        {

            CreateLeadNBFCResponse result = null;

            var blacksoilupdate = _context.BlackSoilUpdates.Where(x => x.LeadId == leadid && !x.IsDeleted && x.IsActive).FirstOrDefault();
            if (blacksoilupdate != null)
            {
                #region logic to get credentials
                if (isTest)
                {
                    api = new LeadNBFCSubActivityDTO
                    {
                        TAPIKey = "shopkirana@saraloan.in",
                        TAPISecretKey = "shopkirana@saraloan.in",
                        APIUrl = "https://stag.saraloan.in/api/v1/core/businesses/{{BUSINESS_ID}}/send_to_los/",
                        Sequence = 2,
                    };
                }
                else
                {


                    if (api != null)
                    {
                        if (api.Status == LeadNBFCApiConstants.Completed || api.Status == LeadNBFCApiConstants.CompletedWithError)
                        {
                            return new CreateLeadNBFCResponse
                            {
                                IsSuccess = true,
                                Message = "api already completed/completed with error"
                            };
                        }
                    }
                    else
                    {
                        return new CreateLeadNBFCResponse
                        {
                            IsSuccess = true,
                            Message = "not found any api configuration for same"
                        };
                    }
                }
                _leadNBFCSubActivityManager.UpdateStatus(api.LeadNBFCApiId, LeadNBFCApiConstants.Inprogress);
                #endregion
                api.APIUrl = api.APIUrl.Replace("{{APPLICATION_ID}}", blacksoilupdate.ApplicationId.ToString());
                api.APIUrl = api.APIUrl.Replace("{{SANCTION_DOC_ID}}", blacksoilupdate.SanctionDocId.ToString());
                api.APIUrl = api.APIUrl.Replace("{{AGREEMENT_DOC_ID}}", blacksoilupdate.AgreementDocId.ToString());


                BlackSoilNBFCHelper helper = new BlackSoilNBFCHelper();

                var response = await helper.Attachstamp(api.APIUrl, api.TAPIKey, api.TAPISecretKey, api.LeadNBFCApiId, leadid);
                _context.BlackSoilCommonAPIRequestResponses.Add(response);
                _context.SaveChanges();
                string status = LeadNBFCApiConstants.Completed;
                if (response.IsSuccess)
                {

                    string json = response.Response;

                    var generatedDocs = JsonConvert.DeserializeObject<BlackSoilAttachStampPaperDTO>(json);

                    if (generatedDocs != null)
                    {
                        long stampId = generatedDocs.id.Value;
                        await _blackSoilUpdateManager.UpdateStampId(blacksoilupdate.Id, stampId);
                    }

                }
                else
                {
                    status = LeadNBFCApiConstants.Error;

                }
                _leadNBFCSubActivityManager.UpdateStatus(api.LeadNBFCApiId, status, response.Id);
                await _leadCommonRequestResponseManager.SaveLeadCommonRequestResponse(new LeadCommonRequestInput { CommonRequestResponseId = response.Id, LeadId = leadid });

                return new CreateLeadNBFCResponse
                {
                    IsSuccess = response.IsSuccess,
                    Message = ""
                };
                //_leadNBFCSubActivityManager.UpdateSubActivityStatus(api.LeadNBFCSubActivityId, status);
            }
            else
            {
                return new CreateLeadNBFCResponse
                {
                    IsSuccess = false,
                    Message = "businessId not found in BlackSoilUpdates"
                };
            }


            return new CreateLeadNBFCResponse
            {
                IsSuccess = true,
                Message = "BlackSoilLeadNBFCService"
            };
        }

        private async Task<ICreateLeadNBFCResponse> PostEsignStamps(LeadNBFCSubActivityDTO api, long leadid, long nbfccompanyid, bool isTest = false)
        {

            CreateLeadNBFCResponse result = null;

            var blacksoilupdate = _context.BlackSoilUpdates.Where(x => x.LeadId == leadid && !x.IsDeleted && x.IsActive).FirstOrDefault();
            if (blacksoilupdate != null)
            {
                #region logic to get credentials
                if (isTest)
                {
                    api = new LeadNBFCSubActivityDTO
                    {
                        TAPIKey = "shopkirana@saraloan.in",
                        TAPISecretKey = "shopkirana@saraloan.in",
                        APIUrl = "https://stag.saraloan.in/api/v1/core/businesses/{{BUSINESS_ID}}/send_to_los/",
                        Sequence = 2,
                    };
                }
                else
                {


                    if (api != null)
                    {
                        if (api.Status == LeadNBFCApiConstants.Completed || api.Status == LeadNBFCApiConstants.CompletedWithError)
                        {
                            return new CreateLeadNBFCResponse
                            {
                                IsSuccess = true,
                                Message = "api already completed/completed with error"
                            };
                        }
                    }
                    else
                    {
                        return new CreateLeadNBFCResponse
                        {
                            IsSuccess = true,
                            Message = "not found any api configuration for same"
                        };
                    }
                }
                _leadNBFCSubActivityManager.UpdateStatus(api.LeadNBFCApiId, LeadNBFCApiConstants.Inprogress);
                #endregion


                BlackSoilNBFCHelper helper = new BlackSoilNBFCHelper();

                var response = await helper.CreateEsign(api.APIUrl, api.TAPIKey, api.TAPISecretKey, new BlackSoilEsignsDTO
                {
                    document_id = blacksoilupdate.StampId.Value.ToString(),
                    new_esign = true
                }, api.LeadNBFCApiId, leadid);

                _context.BlackSoilCommonAPIRequestResponses.Add(response);
                _context.SaveChanges();
                string status = LeadNBFCApiConstants.Completed;
                if (response.IsSuccess)
                {

                    string json = response.Response;

                    var generatedDocs = JsonConvert.DeserializeObject<BlackSoilCreateEsignDTO>(json);

                    if (generatedDocs != null)
                    {
                        long signId = generatedDocs.id.Value;
                        await _blackSoilUpdateManager.UpdateESignId(blacksoilupdate.Id, signId);
                    }

                }
                else
                {
                    status = LeadNBFCApiConstants.Error;

                }
                _leadNBFCSubActivityManager.UpdateStatus(api.LeadNBFCApiId, status, response.Id);
                await _leadCommonRequestResponseManager.SaveLeadCommonRequestResponse(new LeadCommonRequestInput { CommonRequestResponseId = response.Id, LeadId = leadid });

                return new CreateLeadNBFCResponse
                {
                    IsSuccess = response.IsSuccess,
                    Message = ""
                };

                //_leadNBFCSubActivityManager.UpdateSubActivityStatus(api.LeadNBFCSubActivityId, status);
            }
            else
            {
                return new CreateLeadNBFCResponse
                {
                    IsSuccess = false,
                    Message = "businessId not found in BlackSoilUpdates"
                };
            }


            return new CreateLeadNBFCResponse
            {
                IsSuccess = true,
                Message = "BlackSoilLeadNBFCService"
            };
        }

        private async Task<ICreateLeadNBFCResponse> UpdateDocStatus(LeadNBFCSubActivityDTO api, long leadid, long nbfccompanyid, bool isTest = false)
        {

            CreateLeadNBFCResponse result = null;

            var blacksoilupdate = _context.BlackSoilUpdates.Where(x => x.LeadId == leadid && !x.IsDeleted && x.IsActive).FirstOrDefault();
            if (blacksoilupdate != null)
            {
                #region logic to get credentials
                if (isTest)
                {
                    api = new LeadNBFCSubActivityDTO
                    {
                        TAPIKey = "shopkirana@saraloan.in",
                        TAPISecretKey = "shopkirana@saraloan.in",
                        APIUrl = "https://stag.saraloan.in/api/v1/core/businesses/{{BUSINESS_ID}}/send_to_los/",
                        Sequence = 2,
                    };
                }
                else
                {


                    if (api != null)
                    {
                        if (api.Status == LeadNBFCApiConstants.Completed || api.Status == LeadNBFCApiConstants.CompletedWithError)
                        {
                            return new CreateLeadNBFCResponse
                            {
                                IsSuccess = true,
                                Message = "api already completed/completed with error"
                            };
                        }
                    }
                    else
                    {
                        return new CreateLeadNBFCResponse
                        {
                            IsSuccess = true,
                            Message = "not found any api configuration for same"
                        };
                    }
                }
                _leadNBFCSubActivityManager.UpdateStatus(api.LeadNBFCApiId, LeadNBFCApiConstants.Inprogress);
                #endregion

                api.APIUrl = api.APIUrl.Replace("{{ESIGN _ID}}", blacksoilupdate.ESingingId.ToString());

                BlackSoilNBFCHelper helper = new BlackSoilNBFCHelper();

                var response = await helper.SignerStatus(api.APIUrl, api.TAPIKey, api.TAPISecretKey, api.LeadNBFCApiId, leadid);

                _context.BlackSoilCommonAPIRequestResponses.Add(response);
                _context.SaveChanges();
                string status = LeadNBFCApiConstants.Completed;
                if (response.IsSuccess)
                {


                    string json = response.Response;

                    var generatedDocs = JsonConvert.DeserializeObject<BlackSoilSignersDTO>(json);

                    if (generatedDocs != null && generatedDocs.signers != null && generatedDocs.signers.Any())
                    {
                        string signURL = generatedDocs.signers.FirstOrDefault(x => x.type == "Applicant").sign_url;

                        await _blackSoilUpdateManager.UpdateSingingUrl(blacksoilupdate.Id, signURL);
                        _leadNBFCSubActivityManager.UpdateSubActivityStatus(api.LeadNBFCSubActivityId, LeadNBFCSubActivityConstants.Completed);




                        var leadActivity = await _context.LeadActivityMasterProgresses.Where(x => x.LeadMasterId == leadid
                                       && !x.IsCompleted && x.IsActive && !x.IsDeleted && x.SubActivityMasterName == SubActivityConstants.PrepareAgreement).FirstOrDefaultAsync();
                        if (leadActivity != null)
                        {
                            leadActivity.IsCompleted = true;
                            leadActivity.IsApproved = 1;
                            _context.Entry(leadActivity).State = EntityState.Modified;
                            await _context.SaveChangesAsync();


                            //------------------S : Make log---------------------
                            #region Make History
                            var resultHistory = await _leadHistoryManager.GetLeadHistroy(leadid, "UpdateDocStatus_PrepareBlackSoil");
                            LeadUpdateHistoryEvent histroyEvent = new LeadUpdateHistoryEvent
                            {
                                LeadId = leadid,
                                UserID = resultHistory.UserId,
                                UserName = "",
                                EventName = "Agreement Prepare-BlackSoil",//context.Message.KYCMasterCode, //result.EntityIDofKYCMaster.ToString(),
                                Narretion = resultHistory.Narretion,
                                NarretionHTML = resultHistory.NarretionHTML,
                                CreatedTimeStamp = resultHistory.CreatedTimeStamp
                            };
                            await _massTransitService.Publish(histroyEvent);
                            #endregion
                            //------------------E : Make log---------------------

                        }
                    }

                }
                else
                {
                    status = LeadNBFCApiConstants.Error;

                }
                _leadNBFCSubActivityManager.UpdateStatus(api.LeadNBFCApiId, status, response.Id);
                await _leadCommonRequestResponseManager.SaveLeadCommonRequestResponse(new LeadCommonRequestInput { CommonRequestResponseId = response.Id, LeadId = leadid });


                return new CreateLeadNBFCResponse
                {
                    IsSuccess = response.IsSuccess,
                    Message = ""
                };

                //_leadNBFCSubActivityManager.UpdateSubActivityStatus(api.LeadNBFCSubActivityId, status);
            }
            else
            {
                return new CreateLeadNBFCResponse
                {
                    IsSuccess = false,
                    Message = "businessId not found in BlackSoilUpdates"
                };
            }


            return new CreateLeadNBFCResponse
            {
                IsSuccess = true,
                Message = "BlackSoilLeadNBFCService"
            };
        }


        #endregion

        public async Task<ICreateLeadNBFCResponse> AgreementEsign(long leadid, long nbfcCompanyId)
        {
            CreateLeadNBFCResponse result = null;
            LeadNBFCSubActivityDTO api = null;
            var blacksoilupdate = _context.BlackSoilUpdates.Where(x => x.LeadId == leadid && !x.IsDeleted && x.IsActive).FirstOrDefault();
            if (blacksoilupdate != null)
            {
                #region logic to get credentials

                string subactivityCode = SubActivityConstants.AgreementEsign;
                var list = await _leadNBFCSubActivityManager.GetLeadNBFCSubActivity(new LeadNBFCSubActivityRequest
                {
                    Code = subactivityCode,
                    LeadId = leadid,
                    CompanyIdentificationCode = CompanyIdentificationCodeConstants.BlackSoil
                });

                if (list != null && list.Count > 0)
                {
                    api = list.FirstOrDefault();
                    if (api.Status == LeadNBFCApiConstants.Completed || api.Status == LeadNBFCApiConstants.CompletedWithError)
                    {
                        return new CreateLeadNBFCResponse
                        {
                            IsSuccess = true,
                            Message = "api already completed/completed with error"
                        };
                    }
                }
                else
                {
                    return new CreateLeadNBFCResponse
                    {
                        IsSuccess = true,
                        Message = "not found any api configuration for same"
                    };
                }


                _leadNBFCSubActivityManager.UpdateStatus(api.LeadNBFCApiId, LeadNBFCApiConstants.Inprogress);
                #endregion

                api.APIUrl = api.APIUrl.Replace("{{ESIGN _ID}}", blacksoilupdate.ESingingId.ToString());

                BlackSoilNBFCHelper helper = new BlackSoilNBFCHelper();

                var response = await helper.SignerStatus(api.APIUrl, api.TAPIKey, api.TAPISecretKey, api.LeadNBFCApiId, leadid);

                _context.BlackSoilCommonAPIRequestResponses.Add(response);
                _context.SaveChanges();
                string status = LeadNBFCApiConstants.Error;
                if (response.IsSuccess)
                {


                    string json = response.Response;

                    var generatedDocs = JsonConvert.DeserializeObject<BlackSoilSignersDTO>(json);

                    if (generatedDocs != null && generatedDocs.signers != null && generatedDocs.signers.Any())
                    {
                        string signerStatus = generatedDocs.signers.FirstOrDefault(x => x.type == "Applicant").status;

                        if (signerStatus == "signed")
                        {
                            var lead = await _context.Leads.FirstOrDefaultAsync(x => x.Id == leadid);
                            lead.Status = LeadStatusEnum.LoanAvailed.ToString();
                            _context.Entry(lead).State = EntityState.Modified;


                            status = LeadNBFCApiConstants.Completed;
                            _leadNBFCSubActivityManager.UpdateSubActivityStatus(api.LeadNBFCSubActivityId, LeadNBFCSubActivityConstants.Completed);
                            var leadActivity = await _context.LeadActivityMasterProgresses.Where(x => x.LeadMasterId == leadid
                                           && !x.IsCompleted && x.IsActive && !x.IsDeleted && x.SubActivityMasterName == SubActivityConstants.AgreementEsign).FirstOrDefaultAsync();
                            if (leadActivity != null)
                            {
                                leadActivity.IsCompleted = true;
                                leadActivity.IsApproved = 1;
                                _context.Entry(leadActivity).State = EntityState.Modified;
                                _context.SaveChanges();

                                //------------------S : Make log---------------------
                                #region Make History
                                var resultHistory = await _leadHistoryManager.GetLeadHistroy(leadid, "AgreementEsign_BlackSoil");
                                LeadUpdateHistoryEvent histroyEvent = new LeadUpdateHistoryEvent
                                {
                                    LeadId = leadid,
                                    UserID = resultHistory.UserId,
                                    UserName = "",
                                    EventName = "Agreement Esign-BlackSoil",//context.Message.KYCMasterCode, //result.EntityIDofKYCMaster.ToString(),
                                    Narretion = resultHistory.Narretion,
                                    NarretionHTML = resultHistory.NarretionHTML,
                                    CreatedTimeStamp = resultHistory.CreatedTimeStamp
                                };
                                await _massTransitService.Publish(histroyEvent);
                                #endregion
                                //------------------E : Make log---------------------

                            }
                        }



                    }

                }
                else
                {
                    status = LeadNBFCApiConstants.Error;

                }
                _leadNBFCSubActivityManager.UpdateStatus(api.LeadNBFCApiId, status, response.Id);
                await _leadCommonRequestResponseManager.SaveLeadCommonRequestResponse(new LeadCommonRequestInput { CommonRequestResponseId = response.Id, LeadId = leadid });

                _leadNBFCSubActivityManager.UpdateSubActivityStatus(api.LeadNBFCSubActivityId, status);
            }
            else
            {
                return new CreateLeadNBFCResponse
                {
                    IsSuccess = false,
                    Message = "businessId not found in BlackSoilUpdates"
                };
            }


            return new CreateLeadNBFCResponse
            {
                IsSuccess = true,
                Message = "BlackSoilLeadNBFCService"
            };
        }

        public async Task<ICreateLeadNBFCResponse> GenerateOffer(long leadid, long nbfcCompanyId)
        {
            ICreateLeadNBFCResponse response = null;
            var subactivity = _leadNBFCSubActivityManager.GetSubactivityData(leadid, nbfcCompanyId, ActivityConstants.GenerateOffer);
            if (subactivity != null && subactivity.Any())
            {
                bool isSuccess = true;
                foreach (var item in subactivity)
                {
                    if (isSuccess)
                    {
                        switch (item.Code)
                        {
                            case SubActivityConstants.CreateLead:
                                response = await CreateLead(leadid);
                                isSuccess = response.IsSuccess;
                                break;
                            case SubActivityConstants.SendToLos:
                                response = await SendToLos(leadid, item.NBFCCompanyId);
                                break;

                        }

                    }

                }
            }

            return response;
        }

        public async Task<ICreateLeadNBFCResponse> PrpareAgreement(long leadid, long nbfcCompanyId)
        {
            ICreateLeadNBFCResponse response = null;
            var subactivity = _leadNBFCSubActivityManager.GetSubactivityData(leadid, nbfcCompanyId, ActivityConstants.Agreement);
            if (subactivity != null && subactivity.Any())
            {
                bool isSuccess = true;
                foreach (var item in subactivity)
                {
                    if (isSuccess)
                    {
                        switch (item.Code)
                        {
                            case SubActivityConstants.PrepareAgreement:
                                response = await GenerateAgreement(leadid, nbfcCompanyId);
                                isSuccess = response.IsSuccess;
                                break;
                            case SubActivityConstants.AgreementEsign:
                                response = await AgreementEsign(leadid, item.NBFCCompanyId);
                                break;

                        }

                    }

                }
            }

            return response;
        }

        public async Task<ICreateLeadNBFCResponse> CreateBank(LeadNBFCSubActivityDTO api, long leadid, long nbfcCompanyId)
        {
            BlackSoilNBFCHelper blackSoilNBFCHelper = new BlackSoilNBFCHelper();
            ICreateLeadNBFCResponse createLeadResponse = null;
            var BankDetail = _context.LeadBankDetails.Where(x => x.LeadId == leadid && x.Type == BankTypeConstant.Borrower && x.IsActive && !x.IsDeleted).FirstOrDefault();
            var BussinessId = _context.BlackSoilUpdates.Where(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted).FirstOrDefault();

            if (api != null)
            {
                if (api.Status == LeadNBFCApiConstants.Completed || api.Status == LeadNBFCApiConstants.CompletedWithError)
                {
                    return new CreateLeadNBFCResponse
                    {
                        IsSuccess = true,
                        Message = "api already completed/completed with error"
                    };
                }
            }
            else
            {
                return new CreateLeadNBFCResponse
                {
                    IsSuccess = true,
                    Message = "not found any api configuration for same"
                };
            }
            _leadNBFCSubActivityManager.UpdateStatus(api.LeadNBFCApiId, LeadNBFCApiConstants.Inprogress);
            api.APIUrl = api.APIUrl.Replace("{{BUSINESS_ID}}", BussinessId.BusinessId.ToString());

            if (BankDetail != null)
            {
                BlackSoilCreateBankInput blackSoilCreateBankInput = new BlackSoilCreateBankInput
                {
                    account_holder_name = BankDetail.AccountHolderName,
                    account_number = BankDetail.AccountNumber,
                    account_type = BankDetail.AccountType,
                    business_id = BussinessId.BusinessId.ToString(),
                    ifsc = BankDetail.IFSCCode,
                    is_for_disbursement = false,
                    is_for_nach = true,
                    password = ""
                };


                var bankDetailApi = await _context.LeadNBFCApis.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.BlackSoilGetBankDetailByIFSCCode && x.IsActive && !x.IsDeleted);
                if (bankDetailApi != null)
                {
                    var APIUrl = bankDetailApi.APIUrl;
                    var Bankresult = await blackSoilNBFCHelper.GetBankDetailByIFSCCode(APIUrl.Replace("{{IFSC_CODE}}", blackSoilCreateBankInput.ifsc), bankDetailApi.TAPIKey, bankDetailApi.TAPISecretKey, leadid);
                    if (Bankresult != null && Bankresult.IsSuccess)
                    {
                        var BankInfo = JsonConvert.DeserializeObject<BankInfoDc>(Bankresult.Response);

                        blackSoilCreateBankInput.bank_name = BankInfo != null ? BankInfo.BANK : blackSoilCreateBankInput.bank_name;
                    }
                    Bankresult.LeadId = leadid;
                    Bankresult.LeadNBFCApiId = bankDetailApi.Id;
                    _context.BlackSoilCommonAPIRequestResponses.Add(Bankresult);
                    _context.SaveChanges();
                }
                var response = await blackSoilNBFCHelper.CreateBank(blackSoilCreateBankInput, api.APIUrl, api.TAPIKey, api.TAPISecretKey, api.LeadNBFCApiId, leadid);
                _context.BlackSoilCommonAPIRequestResponses.Add(response);
                _context.SaveChanges();
                createLeadResponse = new CreateLeadNBFCResponse
                {
                    IsSuccess = false,
                    Message = "Already Exists!!"
                };

                if (response.IsSuccess)
                {
                    string status = LeadNBFCApiConstants.Completed;
                    _leadNBFCSubActivityManager.UpdateStatus(api.LeadNBFCApiId, status, response.Id);
                    createLeadResponse = new CreateLeadNBFCResponse
                    {
                        IsSuccess = true,
                        Message = "Completed!!"
                    };
                }
                else
                {
                    string status = LeadNBFCApiConstants.Error;
                    _leadNBFCSubActivityManager.UpdateStatus(api.LeadNBFCApiId, status, response.Id);
                    createLeadResponse = new CreateLeadNBFCResponse
                    {
                        IsSuccess = true,
                        Message = "Error!!"
                    };
                }
            }
            else
            {
                createLeadResponse = new CreateLeadNBFCResponse
                {
                    IsSuccess = false,
                    Message = "Bank detail not found!!"
                };
            }
            return createLeadResponse;
        }


        public async Task<ICreateLeadNBFCResponse> PanUpdate(long leadid, long nbfcCompanyId)
        {
            BlackSoilNBFCHelper blackSoilNBFCHelper = new BlackSoilNBFCHelper();
            var blackSoilUpdates = _context.BlackSoilUpdates.FirstOrDefault(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);
            var personalDetail = _context.PersonalDetails.FirstOrDefault(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);
            var companyApi = _context.LeadNBFCApis.FirstOrDefault(x => x.Code == CompanyApiConstants.BlackSoilCommonApiForSecret && x.IsActive && !x.IsDeleted);
            BlackSoilPanUpdateInput input = new BlackSoilPanUpdateInput
            {
                business = blackSoilUpdates.BusinessId.ToString(),
                doc_name = "pan",
                doc_number = personalDetail.PanMaskNO,
                doc_type = "id_proof",
                file = personalDetail.PanFrontImage,
                type = "person_document",
                update_url = blackSoilUpdates.PanUpdateUrl
            };

            var response = await blackSoilNBFCHelper.PanUpdate(input, companyApi.TAPIKey, companyApi.TAPISecretKey);
            response.LeadNBFCApiId = companyApi.Id;
            response.LeadId = leadid;
            _context.BlackSoilCommonAPIRequestResponses.Add(response);
            _context.SaveChanges();
            ICreateLeadNBFCResponse panUpdateResponse = new CreateLeadNBFCResponse();


            panUpdateResponse.IsSuccess = response.IsSuccess;
            panUpdateResponse.Message = "Success";
            return panUpdateResponse;
        }
        public async Task<ICreateLeadNBFCResponse> AadhaarUpdate(long leadid, long nbfcCompanyId)
        {
            BlackSoilNBFCHelper blackSoilNBFCHelper = new BlackSoilNBFCHelper();
            var blackSoilUpdates = _context.BlackSoilUpdates.FirstOrDefault(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);
            var personalDetail = _context.PersonalDetails.FirstOrDefault(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);
            var companyApi = _context.LeadNBFCApis.FirstOrDefault(x => x.Code == CompanyApiConstants.BlackSoilCommonApiForSecret && x.IsActive && !x.IsDeleted);
            BlackSoilAadhaarUpdateInput input = new BlackSoilAadhaarUpdateInput
            {
                business = blackSoilUpdates.BusinessId.ToString(),
                doc_name = "aadhaar",
                doc_number = personalDetail.AadhaarMaskNO,
                doc_type = "id_proof",
                file = personalDetail.AadharFrontImage,
                type = "person_document",
                update_url = blackSoilUpdates.AadhaarUpdateUrl
            };

            var response = await blackSoilNBFCHelper.AadhaarUpdate(input, companyApi.TAPIKey, companyApi.TAPISecretKey);
            response.LeadId = leadid;
            _context.BlackSoilCommonAPIRequestResponses.Add(response);
            _context.SaveChanges();
            ICreateLeadNBFCResponse aadharUpdateResponse = new CreateLeadNBFCResponse();


            aadharUpdateResponse.IsSuccess = response.IsSuccess;
            aadharUpdateResponse.Message = "Success";
            return aadharUpdateResponse;
        }
        public async Task<ICreateLeadNBFCResponse> PersonUpdate(long leadid, long nbfcCompanyId)
        {
            BlackSoilNBFCHelper blackSoilNBFCHelper = new BlackSoilNBFCHelper();
            var blackSoilUpdates = _context.BlackSoilUpdates.FirstOrDefault(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);
            var personalDetail = _context.PersonalDetails.FirstOrDefault(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);
            var companyApi = _context.LeadNBFCApis.FirstOrDefault(x => x.Code == CompanyApiConstants.BlackSoilCommonApiForSecret && x.IsActive && !x.IsDeleted);

            BlackSoilPersonUpdateInput input = new BlackSoilPersonUpdateInput
            {
                update_url = blackSoilUpdates.PersonUpdateUrl,
                dob = personalDetail.DOB.ToString("yyyy-MM-dd"),
                full_name = personalDetail.PanNameOnCard,
                gender = personalDetail.Gender == "M" ? "male" : "female",
                first_name = personalDetail.FirstName,
                last_name = personalDetail.LastName != null ? personalDetail.LastName : "",
                middle_name = personalDetail.MiddleName != null ? personalDetail.MiddleName : ""
            };

            var response = await blackSoilNBFCHelper.PersonUpdate(input, companyApi.TAPIKey, companyApi.TAPISecretKey);
            response.LeadId = leadid;
            response.LeadNBFCApiId = companyApi.Id;

            _context.BlackSoilCommonAPIRequestResponses.Add(response);
            _context.SaveChanges();
            ICreateLeadNBFCResponse personUpdateResponse = new CreateLeadNBFCResponse();


            personUpdateResponse.IsSuccess = response.IsSuccess;
            personUpdateResponse.Message = "Success";
            return personUpdateResponse;
        }
        public async Task<ICreateLeadNBFCResponse> BusinessUpdate(long leadid, long nbfcCompanyId)
        {
            BlackSoilNBFCHelper blackSoilNBFCHelper = new BlackSoilNBFCHelper();
            var blackSoilUpdates = _context.BlackSoilUpdates.FirstOrDefault(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);
            var companyApi = _context.LeadNBFCApis.FirstOrDefault(x => x.Code == CompanyApiConstants.BlackSoilCommonApiForSecret && x.IsActive && !x.IsDeleted);
            var businessdetails = _context.BusinessDetails.FirstOrDefault(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);

            BlackSoilBusinessUpdateInput input = new BlackSoilBusinessUpdateInput
            {
                update_url = blackSoilUpdates.BusinessUpdateUrl,
                business_type = businessdetails.BusEntityType,
                name = businessdetails.BusinessName
            };

            var response = await blackSoilNBFCHelper.BusinessUpdate(input, companyApi.TAPIKey, companyApi.TAPISecretKey);
            response.LeadId = leadid;
            response.LeadNBFCApiId = companyApi.Id;

            _context.BlackSoilCommonAPIRequestResponses.Add(response);
            _context.SaveChanges();
            ICreateLeadNBFCResponse BusinessUpdateResponse = new CreateLeadNBFCResponse();


            BusinessUpdateResponse.IsSuccess = response.IsSuccess;
            BusinessUpdateResponse.Message = "Success";
            return BusinessUpdateResponse;
        }
        public async Task<ICreateLeadNBFCResponse> PersonAddressUpdate(long leadid, long nbfcCompanyId)
        {
            BlackSoilNBFCHelper blackSoilNBFCHelper = new BlackSoilNBFCHelper();
            var blackSoilUpdates = _context.BlackSoilUpdates.FirstOrDefault(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);
            var personalDetail = _context.PersonalDetails.FirstOrDefault(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);
            var companyApi = _context.LeadNBFCApis.FirstOrDefault(x => x.Code == CompanyApiConstants.BlackSoilCommonApiForSecret && x.IsActive && !x.IsDeleted);

            string currentAddress = "";
            if (!string.IsNullOrEmpty(personalDetail.CurrentAddressLineOne))
            {
                currentAddress = personalDetail.CurrentAddressLineOne;
            }
            if (!string.IsNullOrEmpty(personalDetail.CurrentAddressLineTwo))
            {
                currentAddress = currentAddress + ", " + personalDetail.CurrentAddressLineTwo;
            }
            if (!string.IsNullOrEmpty(personalDetail.CurrentAddressLineThree))
            {
                currentAddress = currentAddress + ", " + personalDetail.CurrentAddressLineThree;
            }
            if (!string.IsNullOrEmpty(personalDetail.CurrentCityName))
            {
                currentAddress = currentAddress + ", " + personalDetail.CurrentCityName;
            }
            if (!string.IsNullOrEmpty(personalDetail.CurrentStateName))
            {
                currentAddress = currentAddress + ", " + personalDetail.CurrentStateName;
            }


            BlackSoilPersonAddressUpdateInput input = new BlackSoilPersonAddressUpdateInput
            {
                update_url = blackSoilUpdates.PersonAddressUpdateUrl,
                address_line = currentAddress,
                address_name = "permanent_address",
                address_type = "person_address",
                business = blackSoilUpdates.BusinessId.ToString(),
                city = personalDetail.CurrentCityName,
                state = personalDetail.CurrentStateName,
                country = "India",
                full_address = currentAddress,
                pincode = personalDetail.CurrentZipCode.ToString(),
                landmark = "",
                locality = ""
            };

            var response = await blackSoilNBFCHelper.PersonAddressUpdate(input, companyApi.TAPIKey, companyApi.TAPISecretKey);
            response.LeadId = leadid;
            response.LeadNBFCApiId = companyApi.Id;

            _context.BlackSoilCommonAPIRequestResponses.Add(response);
            _context.SaveChanges();
            ICreateLeadNBFCResponse PersonAddressUpdateResponse = new CreateLeadNBFCResponse();


            PersonAddressUpdateResponse.IsSuccess = response.IsSuccess;
            PersonAddressUpdateResponse.Message = "Success";
            return PersonAddressUpdateResponse;
        }
        public async Task<ICreateLeadNBFCResponse> BusinessAddressUpdate(long leadid, long nbfcCompanyId)
        {
            BlackSoilNBFCHelper blackSoilNBFCHelper = new BlackSoilNBFCHelper();
            var blackSoilUpdates = _context.BlackSoilUpdates.FirstOrDefault(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);
            var businessDetail = _context.BusinessDetails.FirstOrDefault(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);
            var companyApi = _context.LeadNBFCApis.FirstOrDefault(x => x.Code == CompanyApiConstants.BlackSoilCommonApiForSecret && x.IsActive && !x.IsDeleted);

            string businessAddress = "";
            if (!string.IsNullOrEmpty(businessDetail.AddressLineOne))
            {
                businessAddress = businessDetail.AddressLineOne;
            }
            if (!string.IsNullOrEmpty(businessDetail.AddressLineTwo))
            {
                businessAddress = businessAddress + ", " + businessDetail.AddressLineTwo;
            }
            if (!string.IsNullOrEmpty(businessDetail.AddressLineThree))
            {
                businessAddress = businessAddress + ", " + businessDetail.AddressLineThree;
            }
            if (!string.IsNullOrEmpty(businessDetail.CityName))
            {
                businessAddress = businessAddress + ", " + businessDetail.CityName;
            }
            if (!string.IsNullOrEmpty(businessDetail.StateName))
            {
                businessAddress = businessAddress + ", " + businessDetail.StateName;
            }


            BlackSoilBusinessAddressUpdateInput input = new BlackSoilBusinessAddressUpdateInput
            {
                update_url = blackSoilUpdates.BusinessAddressUpdateUrl,
                address_line = businessAddress,
                address_name = "mailing_address",
                address_type = "business_address",
                business = blackSoilUpdates.BusinessId.ToString(),
                city = businessDetail.CityName,
                state = businessDetail.StateName,
                country = "India",
                full_address = businessAddress,
                pincode = businessDetail.ZipCode.ToString(),
                landmark = "",
                locality = ""
            };

            var response = await blackSoilNBFCHelper.BusinessAddressUpdate(input, companyApi.TAPIKey, companyApi.TAPISecretKey);
            response.LeadId = leadid;
            response.LeadNBFCApiId = companyApi.Id;

            _context.BlackSoilCommonAPIRequestResponses.Add(response);
            _context.SaveChanges();
            ICreateLeadNBFCResponse BusinessAddressUpdateResponse = new CreateLeadNBFCResponse();
            BusinessAddressUpdateResponse.IsSuccess = response.IsSuccess;
            BusinessAddressUpdateResponse.Message = "Success";
            return BusinessAddressUpdateResponse;
        }
        public async Task<ICreateLeadNBFCResponse> BlackSoilCommonApplicationDetail(long leadid)
        {
            ICreateLeadNBFCResponse Response = new CreateLeadNBFCResponse();


            BlackSoilNBFCHelper blackSoilNBFCHelper = new BlackSoilNBFCHelper();
            var blackSoilUpdates = _context.BlackSoilUpdates.FirstOrDefault(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);
            if (blackSoilUpdates != null)
            {
                var companyApi = _context.LeadNBFCApis.FirstOrDefault(x => x.Code == CompanyApiConstants.BlackSoilCommonApplicationDetail && x.IsActive && !x.IsDeleted);
                if (companyApi != null)
                {
                    string apiurl = companyApi.APIUrl;

                    apiurl = apiurl.Replace("{{APPLICATION_ID}}", blackSoilUpdates.ApplicationId.ToString());

                    var response = await blackSoilNBFCHelper.BlackSoilCommonApplicationDetail(apiurl, companyApi.TAPIKey, companyApi.TAPISecretKey);
                    response.LeadId = leadid;

                    _context.BlackSoilCommonAPIRequestResponses.Add(response);
                    _context.SaveChanges();
                    Response.IsSuccess = response.IsSuccess;
                    Response.Message = "Success";
                }
            }
            return Response;
        }


        public async Task<ResultViewModel<string>> GetPFCollection(long leadid, string MobileNo)
        {
            ResultViewModel<string> Response = new ResultViewModel<string>();
            var apiList = await _leadNBFCSubActivityManager.GetLeadNBFCSubActivity(new LeadNBFCSubActivityRequest
            {
                Code = SubActivityConstants.PFCollection,
                LeadId = leadid,
                CompanyIdentificationCode = CompanyIdentificationCodeConstants.BlackSoil
            });
            var blackSoilUpdates = await _context.BlackSoilUpdates.FirstOrDefaultAsync(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);
            if (blackSoilUpdates != null)
            {
                var companyApi = await _context.LeadNBFCApis.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.BlackSoilCommonApplicationDetail && x.IsActive && !x.IsDeleted);
                if (companyApi != null)
                {
                    string apiurl = companyApi.APIUrl;

                    apiurl = apiurl.Replace("{{APPLICATION_ID}}", blackSoilUpdates.ApplicationId.ToString());
                    BlackSoilNBFCHelper blackSoilNBFCHelper = new BlackSoilNBFCHelper();
                    var res = await blackSoilNBFCHelper.BlackSoilCommonApplicationDetailPWA(apiurl, companyApi.TAPIKey, companyApi.TAPISecretKey);
                    res.LeadId = leadid;
                    res.LeadNBFCApiId = companyApi.Id;
                    await _context.BlackSoilCommonAPIRequestResponses.AddAsync(res);

                    var _blackSoilPFCollection = await _context.BlackSoilPFCollections.FirstOrDefaultAsync(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);

                    var pfdeserializeobject = JsonConvert.DeserializeObject<BlackSoilPfCollectionDc>(res.Response);
                    if (res.IsSuccess && _blackSoilPFCollection == null && pfdeserializeobject != null)
                    {
                        var blackSoilPFCollection = new BlackSoilPFCollection
                        {
                            LeadId = leadid,
                            //processing_fee = pfdeserializeobject.processing_fee ?? 0,
                            processing_fee = pfdeserializeobject.processing_fee_absolute ?? 0,
                            processing_fee_tax = pfdeserializeobject.processing_fee_tax ?? 0,
                            total_processing_fee = pfdeserializeobject.total_processing_fee ?? 0,
                            status = pfdeserializeobject.processing_fee_status,
                            IsActive = true,
                            IsDeleted = false
                        };
                        await _context.BlackSoilPFCollections.AddAsync(blackSoilPFCollection);

                    }
                    await _context.SaveChangesAsync();
                    if (res != null)
                    {
                        Response = await GetPFCollectionSession(leadid, MobileNo, res.IsSuccess);
                    }
                }
            }
            return Response;
        }

        public async Task<ResultViewModel<string>> GetPFCollectionSession(long leadid, string MobileNo, bool isSession)
        {
            ResultViewModel<string> Response = new ResultViewModel<string>();
            var apiList = await _leadNBFCSubActivityManager.GetLeadNBFCSubActivity(new LeadNBFCSubActivityRequest
            {
                Code = SubActivityConstants.PFCollection,
                LeadId = leadid,
                CompanyIdentificationCode = CompanyIdentificationCodeConstants.BlackSoil
            });
            if (!isSession)
            {
                BlackSoilPFCollectionPostDc blackSoilPFCollectionPostDc = new BlackSoilPFCollectionPostDc();
                var api = apiList.FirstOrDefault(x => x.Code == CompanyApiConstants.BlackSoilGetEmbedSession);
                BlackSoilNBFCHelper blackSoilNBFCHelper = new BlackSoilNBFCHelper();
                var apiRes = await blackSoilNBFCHelper.BlackSoilGetEmbedSession(api.APIUrl, api.TAPIKey, api.TAPISecretKey, MobileNo);
                if (apiRes.IsSuccess)
                {
                    var sessionRes = JsonConvert.DeserializeObject<BlackSoilPFCollectionPostDc>(apiRes.Response);
                    blackSoilPFCollectionPostDc = sessionRes;
                }

                var apis = apiList.FirstOrDefault(x => x.Code == CompanyApiConstants.BlackSoilPFCollection);
                if (apis != null)
                    Response.Result = apis.APIUrl.Replace("{SESSION_ID}", blackSoilPFCollectionPostDc.session_id.ToString()).Replace("{BUSINESS_ID}", blackSoilPFCollectionPostDc.business_id.ToString()).Replace("{PERSON_ID}", blackSoilPFCollectionPostDc.person_id.ToString());

                Response.IsSuccess = apiRes.IsSuccess;
                Response.Message = apiRes.IsSuccess ? "Success" : "Failed";

            }
            else
            {
                // Insert PF Collection LeadId, PFAmount, 


                var leadActivity = await _context.LeadActivityMasterProgresses.Where(x => x.LeadMasterId == leadid
                              && !x.IsCompleted && x.IsActive && !x.IsDeleted && x.ActivityMasterName == ActivityConstants.PFCollection && x.SubActivityMasterName == SubActivityConstants.PFCollection).FirstOrDefaultAsync();
                if (leadActivity != null)
                {
                    leadActivity.IsCompleted = true;
                    leadActivity.IsApproved = 1;
                    _context.Entry(leadActivity).State = EntityState.Modified;

                }
                Response.IsSuccess = true;
                Response.Message = "Activity Completed";

            }
            await _context.SaveChangesAsync();

            return Response;
        }

        public Task<LBABusinessLoanDc> LBABusinessLoan(long leadid, string AgreementURL, bool IsSubmit, ProductCompanyConfigDc loanconfig)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> GenerateOtpToAcceptOffer(long LeadMasterId)
        {
            throw new NotImplementedException();
        }

        public Task<GRPCReply<string>> AcceptOfferWithXMLAadharOTP(GRPCRequest<SecondAadharXMLDc> request)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> GetLeadMasterByLeadId(long leadId)
        {
            throw new NotImplementedException();
        }

        public Task<LoanInsuranceConfiguration> GetRateOfInterest(int tenure)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> OfferRegenerate(int LeadId, int tenure, double sactionAmount)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> GetLoanByLoanId(long Leadmasterid)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> GetDisbursementAPI(long Leadmasterid)
        {
            throw new NotImplementedException();
        }

        public Task<List<LeadLoanDataDc>> GetLoan(long LeadMasterId)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> LoanRepaymentScheduleDetails(long LeadMasterId)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> LoanNach(string UMRN, long Leadmasterid)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> ChangeLoanStatus(long LeadMasterId, string Status)
        {
            throw new NotImplementedException();
        }

        public Task<RePaymentScheduleDataDc> GetDisbursedLoanDetail(long Leadmasterid)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> SaveAgreementESignDocument(long leadmasterid, string eSignDocumentURL)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> LoanDataSave(long leadid, long id)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> UpdateBeneficiaryBankDetail(BeneficiaryBankDetailDc Obj)
        {
            throw new NotImplementedException();
        }

        public Task<ResultViewModel<List<OfferEmiDetailDC>>> GetOfferEmiDetails(long leadId, int tenure = 0)
        {
            throw new NotImplementedException();
        }

        public Task<ResultViewModel<string>> GetOfferEmiDetailsDownloadPdf(long leadId, int ReqTenure = 0)
        {
            throw new NotImplementedException();
        }

        public Task<GRPCReply<string>> GetOfferEmiDetailsDownloadPdf(GRPCRequest<EmiDetailReqDc> leadId)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> InsertCeplrBanklist()
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> InsertLoanDataByArthmateTest(long leadid, string loanid)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> eSignSessionAsync(string agreementPdfUrl, long leadid)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> eSignDocumentsAsync(long leadid, string DocumentId)
        {
            throw new NotImplementedException();
        }

        public Task<string> AgreementEsign(GRPCRequest<EsignAgreementRequest> req)
        {
            throw new NotImplementedException();
        }

        public Task<GRPCReply<string>> GenerateToken()
        {
            throw new NotImplementedException();
        }

        public async Task<GRPCReply<string>> AddLead(GRPCRequest<AyeleadReq> ayeleadReq)
        {
            throw new NotImplementedException();
        }
        public async Task<GRPCReply<CheckCreditLineData>> CheckCreditLine(GRPCRequest<AyeleadReq> ayeleadReq)
        {
            throw new NotImplementedException();
        }

        public async Task<GRPCReply<string>> GetWebUrl(GRPCRequest<AyeleadReq> ayeleadReq)
        {
            throw new NotImplementedException();
        }

        public async Task<GRPCReply<string>> TransactionSendOtp(GRPCRequest<AyeleadReq> ayeleadReq)
        {
            throw new NotImplementedException();
        }
        public async Task<GRPCReply<string>> TransactionVerifyOtp(GRPCRequest<TransactionVerifyOtpReqDc> ayeleadReq)
        {
            throw new NotImplementedException();
        }
   
    }
}