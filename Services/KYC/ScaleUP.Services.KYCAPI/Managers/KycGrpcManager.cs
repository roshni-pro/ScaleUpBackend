using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Nito.AsyncEx;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.KYCActivity;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Location.DataContracts;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Global.Infrastructure.Constants.Lead;
using ScaleUP.Services.KYCAPI.KYCFactory;
using ScaleUP.Services.KYCAPI.Persistence;
using ScaleUP.Services.KYCDTO.Transacion;
using ScaleUP.Services.KYCModels.Master;
using ScaleUP.Services.KYCModels.Transaction;
using Serilog;

namespace ScaleUP.Services.KYCAPI.Managers
{
    public class KycGrpcManager
    {
        private string _basePath;
        private readonly ApplicationDbContext _context;
        private IHostEnvironment _hostingEnvironment;
        public KycGrpcManager(ApplicationDbContext context, IHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _basePath = _hostingEnvironment.ContentRootPath;
        }
        public async Task<KYCPANReply> GetKYCPAN(KYCPanRequest request)
        {
            KYCPANReply kYCPANReply = new KYCPANReply();
            kYCPANReply.Status = false;
            KYCDocFactory kYCDocFactory = new KYCDocFactoryConcrete(_context, request.UserId, _basePath);
            IDocType<KarzaPANDTO, KYCActivityPAN> docType = kYCDocFactory.GetDocType<KarzaPANDTO, KYCActivityPAN>(Global.Infrastructure.Constants.KYCMasterConstants.PAN);
            var docDetails1 = await docType.GetDoc(request.UserId);
            var docDetails = docDetails1.Result;
            if (docDetails != null)
            {
                if (docDetails.Any(x => x.Key == "UniqueId"))
                {
                    kYCPANReply.UniqueId = docDetails.FirstOrDefault(x => x.Key == "UniqueId").Value;
                }
                if (docDetails.Any(x => x.Key == KYCDetailConstants.PanDocId))
                {
                    kYCPANReply.Status = true;
                    kYCPANReply.DocumentId = Convert.ToInt64(docDetails.FirstOrDefault(x => x.Key == KYCDetailConstants.PanDocId).Value);
                }
            }
            return kYCPANReply;
        }
        public async Task<KYCAadharReply> GetKYCAadhar(KYCAadharRequest request)
        {
            KYCDocFactory kYCDocFactory = new KYCDocFactoryConcrete(_context, request.UserId, _basePath);
            IDocType<KarzaPANDTO, KYCActivityPAN> docType = kYCDocFactory.GetDocType<KarzaPANDTO, KYCActivityPAN>(Global.Infrastructure.Constants.KYCMasterConstants.PAN);
            KYCAadharReply kYCPANReply = new KYCAadharReply();
            kYCPANReply.Status = false;
            var docDetails1 = await docType.GetDoc(request.UserId);
            var docDetails = docDetails1.Result;
            if (docDetails.Any(x => x.Key == "UniqueId"))
            {
                kYCPANReply.UniqueId = docDetails.FirstOrDefault(x => x.Key == "UniqueId").Value;
            }
            if (docDetails.Any(x => x.Key == KYCAadharConstants.FrontDocumentId))
            {
                kYCPANReply.Status = true;
                kYCPANReply.FrontDocumentId = Convert.ToInt64(docDetails.FirstOrDefault(x => x.Key == KYCAadharConstants.FrontDocumentId).Value);
                kYCPANReply.BackDocumentId = Convert.ToInt64(docDetails.FirstOrDefault(x => x.Key == KYCAadharConstants.BackDocumentId).Value);
            }
            return kYCPANReply;
        }
        public async Task<KYCBankStatementReply> GetKYCBankStatement(KYCBankStatementRequest request)
        {
            KYCDocFactory kYCDocFactory = new KYCDocFactoryConcrete(_context, request.UserId, _basePath);
            IDocType<KarzaPANDTO, KYCActivityPAN> docType = kYCDocFactory.GetDocType<KarzaPANDTO, KYCActivityPAN>(Global.Infrastructure.Constants.KYCMasterConstants.PAN);
            KYCBankStatementReply kYCPANReply = new KYCBankStatementReply();
            kYCPANReply.Status = false;
            var docDetails1 = await docType.GetDoc(request.UserId);
            var docDetails = docDetails1.Result;
            if (docDetails.Any(x => x.Key == KYCBankStatementConstants.BorroBankAccNum))
            {
                kYCPANReply.Status = true;
                kYCPANReply.DocumentId = Convert.ToInt64(docDetails.FirstOrDefault(x => x.Key == KYCBankStatementConstants.DocumentId).Value); ;
            }
            return kYCPANReply;
        }
        public async Task<KYCMSMEReply> GetKYCMSME(KYCMSMERequest request)
        {
            KYCDocFactory kYCDocFactory = new KYCDocFactoryConcrete(_context, request.UserId, _basePath);

            IDocType<KarzaPANDTO, KYCActivityPAN> docType = kYCDocFactory.GetDocType<KarzaPANDTO, KYCActivityPAN>(Global.Infrastructure.Constants.KYCMasterConstants.PAN);
            KYCMSMEReply kYCPANReply = new KYCMSMEReply();
            kYCPANReply.Status = false;
            var docDetails1 = await docType.GetDoc(request.UserId);
            var docDetails = docDetails1.Result;
            if (docDetails.Any(x => x.Key == KYCMSMEConstants.FrontDocumentId))
            {
                kYCPANReply.Status = true;
                kYCPANReply.DocumentId = Convert.ToInt64(docDetails.FirstOrDefault(x => x.Key == KYCMSMEConstants.FrontDocumentId).Value); ;
            }
            return kYCPANReply;
        }
        public async Task<KYCPersonalDetailReply> GetKYCPersonalDetail(KYCPersonalDetailRequest request, ProtoBuf.Grpc.CallContext context = default)
        {
            KYCDocFactory kYCDocFactory = new KYCDocFactoryConcrete(_context, request.UserId, _basePath);
            IDocType<KarzaPANDTO, KYCActivityPAN> docType = kYCDocFactory.GetDocType<KarzaPANDTO, KYCActivityPAN>(Global.Infrastructure.Constants.KYCMasterConstants.PAN);
            KYCPersonalDetailReply kYCPersonalDetailReply = new KYCPersonalDetailReply();
            kYCPersonalDetailReply.Status = false;
            var docDetails = await docType.GetDoc(request.UserId);
            if (docDetails != null)
            {
                kYCPersonalDetailReply.Status = true;
            }
            return kYCPersonalDetailReply;
        }
        public async Task<KYCSelfieReply> GetKYCSelfie(KYCSelfieRequest request)
        {
            KYCDocFactory kYCDocFactory = new KYCDocFactoryConcrete(_context, request.UserId, _basePath);
            IDocType<KarzaPANDTO, KYCActivityPAN> docType = kYCDocFactory.GetDocType<KarzaPANDTO, KYCActivityPAN>(Global.Infrastructure.Constants.KYCMasterConstants.PAN);
            KYCSelfieReply kYCSelfieReply = new KYCSelfieReply();
            kYCSelfieReply.Status = false;
            var docDetails = await docType.GetDoc(request.UserId);
            if (docDetails != null)
            {
                kYCSelfieReply.Status = true;
            }
            return kYCSelfieReply;
        }
        public async Task<UserDetailReply> GetUserDetail(string UserId)
        {
            UserDetailReply userDetailReply = new UserDetailReply();
            IDocType<PersonalDetailDTO, KYCPersonalDetailActivity> docPerType = null;
            KYCDocFactory kYCDocFactory = new KYCDocFactoryConcrete(_context, UserId, _basePath);
            IDocType<KarzaPANDTO, KYCActivityPAN> docType = kYCDocFactory.GetDocType<KarzaPANDTO, KYCActivityPAN>(Global.Infrastructure.Constants.KYCMasterConstants.PAN);
            IDocType<KarzaAadharDTO, KYCActivityAadhar> aadharDocType = kYCDocFactory.GetDocType<KarzaAadharDTO, KYCActivityAadhar>(Global.Infrastructure.Constants.KYCMasterConstants.Aadhar);
            docPerType = kYCDocFactory.GetDocType<PersonalDetailDTO, KYCPersonalDetailActivity>(Global.Infrastructure.Constants.KYCMasterConstants.PersonalDetail);

            var panDocDetails = await docType.GetDoc(UserId);

            var aadharDocDetails = await aadharDocType.GetDoc(UserId);

            var docDetails1 = await docPerType.GetDoc(UserId);
            var docDetails = docDetails1.Result;

            if (docDetails != null)
            {
                userDetailReply.CustomerName = docDetails.FirstOrDefault(x => x.Key == "FirstName").Value + " "
                                                + docDetails.FirstOrDefault(x => x.Key == "LastName").Value;
                userDetailReply.UserId = UserId;
                userDetailReply.AlternatePhoneNo = docDetails.FirstOrDefault(x => x.Key == "AlternatePhoneNo").Value;
            }
            return userDetailReply;
        }

