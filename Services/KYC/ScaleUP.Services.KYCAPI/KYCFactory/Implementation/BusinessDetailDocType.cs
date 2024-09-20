using Microsoft.EntityFrameworkCore;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.KYCActivity;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Services.KYCAPI.Persistence;
using ScaleUP.Services.KYCDTO.Constant;
using ScaleUP.Services.KYCDTO.Enum;
using ScaleUP.Services.KYCDTO.Transacion;
using ScaleUP.Services.KYCModels.Master;
using ScaleUP.Services.KYCModels.Transaction;
using System.Collections.Generic;

namespace ScaleUP.Services.KYCAPI.KYCFactory.Implementation
{
    public class BusinessDetailDocType : BaseDocType, IDocType<BusinessDetailDTO, BusinessActivityDetail>
    {
        public BusinessDetailDocType(ApplicationDbContext context, string userId, string filePath) : base(context, userId, filePath)
        {
            docTypeCode = KYCMasterConstants.BuisnessDetail;
        }

        public async Task<ResultViewModel<long>> GetAndSaveByUniqueId(BusinessActivityDetail input, string userId ,string CreatedBy, string productCode)
        {
            ResultViewModel<long> result = new ResultViewModel<long>();

            try
            {
                ResultViewModel<long> res = null;
                BusinessDetailDTO businessDetailDTO = new BusinessDetailDTO
                {
                    //BusAddCorrCity = input.BusAddCorrCity,
                    //BusAddCorrLine1 = input.BusAddCorrLine1,
                    //BusAddCorrLine2 = input.BusAddCorrLine2,
                    //BusAddCorrPincode = input.BusAddCorrPincode,
                    //BusAddCorrState = input.BusAddCorrState,
                    //BusAddPerCity = input.BusAddPerCity,
                    //BusAddPerLine1 = input.BusAddPerLine1,
                    //BusAddPerLine2 = input.BusAddPerLine2,
                    //BusAddPerPincode = input.BusAddPerPincode,
                    //BusAddPerState = input.BusAddPerState,
                    BusEntityType = input.BusEntityType,
                    BusGSTNO = input.BusGSTNO,
                    BusName = input.BusName,
                    //BusPan = input.BusPan,
                    DOI = input.DOI,
                    LeadMasterId = input.LeadMasterId,
                    CurrentAddressId = input.CurentLocationId,
                    PermanentAddressId = input.PermanentLocationId,
                    BuisnessMonthlySalary = input.BuisnessMonthlySalary,
                    IncomeSlab = input.IncomeSlab,

                    BuisnessProofDocId = input.BuisnessProofDocId,
                    BuisnessProof = input.BuisnessProof,
                    BuisnessDocumentNo = input.BuisnessDocumentNo,
                    InquiryAmount = input.InquiryAmount,
                    SurrogateType = input.SurrogateType
                    //SequenceNo = input.SequenceNo,
                    //BusEstablishmentProofType = "Udhyog Adhaar"
                };
                res = await SaveDoc(businessDetailDTO, userId, CreatedBy, productCode);
                if (res != null && input.BusEntityType != "Proprietorship" && res.IsSuccess == true)
                {
                    result = new ResultViewModel<long>
                    {
                        IsSuccess = false,
                        Message = "Process block as bussiness entity is not type is Proprietorship.",
                        Result = res.Result
                    };
                    return result;
                }
                result = res;
            }
            catch (Exception ex)
            {
                result = new ResultViewModel<long>
                {
                    IsSuccess = false,
                    Message = "Fail to save",
                    Result = 0
                };
            }
            return result;
        }

        public Task<ResultViewModel<BusinessDetailDTO>> GetByUniqueId(BusinessActivityDetail input)
        {
            throw new NotImplementedException();
        }

        //public async Task<Dictionary<string, dynamic>> GetDoc(string userId)
        //{
        //    //var masterInfo = Context.KYCMasterInfos.Where(x => x.UserId == userId && x.IsActive == true && x.IsDeleted == false).OrderByDescending(y => y.LastModified).FirstOrDefault();
        //    var kycTypeId = Context.KYCMasters.Where(x => x.Code == KYCMasterConstants.BuisnessDetail).Select(x => x.Id).First();

