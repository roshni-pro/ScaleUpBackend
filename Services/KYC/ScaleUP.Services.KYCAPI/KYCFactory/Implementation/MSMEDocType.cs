using Grpc.Core;
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
    public class MSMEDocType : BaseDocType, IDocType<MSMEDTO, MSMEActivity>
    {
        public MSMEDocType(ApplicationDbContext context, string userId, string filePath) : base(context, userId, filePath)
        {
            docTypeCode = KYCMasterConstants.MSME;
        }

        public async Task<ResultViewModel<long>> GetAndSaveByUniqueId(MSMEActivity input, string userId, string CreatedBy, string productCode)
        {
            ResultViewModel<long> result = new ResultViewModel<long>();

            try
            {
                //await RemoveDocInfo(userId);
                ResultViewModel<long> res = null;
                MSMEDTO mSMEDTO = new MSMEDTO()
                {
                    //FrontFileUrl = input.FrontFileUrl,
                    //extra fields added
                    BusinessName = input.BusinessName,
                    BusinessType = input.BusinessType,
                    Vintage = input.Vintage,
                    MSMERegNum = input.MSMERegNum,
                    MSMECertificate = input.MSMECertificate,
                    FrontDocumentId = input.FrontDocumentId,
                    LeadMasterId = input.LeadMasterId
                };
                res = await SaveDoc(mSMEDTO, userId, CreatedBy, productCode);
                result = res; 
            }
            catch(Exception ex)
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

        public Task<ResultViewModel<MSMEDTO>> GetByUniqueId(MSMEActivity input)
        {
            throw new NotImplementedException();
        }

        //public async Task<Dictionary<string, dynamic>> GetDoc(string userId)
        //{

        //    var kycTypeId = Context.KYCMasters.Where(x => x.Code == KYCMasterConstants.MSME).Select(x => x.Id).First();

        //    var masterInfo = Context.KYCMasterInfos.Where(x => x.UserId == userId && x.KYCMasterId == kycTypeId && x.IsActive == true && x.IsDeleted == false).Include(y => y.KYCDetailInfoList)
        //        .ThenInclude(z => z.KYCDetail).OrderByDescending(y => y.LastModified).FirstOrDefault();

        //    if (masterInfo != null)
        //    {
        //        var master = await Context.KYCMasters.Where(x => x.Id == masterInfo.KYCMasterId).FirstOrDefaultAsync();


        //        //var kycDetail = await Context.KYCDetails.Where(x => x.KYCMasterId == master.Id).ToListAsync();
        //        //var kycDetailInfo = await Context.KYCDetailInfos.Where(x => x.KYCMasterInfoId == masterInfo.Id).ToListAsync();

        //        if (masterInfo.LastModified.Value.AddDays(master.ValidityDays) < DateTime.Today)
        //        {
        //            masterInfo = null;
        //        }
        //    }

        //    if (masterInfo != null)
        //    {
        //        Dictionary<string, dynamic> keyValuePairsMSMEDoc = new Dictionary<string, dynamic>();
        //        keyValuePairsMSMEDoc.Add("userId", userId);
        //        keyValuePairsMSMEDoc.Add("UniqueId", masterInfo.UniqueId);
        //        foreach (var item in masterInfo.KYCDetailInfoList)
        //        {
        //            switch (item.KYCDetail.FieldType)
        //            {
        //                case (int)FieldTypeEnum.DateTime:
        //                    keyValuePairsMSMEDoc.Add(item.KYCDetail.Field, DateTime.Parse(item.FieldValue));
        //                    break;
        //                case (int)FieldTypeEnum.Double:
        //                    keyValuePairsMSMEDoc.Add(item.KYCDetail.Field, double.Parse(item.FieldValue));
        //                    break;
        //                case (int)FieldTypeEnum.String:
        //                    keyValuePairsMSMEDoc.Add(item.KYCDetail.Field, item.FieldValue);
        //                    break;
        //                case (int)FieldTypeEnum.Integer:
        //                    keyValuePairsMSMEDoc.Add(item.KYCDetail.Field, Int32.Parse(item.FieldValue));
        //                    break;
        //                case (int)FieldTypeEnum.Boolean:
        //                    keyValuePairsMSMEDoc.Add(item.KYCDetail.Field, bool.Parse(item.FieldValue));
        //                    break;
        //                default:
        //                    break;
        //            }

        //        }
        //        return keyValuePairsMSMEDoc;
        //    }
        //    return null;
        //    //throw new NotImplementedException();
        //}

        public ResultViewModel<bool> IsValidTOSave(MSMEDTO doc, KYCMaster kycMaster, string userId, string productCode)
        {
            ResultViewModel<bool> res = null;
            res = new ResultViewModel<bool>
            {
                IsSuccess = true,
                Message = "Success",
                Result = true
            };
            return res;
        }

        public async Task<ResultViewModel<long>> SaveDoc(MSMEDTO doc, string userId, string CreatedBy, string productCode)
        {
            ResultViewModel<long> res = null;

            var msmeId = Context.KYCMasters.Where(x => x.IsActive == true && x.IsDeleted == false && x.Code == KYCMasterConstants.MSME.ToString()).First();
            var thismasterInfo = await Context.KYCMasterInfos
                                    .Include("KYCDetailInfoList")
                                    .Where(x => x.UserId == userId && x.IsActive == true && x.IsDeleted == false && x.KYCMasterId == msmeId.Id && x.ProductCode == productCode)
                                    .FirstOrDefaultAsync();

            var kycDetailQuery = from m in Context.KYCMasters
                                 join d in Context.KYCDetails on m.Id equals d.KYCMasterId
                                 where d.IsActive == true && d.IsDeleted == false
                                 && m.Code == Global.Infrastructure.Constants.KYCMasterConstants.MSME
                                 select d;
            var kycDetailList = kycDetailQuery.ToList();

            var kycMaster = Context.KYCMasters.Where(x => x.Code == Global.Infrastructure.Constants.KYCMasterConstants.MSME && x.IsActive == true && x.IsDeleted == false).First();

            ResultViewModel<bool> isValid = IsValidTOSave(doc, kycMaster, userId, productCode);

            if (isValid.Result)
            {
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
                return res;
            }
            else
            {
                res = new ResultViewModel<long>
                {
                    IsSuccess = false,
                    Message = isValid.Message,
                    Result = 0
                };
                return res;
            }
        }

        //private async Task<int> kYCMasterData()
        //{
        //    int res = 0;
        //    List<KYCMasterDTO> kycData = new List<KYCMasterDTO>();
        //        string qry = "select * from KYCMasterContants";
        //        kycData = Context.Database.SqlQuery<KYCMasterDTO>(qry).ToList();
        //    if(kycData!= null)
        //    {
        //       res = kycData.FirstOrDefault(x => x.MasterName == KYCMasterConstants.MSME).Id;
        //    }
        //        return res;
        //}

        private KYCDetailInfo GetDetail(MSMEDTO doc, KYCDetail kYCDetail)
        {
            KYCDetailInfo kYCDetailInfo = new KYCDetailInfo
            {
                KYCDetailId = kYCDetail.Id,
                IsActive = true,
                IsDeleted = false
            };

            switch (kYCDetail.Field)
            {
                case Global.Infrastructure.Constants.KYCMSMEConstants.MSMERegNum:
                    kYCDetailInfo.FieldValue = doc.MSMERegNum ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCMSMEConstants.BusinessName:
                    kYCDetailInfo.FieldValue = doc.BusinessName ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCMSMEConstants.BusinessType:
                    kYCDetailInfo.FieldValue = doc.BusinessType ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCMSMEConstants.Vintage:
                    kYCDetailInfo.FieldValue = doc.Vintage.ToString() ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCMSMEConstants.FrontDocumentId:
                    kYCDetailInfo.FieldValue = doc.FrontDocumentId.ToString() ?? "";
                    break;
                default:
                    break;
            }
            return kYCDetailInfo;

        }
        public Task<ResultViewModel<bool>> ValidateByUniqueId(MSMEActivity input)
        {
            throw new NotImplementedException();
        }
        public Task<ResultViewModel<long>> IsDocumentExist(MSMEActivity input, string userId)
        {
            throw new NotImplementedException();
        }
    }
}