        public async Task<List<UserListPageInfoReply>> GetUsersListInfo(UserDetailRequest userDetailRequest)
        {
            List<UserListPageInfoReply> userListPageInfoListReply = new List<UserListPageInfoReply>();
            IDocType<PersonalDetailDTO, KYCPersonalDetailActivity> docPerType = null;
            KYCDocFactory kYCDocFactory = new KYCDocFactoryConcrete(_context, userDetailRequest.Usersid.First(), _basePath);

            IDocType<KarzaPANDTO, KYCActivityPAN> docType = kYCDocFactory.GetDocType<KarzaPANDTO, KYCActivityPAN>(Global.Infrastructure.Constants.KYCMasterConstants.PAN);

            docPerType = kYCDocFactory.GetDocType<PersonalDetailDTO, KYCPersonalDetailActivity>(Global.Infrastructure.Constants.KYCMasterConstants.PersonalDetail);

            foreach (var UserId in userDetailRequest.Usersid)
            {
                var panDocDetails1 = await docType.GetDoc(UserId);
                var panDocDetails = panDocDetails1.Result;

                var docDetails1 = await docPerType.GetDoc(UserId);
                var docDetails = docDetails1.Result;

                UserListPageInfoReply userListPageInfoReply = new UserListPageInfoReply();
                if (panDocDetails != null)
                {
                    //kYCPersonalDetailReply.Status = true;
                    userListPageInfoReply.UserId = UserId;
                    userListPageInfoReply.CustomerName = panDocDetails.FirstOrDefault(x => x.Key == KYCDetailConstants.NameOnCard).Value;
                }
                if (docDetails != null)
                {
                    userListPageInfoReply.UserId = UserId;
                    userListPageInfoReply.EmailId = docDetails.FirstOrDefault(x => x.Key == KYCPersonalDetailConstants.EmailId).Value;
                }
                userListPageInfoListReply.Add(userListPageInfoReply);
            }
            return userListPageInfoListReply;
        }
        //public async Task<List<KYCAllInfoResponse>> GetUserLeadInfo(KYCAllInfoRequest kYCAllInfoRequest)
        //{
        //    KYCMasterInfoManager kYCMasterInfoManager = new KYCMasterInfoManager(_context);
        //    var kYCMasterInfo = await kYCMasterInfoManager.GetAllKycInfo(kYCAllInfoRequest);
        //    return kYCMasterInfo;
        //}