        //    var masterInfo = Context.KYCMasterInfos.Where(x => x.UserId == userId && x.KYCMasterId == kycTypeId && x.IsActive == true && x.IsDeleted == false).Include(y => y.KYCDetailInfoList)
        //        .ThenInclude(z => z.KYCDetail).OrderByDescending(y => y.LastModified).FirstOrDefault();


        //    if (masterInfo != null)
        //    {
        //        var master = await Context.KYCMasters.Where(x => x.Id == masterInfo.KYCMasterId).FirstOrDefaultAsync();

        //        var kycDetail = await Context.KYCDetails.Where(x => x.KYCMasterId == master.Id).ToListAsync();
        //        var kycDetailInfo = await Context.KYCDetailInfos.Where(x => x.KYCMasterInfoId == masterInfo.Id).ToListAsync();

        //        //foreach(var detail in kycDetail)
        //        //{
        //        //    KYCDetailInfo kYCDetailInfo = new KYCDetailInfo()
        //        //    {
        //        //        KYCDetail = detail
        //        //    };
        //        //    masterInfo.KYCDetailInfoList = kycDetailInfo;
        //        //    //masterInfo.KYCDetailInfoList.Add(kYCDetailInfo);
        //        //}
        //        //foreach (var detail in kycDetailInfo)
        //        //{
        //        //    KYCDetailInfo kYCDetailInfo = new KYCDetailInfo()
        //        //    {
        //        //        FieldValue = detail.FieldValue
        //        //    };
        //        //    masterInfo.KYCDetailInfoList = kycDetailInfo;
        //        //    //masterInfo.KYCDetailInfoList.Add(kYCDetailInfo);
        //        //}



        //        if (masterInfo.LastModified.Value.AddDays(master.ValidityDays) < DateTime.Today)
        //        {
        //            masterInfo = null;
        //        }
        //    }

        //    if (masterInfo != null)
        //    {
        //        Dictionary<string, dynamic> keyValuePairsBusDoc = new Dictionary<string, dynamic>();
        //        keyValuePairsBusDoc.Add("userId", userId);
        //        keyValuePairsBusDoc.Add("UniqueId", masterInfo.UniqueId);
        //        foreach (var item in masterInfo.KYCDetailInfoList)
        //        {
        //            switch (item.KYCDetail.FieldType)
        //            {
        //                case (int)FieldTypeEnum.DateTime:
        //                    keyValuePairsBusDoc.Add(item.KYCDetail.Field, DateTime.Parse(item.FieldValue));
        //                    break;
        //                case (int)FieldTypeEnum.Double:
        //                    keyValuePairsBusDoc.Add(item.KYCDetail.Field, double.Parse(item.FieldValue));
        //                    break;
        //                case (int)FieldTypeEnum.String:
        //                    keyValuePairsBusDoc.Add(item.KYCDetail.Field, item.FieldValue);
        //                    break;
        //                case (int)FieldTypeEnum.Integer:
        //                    keyValuePairsBusDoc.Add(item.KYCDetail.Field, Int32.Parse(item.FieldValue));
        //                    break;
        //                case (int)FieldTypeEnum.Boolean:
        //                    keyValuePairsBusDoc.Add(item.KYCDetail.Field, bool.Parse(item.FieldValue));
        //                    break;
        //                default:
        //                    break;
        //            }

        //        }
        //        return keyValuePairsBusDoc;
        //    }
        //    return null;
        //    //throw new NotImplementedException();
        //}

