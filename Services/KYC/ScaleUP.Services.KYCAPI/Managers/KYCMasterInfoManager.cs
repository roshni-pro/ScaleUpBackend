//using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Json;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Global.Infrastructure.Constants.DSA;
using ScaleUP.Services.KYCAPI.Migrations;
using ScaleUP.Services.KYCAPI.Persistence;
using ScaleUP.Services.KYCDTO.Constant;
using ScaleUP.Services.KYCDTO.Transacion;
using ScaleUP.Services.KYCModels.Master;
using ScaleUP.Services.KYCModels.Transaction;
using static IdentityServer4.Models.IdentityResources;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ScaleUP.Services.KYCAPI.Managers
{
    public class KYCMasterInfoManager : BaseManager
    {
        public KYCMasterInfoManager(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<KYCAllInfoResponse>> GetAllKycInfo(KYCAllInfoRequest kYCAllInfoRequest)
        {
            var query = from m in _context.KYCMasters
                        join mi in _context.KYCMasterInfos on m.Id equals mi.KYCMasterId
                        where mi.UserId == kYCAllInfoRequest.UserId 
                                && mi.IsActive 
                                && !mi.IsDeleted
                                && mi.ProductCode == kYCAllInfoRequest.ProductCode
                        select new KYCAllInfoResponse
                        {
                            KYCMasterId = m.Id,
                            KYCMasterCode = m.Code,
                            KYCMasterInfoId = mi.Id,
                            KycDetailInfoList = new List<KYCDetailInfoResponse>(),
                            UniqueId = mi.UniqueId
                        };
            List<KYCAllInfoResponse> kYCallInfoResponse = query.ToList();
            if (kYCallInfoResponse != null)
            {
                foreach (var item in kYCallInfoResponse)
                {
                    var DetailQuery = from d in _context.KYCDetails
                                      join df in _context.KYCDetailInfos on d.Id equals df.KYCDetailId
                                      where df.KYCMasterInfoId == item.KYCMasterInfoId
                                      && df.IsActive == true
                                      && df.IsDeleted == false
                                      select new KYCDetailInfoResponse
                                      {
                                          FieldName = d.Field,
                                          FieldType = d.FieldType,
                                          FieldValue = df.FieldValue,
                                          FieldInfoType = d.FieldInfoType
                                      };
                    item.KycDetailInfoList = DetailQuery.ToList();
                }
            }
            return kYCallInfoResponse;
        }


        public async Task<UserDetailsReply> GetKycInfos(string userid)
        {
            UserDetailsReply userDetailsReply = new UserDetailsReply();
            var result = await GetAllKycInfo(new KYCAllInfoRequest { UserId = userid });

            if(result != null && result.Any())
            {
                var panInfo =result.FirstOrDefault(x => x.KYCMasterCode == KYCMasterConstants.PAN);
                if(panInfo != null)
                {
                    #region PAN Detail
                    PanDetailsDc panDetailsDc = new PanDetailsDc();
                    userDetailsReply.panDetail = panDetailsDc;
                    panDetailsDc.UniqueId = panInfo.UniqueId;
                    if(panInfo.KycDetailInfoList != null) {
                        var age =panInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCDetailConstants.Age);
                        if (age != null && !string.IsNullOrEmpty( age.FieldValue))
                        {
                            int intAge;
                            bool isSuccess = int.TryParse(age.FieldValue, out intAge);
                            if(isSuccess)
                            {
                                panDetailsDc.Age = intAge;
                            }
                        }

                        var nameOnCard = panInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCDetailConstants.NameOnCard);
                        if (nameOnCard != null && !string.IsNullOrEmpty(nameOnCard.FieldValue))
                        {
                            panDetailsDc.NameOnCard = nameOnCard.FieldValue;
                        }

                        var DOB = panInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCDetailConstants.DOB); 
                        if(DOB != null && !string.IsNullOrEmpty(DOB.FieldValue))
                        {
                            DateTime datedob;
                            bool isSuccess = DateTime.TryParse(DOB.FieldValue, out datedob);

                            if(isSuccess)
                            {
                                panDetailsDc.DOB = DateTime.Parse(DOB.FieldValue);
                            }  
                        }

                        var DateOfIssue = panInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCDetailConstants.DateOfIssue);
                        if (DateOfIssue != null && !string.IsNullOrEmpty(DateOfIssue.FieldValue))
                        {
                            DateTime dateDateOfIssue;
                            bool isSuccess = DateTime.TryParse(DateOfIssue.FieldValue, out dateDateOfIssue);

                            if (isSuccess)
                            {
                                panDetailsDc.DateOfIssue = DateTime.Parse(DateOfIssue.FieldValue);
                            }     
                        }

                        var FatherName = panInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName ==KYCDetailConstants.FatherName);
                        if (FatherName != null && !string.IsNullOrEmpty(FatherName.FieldValue))
                        {
                            panDetailsDc.FatherName = FatherName.FieldValue;
                        }

                        var Minor = panInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCDetailConstants.Minor);
                        if (Minor != null && !string.IsNullOrEmpty(Minor.FieldValue))
                        {
                            bool boolMinor;
                            bool isSuccess = bool.TryParse(Minor.FieldValue, out boolMinor);
                            if (isSuccess)
                            {
                                panDetailsDc.Minor = bool.Parse(Minor.FieldValue);
                            }
                            
                        }

                        var DocumentId = panInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCDetailConstants.DocumentId);
                        if (DocumentId != null && !string.IsNullOrEmpty(DocumentId.FieldValue))
                        {
                            panDetailsDc.FrontImageUrl = DocumentId.FieldValue;
                        }

                        var PanType = panInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCDetailConstants.PanType);
                        if (PanType != null && !string.IsNullOrEmpty(PanType.FieldValue))
                        {
                            panDetailsDc.PanType = PanType.FieldValue;
                        }

                        userDetailsReply.panDetail = panDetailsDc;

                    }
                    #endregion

                }

                var aadharInfo = result.FirstOrDefault(x => x.KYCMasterCode == KYCMasterConstants.Aadhar);
                if(aadharInfo != null)
                {
                    #region Aadhar Detail
                    AadharDetailsDc aadharDetailsDc = new AadharDetailsDc();
                    userDetailsReply.aadharDetail = aadharDetailsDc;
                    #endregion
                }
            }

            return userDetailsReply;
        }

        public async Task<GRPCReply<bool>> RemoveKYCMasterInfo(GRPCRequest<string> userid)
        {
            GRPCReply<bool> reply = new GRPCReply<bool>();
            var KyMasterInfo = _context.KYCMasterInfos.Where(x=> x.UserId ==  userid.Request && x.IsActive && !x.IsDeleted).Include(y=> y.KYCDetailInfoList).ToList();
           
            if(KyMasterInfo !=null)
            {
                foreach (var item in KyMasterInfo)
                {
                    item.IsActive = false;
                    item.IsDeleted = true;
                    if(item.KYCDetailInfoList!=null)
                    {
                        foreach (var detail in item.KYCDetailInfoList)
                        {
                            detail.IsActive = false;
                            detail.IsDeleted = true;
                            _context.Update(detail);
                        }
                    }
                    _context.Update(item);
                }
                _context.SaveChanges();
            }
            reply.Response = true;
            return reply;
        }


        public async Task<ResultViewModel<bool>> IsValidTOSave(string UserId, string EmailId)
        {
            ResultViewModel<bool> res = null;
            var query = from m in _context.KYCMasters
                        join mi in _context.KYCMasterInfos on m.Id equals mi.KYCMasterId
                        join kd in _context.KYCDetailInfos on mi.Id equals kd.KYCMasterInfoId
                        join d in _context.KYCDetails on kd.KYCDetailId equals d.Id
                        where d.Field == KYCPersonalDetailConstants.EmailId
                            && mi.IsActive == true
                            && mi.IsDeleted == false
                            && m.Code == KYCMasterConstants.PersonalDetail
                            && kd.FieldValue == EmailId
                            && mi.UserId != UserId
                            && kd.IsActive == true && kd.IsDeleted == false
                            && d.IsActive == true && d.IsDeleted == false
                        select mi;
            var masterInfo = query.ToList();
            if (masterInfo != null && masterInfo.Count >= 1)
            {
                res = new ResultViewModel<bool>
                {
                    IsSuccess = true,
                    Message = "Email already exists",
                    Result = false
                };
                return res;
            }
            else
            {
                res = new ResultViewModel<bool>
                {
                    IsSuccess = false,
                    Message = "Not Exists",
                    Result = false
                };
            }
            return res;
        }

        public async Task<ResultViewModel<bool>> IsIVRSNumberValidTOSave(string UserId, string IVRSNumber)
        {
            ResultViewModel<bool> res = null;
            var query = from m in _context.KYCMasters
                        join mi in _context.KYCMasterInfos on m.Id equals mi.KYCMasterId
                        join kd in _context.KYCDetailInfos on mi.Id equals kd.KYCMasterInfoId
                        join d in _context.KYCDetails on kd.KYCDetailId equals d.Id
                        where d.Field == KYCPersonalDetailConstants.IVRSNumber
                            && mi.IsActive == true
                            && mi.IsDeleted == false
                            && m.Code == KYCMasterConstants.PersonalDetail
                            && kd.FieldValue == IVRSNumber
                            && mi.UserId != UserId
                            && kd.IsActive == true && kd.IsDeleted == false
                            && d.IsActive == true && d.IsDeleted == false
                        select mi;
            var masterInfo = query.ToList();
            if (masterInfo != null && masterInfo.Count >= 1)
            {
                res = new ResultViewModel<bool>
                {
                    IsSuccess = true,
                    Message = "IVRS Number already exists",
                    Result = true
                };
                return res;
            }
            else
            {
                res = new ResultViewModel<bool>
                {
                    IsSuccess = false,
                    Message = "Not Exists",
                    Result = false
                };
            }
            return res;
        }


        public async Task<KYCAllInfoResponse> GetKycInfoByDoc(KYCAllInfoRequest kYCAllInfoRequest,string KycMasterCode)
        {
            var query = from m in _context.KYCMasters
                        join mi in _context.KYCMasterInfos on m.Id equals mi.KYCMasterId
                        where mi.UserId == kYCAllInfoRequest.UserId
                                && mi.IsActive
                                && !mi.IsDeleted
                                && mi.ProductCode == kYCAllInfoRequest.ProductCode
                                && m.Code == KycMasterCode
                        select new KYCAllInfoResponse
                        {
                            KYCMasterId = m.Id,
                            KYCMasterCode = m.Code,
                            KYCMasterInfoId = mi.Id,
                            KycDetailInfoList = new List<KYCDetailInfoResponse>(),
                            UniqueId = mi.UniqueId
                        };
            KYCAllInfoResponse kYCallInfoResponse = query.FirstOrDefault();
            if (kYCallInfoResponse != null)
            {
                    var DetailQuery = from d in _context.KYCDetails
                                      join df in _context.KYCDetailInfos on d.Id equals df.KYCDetailId
                                      where df.KYCMasterInfoId == kYCallInfoResponse.KYCMasterInfoId
                                      && d.Field == (KycMasterCode == "BuisnessDetail" ? KYCBuisnessDetailConstants.CurrentAddressId : KYCPersonalDetailConstants.CurrentAddressId)
                                      select df;
                    var kycDetailDetail = DetailQuery.FirstOrDefault();

                kycDetailDetail.IsActive = false;
                kycDetailDetail.IsDeleted = true;
                _context.Update(kycDetailDetail);
            }
            return kYCallInfoResponse;
        }



        public async Task<ResultViewModel<bool>> UpdateBuisnessAddressDoc(string UserId, long CurrentAddressId)
        {
            ResultViewModel<bool> res = null;
            var query = from m in _context.KYCMasters
                        join mi in _context.KYCMasterInfos on m.Id equals mi.KYCMasterId
                        join kd in _context.KYCDetailInfos on mi.Id equals kd.KYCMasterInfoId
                        join d in _context.KYCDetails on kd.KYCDetailId equals d.Id
                        where d.Field == KYCBuisnessDetailConstants.CurrentAddressId
                            && mi.IsActive == true
                            && mi.IsDeleted == false
                            && m.Code == KYCMasterConstants.BuisnessDetail
                            && kd.FieldValue == CurrentAddressId.ToString()
                            && mi.UserId == UserId
                            && kd.IsActive == true && kd.IsDeleted == false
                            && d.IsActive == true && d.IsDeleted == false
                            && mi.ProductCode == ""
                        select kd;
            var detailInfo = query.FirstOrDefaultAsync();
            //if (detailInfo != null)
            //{
            //    var KYCMasterInfoId = detailInfo != null ? detailInfo.KYCMasterInfoId : 0;
            //    var queryy = from d in _context.KYCDetails
            //                join kd in _context.KYCDetailInfos on d.Id equals kd.KYCDetailId
            //                where d.Field == KYCBuisnessDetailConstants.CurrentAddressId
            //                    && kd.FieldValue == CurrentAddressId.ToString()
            //                    && kd.IsActive == true && kd.IsDeleted == false
            //                    && d.IsActive == true && d.IsDeleted == false
            //                    && kd.KYCMasterInfoId == detailInfo.KYCMasterInfoId
            //                select kd;
            //    var detailInfo = query.ToList();
            //    res = new ResultViewModel<bool>
            //    {
            //        IsSuccess = true,
            //        Message = "IVRS Number already exists",
            //        Result = true
            //    };
            //    return res;
            //}
            //else
            //{
            //    res = new ResultViewModel<bool>
            //    {
            //        IsSuccess = false,
            //        Message = "Not Exists",
            //        Result = false
            //    };
            //}
            return res;
        }

        public async Task<GRPCReply<bool>> RemoveDSAPersonalDetails(GRPCRequest<string> req)
        {
            GRPCReply<bool> gRPCReply = new GRPCReply<bool>() { Message="Failed to Remove"};
            List<long> kycMasterCodeIds = new List<long>();
            var query = from m in _context.KYCMasters
                        join mi in _context.KYCMasterInfos on m.Id equals mi.KYCMasterId
                        join kd in _context.KYCDetailInfos on mi.Id equals kd.KYCMasterInfoId
                        join d in _context.KYCDetails on kd.KYCDetailId equals d.Id
                        where  mi.IsActive == true
                            && mi.IsDeleted == false
                            && m.Code == KYCMasterConstants.DSAProfileType
                            && mi.UserId == req.Request
                            && kd.IsActive == true && kd.IsDeleted == false
                            && d.IsActive == true && d.IsDeleted == false
                        select new { kd.FieldValue , m.Id};

            var data = query.FirstOrDefault();

            if (data!=null)
            {
                kycMasterCodeIds.Add(data.Id);
                var dsaPerDetailCode = (data.FieldValue == DSAProfileTypeConstants.DSA) ?
                    await _context.KYCMasters.FirstOrDefaultAsync(x => x.IsActive && !x.IsDeleted && x.Code == KYCMasterConstants.DSAPersonalDetail) :
                    await _context.KYCMasters.FirstOrDefaultAsync(x => x.IsActive && !x.IsDeleted && x.Code == KYCMasterConstants.ConnectorPersonalDetail);

                if(dsaPerDetailCode != null)
                {
                    kycMasterCodeIds.Add(dsaPerDetailCode.Id);
                }
        
            }

            var dsaTypeAndPersonalDetail = await _context.KYCMasterInfos.Where(x => x.UserId == req.Request && x.IsActive && !x.IsDeleted 
                                                        && kycMasterCodeIds.Contains(x.KYCMasterId)).ToListAsync();

            foreach (var detail in dsaTypeAndPersonalDetail)
            {
                detail.IsActive = false;
                detail.IsDeleted = true;
                _context.Entry(detail).State = EntityState.Modified;
            }
            if (_context.SaveChanges() > 0)
            {
                gRPCReply.Response = true;
            }
            return gRPCReply;
        }

        public async Task<List<KYCAllInfoResponse>> GetAllKycInfoByUserIdsList(KYCAllInfoByUserIdsRequest kYCAllInfoRequest)
        {
            var query = from m in _context.KYCMasters
                        join mi in _context.KYCMasterInfos on m.Id equals mi.KYCMasterId
                        where kYCAllInfoRequest.UserId.Contains(mi.UserId)
                                && mi.IsActive
                                && !mi.IsDeleted
                                && mi.ProductCode == kYCAllInfoRequest.ProductCode
                        select new KYCAllInfoResponse
                        {
                            KYCMasterId = m.Id,
                            KYCMasterCode = m.Code,
                            KYCMasterInfoId = mi.Id,
                            KycDetailInfoList = new List<KYCDetailInfoResponse>(),
                            UniqueId = mi.UniqueId
                        };
            List<KYCAllInfoResponse> kYCallInfoResponse = query.ToList();
            if (kYCallInfoResponse != null)
            {
                foreach (var item in kYCallInfoResponse)
                {
                    var DetailQuery = from d in _context.KYCDetails
                                      join df in _context.KYCDetailInfos on d.Id equals df.KYCDetailId
                                      where df.KYCMasterInfoId == item.KYCMasterInfoId
                                      && df.IsActive == true
                                      && df.IsDeleted == false
                                      select new KYCDetailInfoResponse
                                      {
                                          FieldName = d.Field,
                                          FieldType = d.FieldType,
                                          FieldValue = df.FieldValue,
                                          FieldInfoType = d.FieldInfoType
                                      };
                    item.KycDetailInfoList = DetailQuery.ToList();
                }
            }
            return kYCallInfoResponse;
        }

        public async Task<ResultViewModel<bool>> IsDSAEmailExist(string UserId, string EmailId, string productCode)
        {
            ResultViewModel<bool> res = null;
            var query = from m in _context.KYCMasters
                        join mi in _context.KYCMasterInfos on m.Id equals mi.KYCMasterId
                        join kd in _context.KYCDetailInfos on mi.Id equals kd.KYCMasterInfoId
                        join d in _context.KYCDetails on kd.KYCDetailId equals d.Id
                        where d.Field == KYCPersonalDetailConstants.EmailId
                            && mi.IsActive == true
                            && mi.IsDeleted == false
                            && kd.FieldValue == EmailId
                            && mi.UserId != UserId
                            && kd.IsActive == true && kd.IsDeleted == false
                            && d.IsActive == true && d.IsDeleted == false
                        select mi;
            var masterInfo = query.ToList();
            if (masterInfo != null && masterInfo.Count >= 1)
            {
                res = new ResultViewModel<bool>
                {
                    IsSuccess = true,
                    Message = "Email already exists",
                    Result = false
                };
                return res;
            }
            else
            {
                res = new ResultViewModel<bool>
                {
                    IsSuccess = false,
                    Message = "Not Exists",
                    Result = false
                };
            }
            return res;
        }

        public async Task<GRPCReply<bool>> DSAGSTExist(GRPCRequest<DSAGSTExistRequest> dSAGSTExistRequest)
        {
            GRPCReply<bool> res = new GRPCReply<bool>();
            var query = from m in _context.KYCMasters
                        join mi in _context.KYCMasterInfos on m.Id equals mi.KYCMasterId
                        join kd in _context.KYCDetailInfos on mi.Id equals kd.KYCMasterInfoId
                        join d in _context.KYCDetails on kd.KYCDetailId equals d.Id
                        where d.Field == DSAPersonalDetailConstants.GSTNumber
                            && mi.IsActive == true
                            && mi.IsDeleted == false
                            && kd.FieldValue == dSAGSTExistRequest.Request.gst
                            && mi.UserId != dSAGSTExistRequest.Request.UserId
                            && kd.IsActive == true && kd.IsDeleted == false
                            && d.IsActive == true && d.IsDeleted == false
                            && mi.ProductCode == dSAGSTExistRequest.Request.productCode
                        select mi;
            var masterInfo = query.ToList();
            if (masterInfo != null && masterInfo.Count >= 1)
            {
                res.Status = true;
                res.Message = "GST already exists";
                res.Response = false;
                return res;
            }
            else
            {
                res.Status = false;
                res.Message = "Not Exists";
                res.Response = false;
            }
            return res;
        }

        public async Task<GRPCReply<List<DSACityListDc>>> GetDSACityList(GRPCRequest<string> req)
        {
            GRPCReply<List<DSACityListDc>> res = new GRPCReply<List<DSACityListDc>>();
            var masterInfo = new List<DSACityListDc>(); 
            if (req.Request == DSAProfileTypeConstants.Connector || req.Request == "All") 
            {
                var KYCCodeList = new List<string> { KYCMasterConstants.ConnectorPersonalDetail, KYCMasterConstants.Aadhar };
                var KYCFieldList = new List<string> { DSAPersonalDetailConstants.WorkingLocation, KYCAadharConstants.LocationId };
                var query = from m in _context.KYCMasters
                            join mi in _context.KYCMasterInfos on m.Id equals mi.KYCMasterId
                            join d in _context.KYCDetails on m.Id equals d.KYCMasterId
                            join kd in _context.KYCDetailInfos on new { x1 = mi.Id, x2 = d.Id } equals new { x1 = kd.KYCMasterInfoId, x2 = kd.KYCDetailId }
                            where KYCCodeList.Contains(m.Code)  && KYCFieldList.Contains(d.Field) 
                                && mi.IsActive == true && mi.IsDeleted == false
                                && kd.IsActive == true && kd.IsDeleted == false
                                && d.IsActive == true && d.IsDeleted == false
                            select new DSACityListDc { CityId = Convert.ToInt64(kd.FieldValue), CityName = "" };
                 masterInfo = query.Distinct().ToList();
            }
            if (req.Request == DSAProfileTypeConstants.DSA || req.Request == "All")
            {
                var query = from m in _context.KYCMasters
                            join mi in _context.KYCMasterInfos on m.Id equals mi.KYCMasterId
                            join d in _context.KYCDetails on m.Id equals d.KYCMasterId
                            join kd in _context.KYCDetailInfos on new { x1 = mi.Id, x2 = d.Id } equals new { x1 = kd.KYCMasterInfoId, x2 = kd.KYCDetailId }
                            where m.Code == KYCMasterConstants.DSAPersonalDetail && d.Field == DSAPersonalDetailConstants.CurrentAddressId
                                && mi.IsActive == true && mi.IsDeleted == false
                                && kd.IsActive == true && kd.IsDeleted == false
                                && d.IsActive == true && d.IsDeleted == false
                            select new DSACityListDc { CityId = Convert.ToInt64(kd.FieldValue), CityName = "" };
                
                var masterData = query.Distinct().ToList();
                //masterInfo = masterInfo!=null && masterInfo.Any() ? masterInfo.AddRange(query.Distinct().ToList()) : query.Distinct().ToList();  
                if (masterInfo.Any())
                    masterInfo.AddRange(masterData);
                else
                    masterInfo = masterData;
            }

            if (masterInfo != null && masterInfo.Any())
            {
                res.Status = true;
                res.Message = "Data found";
                res.Response = masterInfo;
                return res;
            }
            else
            {
                res.Status = false;
                res.Message = "Data Not Exists";
            }
            return res;
        }
        public async Task<GRPCReply<bool>> UpdateAadharStatus(KYCAllInfoByUserIdsRequest kYCAllInfoRequest)
        {
            GRPCReply<bool> gRPCReply = new GRPCReply<bool>();
            var UserName = kYCAllInfoRequest.UserId.FirstOrDefault();
            var AAdharId = _context.KYCMasters.Where(x => x.IsActive == true && x.IsDeleted == false && x.Code == KYCMasterConstants.Aadhar.ToString()).First();
            var kycMasterData = await _context.KYCMasterInfos
                                    .Include("KYCDetailInfoList")
                                    .Where(x => x.UserId == UserName && x.KYCMasterId == AAdharId.Id && x.ProductCode == kYCAllInfoRequest.ProductCode)
                                    .OrderByDescending(x=>x.Id)
                                    .FirstOrDefaultAsync();
            KYCMasterInfo kYCMasterInfo = null;
            if (kycMasterData != null)
            {
                kYCMasterInfo = kycMasterData;
                kYCMasterInfo.LastModified = DateTime.Now;
                kYCMasterInfo.LastModifiedBy = kYCAllInfoRequest.CreatedBy;
                kYCMasterInfo.IsActive = true;
                kYCMasterInfo.IsDeleted = false;
                _context.Entry(kYCMasterInfo).State = EntityState.Modified;
                _context.SaveChanges();
                gRPCReply.Status = true;
                gRPCReply.Message = "Changed Successfully.";

            }
            return gRPCReply;
        }

    }
}