        public async Task<Dictionary<string, List<KYCSpecificDetailResponse>>> GetKYCSpecificDetail(GRPCRequest<KYCSpecificDetailRequest> request)
        {
            if (request == null || request.Request == null || request.Request.KYCReqiredFieldList == null || !request.Request.KYCReqiredFieldList.Any())
            {
                return null;
            }

            List<string> masterCodeList = new List<string>(request.Request.KYCReqiredFieldList.Keys);

            List<string> detailCodeList = new List<string>();

            foreach (var item in request.Request.KYCReqiredFieldList)
            {
                if (item.Value != null && item.Value.Any())
                {
                    detailCodeList.AddRange(item.Value);
                }
            }


            var query = from m in _context.KYCMasters
                        join rm in masterCodeList on m.Code equals rm
                        join d in _context.KYCDetails on m.Id equals d.KYCMasterId
                        join rd in detailCodeList on d.Field equals rd
                        where m.IsActive == true
                           && m.IsDeleted == false
                        select d.Id;

            var detailIdList = query.ToList();

            if (detailIdList != null && detailIdList.Any() && request.Request.UserIdList != null && request.Request.UserIdList.Any())
            {
                var userList = request.Request.UserIdList.Select(x => x.UserId + "_" + x.ProductCode).ToList();


                var dataQuery = from m in _context.KYCMasters
                                join rm in masterCodeList on m.Code equals rm
                                join mi in _context.KYCMasterInfos on m.Id equals mi.KYCMasterId
                                //join req in userList on  new { productCode = mi.ProductCode, userId= mi.UserId} equals new { productCode = req.ProductCode, userId = req.UserId }

                                join di in _context.KYCDetailInfos on mi.Id equals di.KYCMasterInfoId
                                join rd in detailIdList on di.KYCDetailId equals rd
                                join d in _context.KYCDetails on di.KYCDetailId equals d.Id
                                join req in userList on mi.UserId + "_" + mi.ProductCode equals req

                                where m.IsActive == true
                                   && m.IsDeleted == false
                                   //&& request.Request.UserIdList.Any(dict => dict.ContainsKey("UserId") && dict["UserId"] == mi.UserId && dict.ContainsKey("ProductCode") && dict["ProductCode"] == mi.ProductCode)
                                   && mi.IsDeleted == false
                                   && mi.IsActive == true
                                select new KYCSpecificDetailResponse
                                {
                                    FieldName = d.Field,
                                    FieldValue = di.FieldValue,
                                    MasterCode = m.Code,
                                    UserId = mi.UserId
                                };
                var data = dataQuery.ToList();

                if (data != null && data.Any())
                {
                    Dictionary<string, List<KYCSpecificDetailResponse>> response = data.GroupBy(x => x.UserId).ToDictionary(group => group.Key, group => group.ToList());
                    return response;
                }
            }

            return null;
        }



        public async Task<GRPCReply<bool>> RemoveSecretInfo(GRPCRequest<string> request)
        {
            var query = from mi in _context.KYCMasterInfos
                        join m in _context.KYCMasters on mi.KYCMasterId equals m.Id
                        where mi.IsActive == true
                           && mi.IsDeleted == false
                           && (m.Code == KYCMasterConstants.PAN || m.Code == KYCMasterConstants.Aadhar)
                           && mi.UserId == request.Request
                        select mi;
            var masterInfoList = query.ToList();

            bool isSave = false;
            if (masterInfoList != null && masterInfoList.Any())
            {
                foreach (var item in masterInfoList)
                {
                    if (!string.IsNullOrEmpty(item.UniqueId) && item.UniqueId.Length > 4)
                    {
                        string initials = new String('X', item.UniqueId.Length - 4);
                        string last = item.UniqueId.Substring(item.UniqueId.Length - 4);
                        item.UniqueId = initials + last;

                        _context.Entry(item).State = EntityState.Modified;

                        isSave = true;
                    }
                }

                var masterInfoIdList = masterInfoList.Select(x => x.Id).ToList();

                var detailQuery = from di in _context.KYCDetailInfos
                                  join d in _context.KYCDetails on di.KYCDetailId equals d.Id
                                  join mi in masterInfoIdList on di.KYCMasterInfoId equals mi
                                  where di.IsActive == true
                                     && di.IsDeleted == false
                                     && (d.Field == KYCDetailConstants.DocumentId || d.Field == KYCAadharConstants.FrontDocumentId || d.Field == KYCAadharConstants.BackDocumentId)
                                     && di.IsActive && !di.IsDeleted

                                  select di;
                var detailList = detailQuery.ToList();

                if (detailList != null && detailList.Count > 0)
                {
                    foreach (var item in detailList)
                    {
                        item.FieldValue = "";
                        _context.Entry(item).State = EntityState.Modified;


                    }
                }
            }

            if (isSave)
            {
                await _context.SaveChangesAsync();
            }

            return new GRPCReply<bool> { Response = true, Status = true, Message = "" };
        }