        public ResultViewModel<bool> IsValidTOSave(BusinessDetailDTO doc, KYCMaster kycMaster, string userId, string productCode)
        {
            ResultViewModel<bool> res = null;
            var query = from m in Context.KYCMasters
                        join mi in Context.KYCMasterInfos on m.Id equals mi.KYCMasterId
                        join kd in Context.KYCDetailInfos on mi.Id equals kd.KYCMasterInfoId
                        join d in Context.KYCDetails on kd.KYCDetailId equals d.Id
                        where ((d.Field == KYCBuisnessDetailConstants.BuisnessProof && kd.FieldValue == doc.BuisnessProof) && (d.Field == KYCBuisnessDetailConstants.BuisnessDocumentNo && kd.FieldValue == doc.BuisnessDocumentNo))
                            && mi.IsActive == true
                            && mi.ProductCode == productCode
                            && mi.IsDeleted == false
                            && m.Code == this.docTypeCode
                            && mi.UserId !=_userId
                            && kd.IsActive == true && kd.IsDeleted == false
                            && d.IsActive == true && d.IsDeleted == false
                        select mi;
            var masterInfo = query.ToList();
            if (string.IsNullOrEmpty(doc.BusGSTNO) && doc.BuisnessProof == "GST Certificate")
            {
                res = new ResultViewModel<bool>
                {
                    IsSuccess = false,
                    Message = "Not found!!",
                    Result = true
                };
                return res;
            }
            if (string.IsNullOrEmpty(doc.BuisnessDocumentNo) && doc.BuisnessProof == "Others")
            {
                res = new ResultViewModel<bool>
                {
                    IsSuccess = false,
                    Message = "Not found!!",
                    Result = true
                };
                return res;
            }
            if (string.IsNullOrEmpty(doc.BuisnessDocumentNo) && doc.BuisnessProof != "Others")
            {
                res = new ResultViewModel<bool>
                {
                    IsSuccess = false,
                    Message = "Buisness Document No Not found for " + doc.BuisnessProof + " !!",
                    Result = false
                };
                return res;
            }
            else if (masterInfo != null && masterInfo.Count>=2)
            {
                res = new ResultViewModel<bool>
                {
                    IsSuccess = false,
                    Message = "GST already exists",
                    Result = false
                };
                return res;
            }
            else
            {
                res = new ResultViewModel<bool>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Result = true
                };
            }
            return res;
        }