        public async Task<GRPCReply<bool>> RemoveKYCPersonalInfo(GRPCRequest<string> userid)
        {
            var query = from i in _context.KYCMasterInfos
                        join m in _context.KYCMasters on i.KYCMasterId equals m.Id
                        where m.IsActive == true && m.IsDeleted == false && i.IsActive && !i.IsDeleted && m.Code == KYCMasterConstants.PersonalDetail &&
                        i.UserId == userid.Request
                        select i;

            var data = query.FirstOrDefault();
            if (data != null)
            {
                data.IsActive = false;
                data.IsDeleted = true;
                _context.Entry(data).State = EntityState.Modified;


                var detailList = _context.KYCDetailInfos.Where(x => x.KYCMasterInfoId == data.Id && x.IsActive && !x.IsDeleted).ToList();
                if (detailList != null)
                {
                    foreach (var item in detailList)
                    {
                        item.IsActive = false;
                        item.IsDeleted = true;
                        _context.Entry(item).State = EntityState.Modified;
                    }
                }
                _context.SaveChanges();
            }
            return new GRPCReply<bool> { Response = true, Status = true, Message = "" };
        }


        public async Task<GRPCReply<bool>> RemoveKYCInfoOnReset(GRPCRequest<ResetLeadRequestDc> request)
        {
            var query = from i in _context.KYCMasterInfos
                        join m in _context.KYCMasters on i.KYCMasterId equals m.Id
                        where m.IsActive == true && m.IsDeleted == false && i.IsActive && !i.IsDeleted && i.UserId == request.Request.userid && i.ProductCode == request.Request.ProductCode
                        select i;

            var data = await query.ToListAsync();
            if (data != null && data.Any() && data.Count > 0)
            {
                foreach (var kycinfo in data)
                {
                    kycinfo.IsActive = false;
                    kycinfo.IsDeleted = true;
                    _context.Entry(kycinfo).State = EntityState.Modified;


                    var detailList = _context.KYCDetailInfos.Where(x => x.KYCMasterInfoId == kycinfo.Id && x.IsActive && !x.IsDeleted).ToList();
                    if (detailList != null)
                    {
                        foreach (var item in detailList)
                        {
                            item.IsActive = false;
                            item.IsDeleted = true;
                            _context.Entry(item).State = EntityState.Modified;
                        }
                    }
                }
                _context.SaveChanges();
            }
            return new GRPCReply<bool> { Response = true, Status = true, Message = "" };
        }
        [AllowAnonymous]
        public async Task<GRPCReply<bool>> UpdateAddress(GRPCRequest<UpdateAddressRequest> updateAddressRequest)
        {
            var query = from i in _context.KYCMasterInfos
                        join m in _context.KYCMasters on i.KYCMasterId equals m.Id
                        where m.IsActive == true && m.IsDeleted == false && i.IsActive && !i.IsDeleted && m.Code == updateAddressRequest.Request.AddressType &&
                        i.UserId == updateAddressRequest.Request.UserId && i.ProductCode == updateAddressRequest.Request.ProductCode
                        select i;

            var data = query.FirstOrDefault();
            if (data != null)
            {
                KYCDetail detailList = new KYCDetail();
                detailList = _context.KYCDetails.Where(x => x.Field == (updateAddressRequest.Request.AddressType == "PersonalDetail" ? KYCPersonalDetailConstants.CurrentAddressId : KYCBuisnessDetailConstants.CurrentAddressId) && x.KYCMasterId == data.KYCMasterId && x.IsActive && !x.IsDeleted).FirstOrDefault();
                var detailInfoList = _context.KYCDetailInfos.Where(x => x.KYCMasterInfoId == data.Id && x.KYCDetailId == detailList.Id && x.IsActive && !x.IsDeleted).FirstOrDefault();
                if (detailInfoList != null)
                {
                    detailInfoList.IsActive = false;
                    detailInfoList.IsDeleted = true;
                    _context.Entry(detailInfoList).State = EntityState.Modified;
                }
                KYCDetailInfo kYCDetailInfo = new KYCDetailInfo
                {
                    KYCDetailId = detailList.Id,
                    KYCMasterInfoId = data.Id,
                    FieldValue = updateAddressRequest.Request.CurrentAddressId.ToString(),
                    IsActive = true,
                    IsDeleted = false,
                    Created = DateTime.Now,
                    CreatedBy = updateAddressRequest.Request.UserId
                };
                _context.KYCDetailInfos.Add(kYCDetailInfo);

                _context.SaveChanges();
            }
            return new GRPCReply<bool> { Response = true, Status = true, Message = "" };
        }

        [AllowAnonymous]
        public async Task<GRPCReply<bool>> UpdateBuisnessDetail(GRPCRequest<UpdateBuisnessDetailRequest> updateBuisnessDetailRequest)
        {
            var query = from i in _context.KYCMasterInfos
                        join m in _context.KYCMasters on i.KYCMasterId equals m.Id
                        where m.IsActive == true && m.IsDeleted == false && i.IsActive && !i.IsDeleted && m.Code == KYCMasterConstants.BuisnessDetail &&
                        i.UserId == updateBuisnessDetailRequest.Request.UserId && i.ProductCode == updateBuisnessDetailRequest.Request.ProductCode
                        select i;

            var data = query.FirstOrDefault();
            if (data != null)
            {
                List<KYCDetail> detailList = new List<KYCDetail>();
                List<KYCDetailInfo> kYCDetailInfos = new List<KYCDetailInfo>();
                List<long> Ids = null;
                detailList = _context.KYCDetails.Where(x => (x.Field != KYCBuisnessDetailConstants.BuisnessProof && x.Field != KYCBuisnessDetailConstants.BuisnessProofDocId) && x.KYCMasterId == data.KYCMasterId && x.IsActive && !x.IsDeleted).ToList();
                Ids = detailList.Select(x => x.Id).ToList();
                var detailInfoList = _context.KYCDetailInfos.Where(x => Ids.Contains(x.KYCDetailId) && x.KYCMasterInfoId == data.Id && x.IsActive && !x.IsDeleted).ToList();
                if (detailInfoList != null && detailInfoList.Any())
                {
                    foreach (var detail in detailInfoList)
                    {
                        detail.IsActive = false;
                        detail.IsDeleted = true;
                        _context.Entry(detail).State = EntityState.Modified;
                    }
                    _context.SaveChanges();

                }


                foreach (var item in detailList)
                {
                    KYCDetailInfo kYCDetailInfo = new KYCDetailInfo
                    {
                        KYCDetailId = item.Id,
                        KYCMasterInfoId = data.Id,
                        IsActive = true,
                        IsDeleted = false,
                        Created = DateTime.Now,
                        CreatedBy = updateBuisnessDetailRequest.Request.UserId
                    };
                    switch (item.Field)
                    {

                        case Global.Infrastructure.Constants.KYCBuisnessDetailConstants.BusinessName:
                            kYCDetailInfo.FieldValue = updateBuisnessDetailRequest.Request.BusName ?? "";
                            break;
                        case Global.Infrastructure.Constants.KYCBuisnessDetailConstants.DOI:
                            kYCDetailInfo.FieldValue = updateBuisnessDetailRequest.Request.DOI ?? "";
                            break;
                        case Global.Infrastructure.Constants.KYCBuisnessDetailConstants.BusGSTNO:
                            kYCDetailInfo.FieldValue = updateBuisnessDetailRequest.Request.BusGSTNO ?? "";
                            break;
                        case Global.Infrastructure.Constants.KYCBuisnessDetailConstants.BusEntityType:
                            kYCDetailInfo.FieldValue = updateBuisnessDetailRequest.Request.BusEntityType ?? "";
                            break;
                        case Global.Infrastructure.Constants.KYCBuisnessDetailConstants.BuisnessMonthlySalary:
                            kYCDetailInfo.FieldValue = updateBuisnessDetailRequest.Request.BuisnessMonthlySalary.HasValue ? updateBuisnessDetailRequest.Request.BuisnessMonthlySalary.ToString() : "";
                            break;
                        case Global.Infrastructure.Constants.KYCBuisnessDetailConstants.IncomeSlab:
                            kYCDetailInfo.FieldValue = updateBuisnessDetailRequest.Request.IncomeSlab ?? "";
                            break;
                        case Global.Infrastructure.Constants.KYCBuisnessDetailConstants.BuisnessDocumentNo:
                            kYCDetailInfo.FieldValue = updateBuisnessDetailRequest.Request.BuisnessDocumentNo ?? "";
                            break;
                        case Global.Infrastructure.Constants.KYCBuisnessDetailConstants.InquiryAmount:
                            kYCDetailInfo.FieldValue = updateBuisnessDetailRequest.Request.InquiryAmount.HasValue ? updateBuisnessDetailRequest.Request.InquiryAmount.ToString() : "0";
                            break;
                        case Global.Infrastructure.Constants.KYCBuisnessDetailConstants.SurrogateType:
                            kYCDetailInfo.FieldValue = updateBuisnessDetailRequest.Request.SurrogateType ?? "";
                            break;
                        case Global.Infrastructure.Constants.KYCBuisnessDetailConstants.CurrentAddressId:
                            kYCDetailInfo.FieldValue = updateBuisnessDetailRequest.Request.CurrentAddressId.HasValue ? updateBuisnessDetailRequest.Request.CurrentAddressId.ToString() : "";
                            break;
                        default:
                            break;
                    }
                    kYCDetailInfos.Add(kYCDetailInfo);
                }
                await _context.AddRangeAsync(kYCDetailInfos);
                _context.SaveChanges();
            }
            return new GRPCReply<bool> { Response = true, Status = true, Message = "Updated Successfully!!" };
        }