        public async Task<ResultViewModel<long>> SaveDoc(BusinessDetailDTO doc, string userId, string CreatedBy, string productCode)
        {
            ResultViewModel<long> res = null;

            var BussinessId = Context.KYCMasters.Where(x => x.IsActive == true && x.IsDeleted == false && x.Code == KYCMasterConstants.BuisnessDetail.ToString()).First();
            var thismasterInfo = await Context.KYCMasterInfos
                                    .Include("KYCDetailInfoList")
                                    .Where(x => x.UserId == userId && x.IsActive == true && x.IsDeleted == false && x.KYCMasterId == BussinessId.Id && x.ProductCode == productCode)
                                    .FirstOrDefaultAsync();

            var kycDetailQuery = from m in Context.KYCMasters
                                 join d in Context.KYCDetails on m.Id equals d.KYCMasterId
                                 where d.IsActive == true && d.IsDeleted == false
                                 && m.Code == Global.Infrastructure.Constants.KYCMasterConstants.BuisnessDetail
                                 select d;
            var kycDetailList = kycDetailQuery.ToList();

            var kycMaster = Context.KYCMasters.Where(x => x.Code == Global.Infrastructure.Constants.KYCMasterConstants.BuisnessDetail && x.IsActive == true && x.IsDeleted == false).First();

            ResultViewModel<bool> isValid = IsValidTOSave(doc, kycMaster, userId, productCode);
            if (isValid.Result)
            {
                //await RemoveDocInfo(userId);
                KYCMasterInfo kYCMasterInfo = null;
                if (thismasterInfo == null)
                {
                    kYCMasterInfo = new KYCMasterInfo
                    {
                        LastModified = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false,
                        ResponseJSON = Newtonsoft.Json.JsonConvert.SerializeObject(doc),
                        UniqueId = doc.LeadMasterId.ToString(),
                        UserId = userId,
                        KYCMasterId = kycMaster.Id,
                        Created = DateTime.Now,
                        KYCDetailInfoList = new List<KYCDetailInfo>(),
                        CreatedBy = CreatedBy,
                        ProductCode = productCode,
                    };
                }
                else
                {
                    kYCMasterInfo = thismasterInfo;
                    kYCMasterInfo.LastModified = DateTime.Now;
                    kYCMasterInfo.ResponseJSON = Newtonsoft.Json.JsonConvert.SerializeObject(doc);
                    kYCMasterInfo.UniqueId = doc.LeadMasterId.ToString();
                    kYCMasterInfo.LastModifiedBy = CreatedBy;
                    kYCMasterInfo.ProductCode = productCode;

                    if (kYCMasterInfo.KYCDetailInfoList == null)
                    {
                        kYCMasterInfo.KYCDetailInfoList = new List<KYCDetailInfo>();
                    }
                }
                foreach (var item in kycDetailList)
                {
                    var kycDetailInfo = GetDetail(doc, item);
                    kycDetailInfo.KYCMasterInfoId = kYCMasterInfo.Id;

                    if (kYCMasterInfo.KYCDetailInfoList.Any(x => x.KYCDetailId == kycDetailInfo.KYCDetailId))
                    {
                        var kycDetailInfoTemp = kYCMasterInfo.KYCDetailInfoList.First(x => x.KYCDetailId == kycDetailInfo.KYCDetailId);
                        kycDetailInfoTemp.FieldValue = kycDetailInfo.FieldValue;
                        kycDetailInfoTemp.IsActive = true;
                        kycDetailInfoTemp.IsDeleted = false;
                        kycDetailInfoTemp.LastModifiedBy = CreatedBy;
                    }
                    else
                    {
                        kYCMasterInfo.KYCDetailInfoList.Add(kycDetailInfo);
                        kYCMasterInfo.CreatedBy = CreatedBy;
                    }
                }
                if (kYCMasterInfo.Id == 0)
                {
                    Context.KYCMasterInfos.Add(kYCMasterInfo);
                }
                else
                {
                    Context.Entry(kYCMasterInfo).State = EntityState.Modified;
                }
                await Context.SaveChangesAsync();
                res = new ResultViewModel<long>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Result = kYCMasterInfo.Id
                };
            }
            else
            {
                res = new ResultViewModel<long>
                {
                    IsSuccess = false,
                    Message = isValid.Message,
                    Result = 0
                };
            }
            return res;
        }

        private KYCDetailInfo GetDetail(BusinessDetailDTO doc, KYCDetail kYCDetail)
        {
            KYCDetailInfo kYCDetailInfo = new KYCDetailInfo
            {
                KYCDetailId = kYCDetail.Id,
                IsActive = true,
                IsDeleted = false
            };

            switch (kYCDetail.Field)
            {

                case Global.Infrastructure.Constants.KYCBuisnessDetailConstants.BusinessName:
                    kYCDetailInfo.FieldValue = doc.BusName ?? "";
                    break;
                //case KYCBuisnessDetailConstants.BusName:
                //    kYCDetailInfo.FieldValue = doc.BusName ?? "";
                //    break;
                case Global.Infrastructure.Constants.KYCBuisnessDetailConstants.DOI:
                    kYCDetailInfo.FieldValue = doc.DOI ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCBuisnessDetailConstants.BusGSTNO:
                    kYCDetailInfo.FieldValue = doc.BusGSTNO ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCBuisnessDetailConstants.BusEntityType:
                    kYCDetailInfo.FieldValue = doc.BusEntityType ?? "";
                    break;
                //case Global.Infrastructure.Constants.KYCBuisnessDetailConstants.BusPan:
                //    kYCDetailInfo.FieldValue = doc.BusPan ?? "";
                //    break;
                case Global.Infrastructure.Constants.KYCBuisnessDetailConstants.CurrentAddressId:
                    kYCDetailInfo.FieldValue = doc.CurrentAddressId.HasValue ? doc.CurrentAddressId.ToString() : "";
                    break;
                case Global.Infrastructure.Constants.KYCBuisnessDetailConstants.BuisnessMonthlySalary:
                    kYCDetailInfo.FieldValue = doc.BuisnessMonthlySalary.HasValue ? doc.BuisnessMonthlySalary.ToString() : "";
                    break;
                case Global.Infrastructure.Constants.KYCBuisnessDetailConstants.OwnershipType:
                    kYCDetailInfo.FieldValue = doc.OwnershipType ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCBuisnessDetailConstants.IncomeSlab:
                    kYCDetailInfo.FieldValue = doc.IncomeSlab ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCBuisnessDetailConstants.CustomerElectricityNumber:
                    kYCDetailInfo.FieldValue = doc.CustomerElectricityNumber ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCBuisnessDetailConstants.BuisnessDocumentNo:
                    kYCDetailInfo.FieldValue = doc.BuisnessDocumentNo.ToString() ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCBuisnessDetailConstants.BuisnessProof:
                    kYCDetailInfo.FieldValue = doc.BuisnessProof ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCBuisnessDetailConstants.BuisnessProofDocId:
                    kYCDetailInfo.FieldValue = doc.BuisnessProofDocId.ToString() ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCBuisnessDetailConstants.InquiryAmount:
                    kYCDetailInfo.FieldValue = doc.InquiryAmount.HasValue ? doc.InquiryAmount.ToString() : "0";
                    break;
                case Global.Infrastructure.Constants.KYCBuisnessDetailConstants.SurrogateType:
                    kYCDetailInfo.FieldValue = doc.SurrogateType ?? "";
                    break;
                //case Global.Infrastructure.Constants.KYCPersonalDetailConstants.PermanentAddressId:
                //    kYCDetailInfo.FieldValue = doc.PermanentAddressId.HasValue ? doc.PermanentAddressId.ToString() : "";
                //    break;
                //case KYCBuisnessDetailConstants.BusAddCorrLine1:
                //    kYCDetailInfo.FieldValue = doc.BusAddCorrLine1 ?? "";
                //    break;
                //case KYCBuisnessDetailConstants.BusAddCorrLine2:
                //    kYCDetailInfo.FieldValue = doc.BusAddCorrLine2 ?? "";
                //    break;
                //case KYCBuisnessDetailConstants.BusAddCorrPincode:
                //    kYCDetailInfo.FieldValue = doc.BusAddCorrPincode ?? "";
                //    break;
                //case KYCBuisnessDetailConstants.BusAddCorrCity:
                //    kYCDetailInfo.FieldValue = doc.BusAddCorrCity ?? "";
                //    break;
                //case KYCBuisnessDetailConstants.BusAddCorrState:
                //    kYCDetailInfo.FieldValue = doc.BusAddCorrState ?? "";
                //    break;
                //case KYCBuisnessDetailConstants.BusAddPerLine1:
                //    kYCDetailInfo.FieldValue = doc.BusAddPerLine1;
                //    break;
                //case KYCBuisnessDetailConstants.BusAddPerLine2:
                //    kYCDetailInfo.FieldValue = doc.BusAddPerLine2;
                //    break;
                //case KYCBuisnessDetailConstants.BusAddPerPincode:
                //    kYCDetailInfo.FieldValue = doc.BusAddPerPincode;
                //    break;
                //case KYCBuisnessDetailConstants.BusAddPerCity:
                //    kYCDetailInfo.FieldValue = doc.BusAddPerCity;
                //    break;
                //case KYCBuisnessDetailConstants.BusAddPerState:
                //    kYCDetailInfo.FieldValue = doc.BusAddPerState;
                //    break;
                //case KYCBuisnessDetailConstants.SequenceNo:
                //    kYCDetailInfo.FieldValue = doc.SequenceNo.ToString();
                //    break;
                //case KYCBuisnessDetailConstants.BusEstablishmentProofType:
                //    kYCDetailInfo.FieldValue = doc.BusEstablishmentProofType;
                //    break;
                default:
                    break;
            }
            return kYCDetailInfo;

        }

        public Task<ResultViewModel<bool>> ValidateByUniqueId(BusinessActivityDetail input)
        {
            throw new NotImplementedException();
        }

        public Task<ResultViewModel<long>> IsDocumentExist(BusinessActivityDetail input, string userId)
        {
            throw new NotImplementedException();
        }
    }
}