        public async Task<GRPCReply<bool>> UploadLeadDocuments(GRPCRequest<UpdateLeadDocumentDetailRequest> updateLeadDocumentDetailRequest)
        {
            string masterConstants = "";
            string docName = "";
            if (updateLeadDocumentDetailRequest.Request.ProductCode == "CreditLine")
            {
                if (updateLeadDocumentDetailRequest.Request.DocumentName == "gst_certificate" || updateLeadDocumentDetailRequest.Request.DocumentName == "udyog_aadhaar" || updateLeadDocumentDetailRequest.Request.DocumentName == "other")
                {
                    masterConstants = KYCMasterConstants.BuisnessDetail;
                    if (updateLeadDocumentDetailRequest.Request.DocumentName == "gst_certificate" || updateLeadDocumentDetailRequest.Request.DocumentName == "other")
                    {
                        docName = updateLeadDocumentDetailRequest.Request.DocumentName == "gst_certificate" ? "GST Certificate" : "Others";
                    }
                    else
                    {
                        docName = "Udyog Aadhaar Certificate";
                    }
                }
            }
            if (updateLeadDocumentDetailRequest.Request.ProductCode == "BusinessLoan")
            {
                if (updateLeadDocumentDetailRequest.Request.DocumentName == "gst_certificate" || updateLeadDocumentDetailRequest.Request.DocumentName == "other")
                {
                    masterConstants = KYCMasterConstants.BuisnessDetail;
                    docName = updateLeadDocumentDetailRequest.Request.DocumentName == "gst_certificate" ? "GST Certificate" : "Others";
                }
                if (updateLeadDocumentDetailRequest.Request.DocumentName == "udyog_aadhaar")
                {
                    masterConstants = KYCMasterConstants.MSME;
                    docName = "Udyog Aadhaar Certificate";
                }
            }
            var query = from i in _context.KYCMasterInfos
                        join m in _context.KYCMasters on i.KYCMasterId equals m.Id
                        where m.IsActive == true && m.IsDeleted == false && i.IsActive && !i.IsDeleted && m.Code == masterConstants &&
                        i.UserId == updateLeadDocumentDetailRequest.Request.UserId && i.ProductCode == updateLeadDocumentDetailRequest.Request.ProductCode
                        select i;

            var data = query.FirstOrDefault();
            if (data != null)
            {
                List<KYCDetail> detailList = new List<KYCDetail>();
                List<KYCDetailInfo> kYCDetailInfos = new List<KYCDetailInfo>();
                if (masterConstants == KYCMasterConstants.BuisnessDetail)
                {
                    var docIdData = await _context.KYCDetails.Where(x => x.Field == KYCBuisnessDetailConstants.BuisnessProofDocId && x.KYCMasterId == data.KYCMasterId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
                    var proofData = await _context.KYCDetails.Where(x => x.Field == KYCBuisnessDetailConstants.BuisnessProof && x.KYCMasterId == data.KYCMasterId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
                    detailList.Add(docIdData);
                    detailList.Add(proofData);
                }
                if (masterConstants == KYCMasterConstants.MSME)
                {
                    detailList = await _context.KYCDetails.Where(x => x.Field == KYCMSMEConstants.FrontDocumentId && x.KYCMasterId == data.KYCMasterId && x.IsActive && !x.IsDeleted).ToListAsync();
                }
                List<long> Ids = detailList.Select(x => x.Id).ToList();
                var detailInfo = await _context.KYCDetailInfos.Where(x => Ids.Contains(x.KYCDetailId) && x.KYCMasterInfoId == data.Id && x.IsActive && !x.IsDeleted).ToListAsync();
                if (detailInfo != null && detailInfo.Any())
                {
                    foreach (var detail in detailInfo)
                    {
                        detail.IsActive = false;
                        detail.IsDeleted = true;
                        _context.Entry(detail).State = EntityState.Modified;
                    }
                    _context.SaveChanges();

                    foreach (var item in detailList)
                    {
                        KYCDetailInfo kYCDetailInfo = new KYCDetailInfo
                        {
                            KYCDetailId = item.Id,
                            KYCMasterInfoId = data.Id,
                            IsActive = true,
                            IsDeleted = false,
                            Created = DateTime.Now,
                            CreatedBy = updateLeadDocumentDetailRequest.Request.UserId
                        };
                        if (masterConstants == KYCMasterConstants.BuisnessDetail)
                        {
                            switch (item.Field)
                            {

                                case Global.Infrastructure.Constants.KYCBuisnessDetailConstants.BuisnessProof:
                                    kYCDetailInfo.FieldValue = docName ?? "";
                                    break;
                                case Global.Infrastructure.Constants.KYCBuisnessDetailConstants.BuisnessProofDocId:
                                    kYCDetailInfo.FieldValue = updateLeadDocumentDetailRequest.Request.DocId ?? "";
                                    break;
                                default:
                                    break;
                            }
                            kYCDetailInfos.Add(kYCDetailInfo);
                        }
                        else if (masterConstants == KYCMasterConstants.MSME)
                        {
                            switch (item.Field)
                            {

                                case Global.Infrastructure.Constants.KYCMSMEConstants.FrontDocumentId:
                                    kYCDetailInfo.FieldValue = updateLeadDocumentDetailRequest.Request.DocId ?? "";
                                    break;
                                default:
                                    break;
                            }
                            kYCDetailInfos.Add(kYCDetailInfo);
                        }
                        else
                        {
                            switch (item.Field)
                            {

                                case Global.Infrastructure.Constants.KYCPersonalDetailConstants.ElectricityBillDocumentId:
                                    kYCDetailInfo.FieldValue = updateLeadDocumentDetailRequest.Request.DocId ?? "";
                                    break;
                                default:
                                    break;
                            }
                            kYCDetailInfos.Add(kYCDetailInfo);
                        }

                    }
                    await _context.AddRangeAsync(kYCDetailInfos);

                }
                _context.SaveChanges();
            }
            return new GRPCReply<bool> { Response = true, Status = true, Message = "Updated Successfully!!" };
        }


        public async Task<GRPCReply<bool>> UploadMultiLeadDocuments(GRPCRequest<UpdateLeadDocumentDetailListRequest> updateLeadDocumentDetailRequest)
        {
            if (updateLeadDocumentDetailRequest.Request.DocumentName == BlackSoilBusinessDocNameConstants.GstCertificate || updateLeadDocumentDetailRequest.Request.DocumentName == BlackSoilBusinessDocNameConstants.Other || updateLeadDocumentDetailRequest.Request.DocumentName == BlackSoilBusinessDocNameConstants.UdyogAadhaar)
            {
                if (updateLeadDocumentDetailRequest.Request.DocList.Count > 1)
                    return new GRPCReply<bool> { Response = false, Status = false, Message = "Can not upload Multiple document for " + updateLeadDocumentDetailRequest.Request.DocumentName };
            }
            string masterConstants = "";
            string docName = "";
            if (updateLeadDocumentDetailRequest.Request.ProductCode == "CreditLine")
            {
                if (updateLeadDocumentDetailRequest.Request.DocumentName == BlackSoilBusinessDocNameConstants.GstCertificate || updateLeadDocumentDetailRequest.Request.DocumentName == BlackSoilBusinessDocNameConstants.UdyogAadhaar || updateLeadDocumentDetailRequest.Request.DocumentName == BlackSoilBusinessDocNameConstants.Other)
                {
                    masterConstants = KYCMasterConstants.BuisnessDetail;
                    if (updateLeadDocumentDetailRequest.Request.DocumentName == BlackSoilBusinessDocNameConstants.GstCertificate || updateLeadDocumentDetailRequest.Request.DocumentName == BlackSoilBusinessDocNameConstants.Other)
                    {
                        docName = updateLeadDocumentDetailRequest.Request.DocumentName == BlackSoilBusinessDocNameConstants.GstCertificate ? "GST Certificate" : "Others";
                    }
                    else
                    {
                        docName = "Udyog Aadhaar Certificate";
                    }
                }
                else if (updateLeadDocumentDetailRequest.Request.DocumentName == BlackSoilBusinessDocNameConstants.SurrogateITRCertificate || updateLeadDocumentDetailRequest.Request.DocumentName == BlackSoilBusinessDocNameConstants.SurrogateGstCertificate)
                {
                    masterConstants = KYCMasterConstants.BuisnessDetail;
                }
                else if (updateLeadDocumentDetailRequest.Request.DocumentName == "Address Proof")
                {
                    masterConstants = KYCMasterConstants.PersonalDetail;
                }
            }
            if (updateLeadDocumentDetailRequest.Request.ProductCode == "BusinessLoan")
            {
                if (updateLeadDocumentDetailRequest.Request.DocumentName == BlackSoilBusinessDocNameConstants.GstCertificate || updateLeadDocumentDetailRequest.Request.DocumentName == BlackSoilBusinessDocNameConstants.Other)
                {
                    masterConstants = KYCMasterConstants.BuisnessDetail;
                    docName = updateLeadDocumentDetailRequest.Request.DocumentName == BlackSoilBusinessDocNameConstants.GstCertificate ? "GST Certificate" : "Others";
                }
                if (updateLeadDocumentDetailRequest.Request.DocumentName == BlackSoilBusinessDocNameConstants.UdyogAadhaar)
                {
                    masterConstants = KYCMasterConstants.MSME;
                    docName = "Udyog Aadhaar Certificate";
                }
                if (updateLeadDocumentDetailRequest.Request.DocumentName == BlackSoilBusinessDocNameConstants.SurrogateITRCertificate || updateLeadDocumentDetailRequest.Request.DocumentName == BlackSoilBusinessDocNameConstants.SurrogateGstCertificate)
                {
                    masterConstants = KYCMasterConstants.BankStatementCreditLending;
                }
                if (updateLeadDocumentDetailRequest.Request.DocumentName == "Address Proof")
                {
                    masterConstants = KYCMasterConstants.PersonalDetail;
                }
            }

            var query = from i in _context.KYCMasterInfos
                        join m in _context.KYCMasters on i.KYCMasterId equals m.Id
                        where m.IsActive == true && m.IsDeleted == false && i.IsActive && !i.IsDeleted && m.Code == masterConstants &&
                        i.UserId == updateLeadDocumentDetailRequest.Request.UserId && i.ProductCode == updateLeadDocumentDetailRequest.Request.ProductCode
                        select i;

            var data = query.FirstOrDefault();
            if (data != null)
            {
                List<KYCDetail> detailList = new List<KYCDetail>();
                List<KYCDetailInfo> kYCDetailInfos = new List<KYCDetailInfo>();
                if (masterConstants == KYCMasterConstants.PersonalDetail)
                {
                    var docIdData = await _context.KYCDetails.Where(x => x.Field == KYCPersonalDetailConstants.ElectricityBillDocumentId && x.KYCMasterId == data.KYCMasterId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
                    detailList.Add(docIdData);
                }
                if (masterConstants == KYCMasterConstants.BuisnessDetail)
                {
                        var docIdData = await _context.KYCDetails.Where(x => x.Field == KYCBuisnessDetailConstants.BuisnessProofDocId && x.KYCMasterId == data.KYCMasterId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
                        var proofData = await _context.KYCDetails.Where(x => x.Field == KYCBuisnessDetailConstants.BuisnessProof && x.KYCMasterId == data.KYCMasterId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
                        detailList.Add(docIdData);
                        detailList.Add(proofData);
                }
                if (masterConstants == KYCMasterConstants.BankStatementCreditLending)
                {
                    var SurrodocIdData = await _context.KYCDetails.Where(x => x.Field == KYCBankStatementCreditLendingConstant.SarrogateDocId && x.KYCMasterId == data.KYCMasterId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
                    detailList.Add(SurrodocIdData);
                }
                if (masterConstants == KYCMasterConstants.MSME)
                {
                    detailList = await _context.KYCDetails.Where(x => x.Field == KYCMSMEConstants.FrontDocumentId && x.KYCMasterId == data.KYCMasterId && x.IsActive && !x.IsDeleted).ToListAsync();
                }
                List<long> Ids = detailList.Select(x => x.Id).ToList();
                var detailInfo = await _context.KYCDetailInfos.Where(x => Ids.Contains(x.KYCDetailId) && x.KYCMasterInfoId == data.Id && x.IsActive && !x.IsDeleted).ToListAsync();
                if (detailInfo != null && detailInfo.Any())
                {
                    foreach (var detail in detailInfo)
                    {
                        detail.IsActive = false;
                        detail.IsDeleted = true;
                        _context.Entry(detail).State = EntityState.Modified;
                    }
                    //_context.SaveChanges();
                }

                //foreach (var docDoc in updateLeadDocumentDetailRequest.Request.DocList)
                //{
                    var DocIdsList = updateLeadDocumentDetailRequest.Request.DocList.Select(x => x.DocId).ToList();
                    foreach (var item in detailList)
                    {
                        KYCDetailInfo kYCDetailInfo = new KYCDetailInfo
                        {
                            KYCDetailId = item.Id,
                            KYCMasterInfoId = data.Id,
                            IsActive = true,
                            IsDeleted = false,
                            Created = DateTime.Now,
                            CreatedBy = updateLeadDocumentDetailRequest.Request.UserId
                        };
                        if (masterConstants == KYCMasterConstants.BuisnessDetail)
                        {                          
                            switch (item.Field)
                            {
                                case Global.Infrastructure.Constants.KYCBuisnessDetailConstants.BuisnessProof:
                                    kYCDetailInfo.FieldValue = docName ?? "";
                                    break;
                                case Global.Infrastructure.Constants.KYCBuisnessDetailConstants.BuisnessProofDocId:
                                    kYCDetailInfo.FieldValue = string.Join(",",DocIdsList) ?? "";
                                    break;
                                default:
                                    break;
                            }
                            kYCDetailInfos.Add(kYCDetailInfo);
                        }
                        else if (masterConstants == KYCMasterConstants.PersonalDetail)
                        {
                            switch (item.Field)
                            {
                                case Global.Infrastructure.Constants.KYCPersonalDetailConstants.ElectricityBillDocumentId:
                                    kYCDetailInfo.FieldValue = string.Join(",", DocIdsList) ?? "";
                                    break;
                                default:
                                    break;
                            }
                            kYCDetailInfos.Add(kYCDetailInfo);
                        }
                        else if (masterConstants == KYCMasterConstants.MSME)
                        {
                            switch (item.Field)
                            {

                                case Global.Infrastructure.Constants.KYCMSMEConstants.FrontDocumentId:
                                    kYCDetailInfo.FieldValue = string.Join(",", DocIdsList) ?? "";
                                    break;
                                default:
                                    break;
                            }
                            kYCDetailInfos.Add(kYCDetailInfo);
                        }
                        else if (masterConstants == KYCMasterConstants.BankStatementCreditLending)
                        {
                            switch (item.Field)
                            {
                                case Global.Infrastructure.Constants.KYCBankStatementCreditLendingConstant.SarrogateDocId:
                                    kYCDetailInfo.FieldValue = string.Join(",", DocIdsList) ?? "";
                                    break;
                                default:
                                    break;
                            }
                            kYCDetailInfos.Add(kYCDetailInfo);
                        }
                        else
                        {
                            switch (item.Field)
                            {

                                case Global.Infrastructure.Constants.KYCPersonalDetailConstants.ElectricityBillDocumentId:
                                    kYCDetailInfo.FieldValue = string.Join(",", DocIdsList) ?? "";
                                    break;
                                default:
                                    break;
                            }
                            kYCDetailInfos.Add(kYCDetailInfo);
                        }

                    //}
                }
                await _context.AddRangeAsync(kYCDetailInfos);
                _context.SaveChanges();
            }
            return new GRPCReply<bool> { Response = true, Status = true, Message = "Updated Successfully!!" };
        }
        
    }
}
