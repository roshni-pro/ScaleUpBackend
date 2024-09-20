using Elasticsearch.Net;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.KYCActivity;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Services.KYCAPI.Helpers;
using ScaleUP.Services.KYCAPI.Persistence;
using ScaleUP.Services.KYCDTO.Constant;
using ScaleUP.Services.KYCDTO.Enum;
using ScaleUP.Services.KYCDTO.Transacion;
using ScaleUP.Services.KYCModels.Master;
using ScaleUP.Services.KYCModels.Transaction;

namespace ScaleUP.Services.KYCAPI.KYCFactory.Implementation
{
    public class AppyFlowGSTDocType : BaseDocType, IDocType<AppyFlowGSTDTO, KYCActivityGST>
    {
        public AppyFlowGSTDocType(ApplicationDbContext context, string userId, string filePath) : base(context, userId, filePath)
        {
            docTypeCode = KYCMasterConstants.GST;
        }
        public async Task<ResultViewModel<long>> SaveDoc(AppyFlowGSTDTO doc, string userId, string CreatedBy, string productCode)
        {
            ResultViewModel<long> res = null;
            var kycDetailQuery = from m in Context.KYCMasters
                                 join d in Context.KYCDetails on m.Id equals d.KYCMasterId
                                 where d.IsActive == true && d.IsDeleted == false

                                 && m.Code == KYCMasterConstants.GST
                                 select d;
            var kycDetailList = kycDetailQuery.ToList();

            var kycMaster = Context.KYCMasters.Where(x => x.Code == KYCMasterConstants.GST && x.IsActive == true && x.IsDeleted == false).First();

            ResultViewModel<bool> isValid = IsValidTOSave(doc, kycMaster, userId, productCode);

            if (isValid.Result)
            {
                KYCMasterInfo kYCMasterInfo = new KYCMasterInfo
                {
                    LastModified = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false,
                    ResponseJSON = JsonConvert.SerializeObject(doc),
                    UniqueId = doc.GSTNumber,
                    UserId = userId,
                    KYCDetailInfoList = new List<KYCDetailInfo>(),
                    ProductCode = productCode,

                };

                foreach (var item in kycDetailList)
                {
                    var kycDetailInfo = GetDetail(doc, item);
                    kYCMasterInfo.KYCDetailInfoList.Add(kycDetailInfo);
                }

                Context.KYCMasterInfos.Add(kYCMasterInfo);
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

        public ResultViewModel<bool> IsValidTOSave(AppyFlowGSTDTO doc, KYCMaster kycMaster, string userId, string productCode)
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

        private KYCDetailInfo GetDetail(AppyFlowGSTDTO doc, KYCDetail kYCDetail)
        {
            KYCDetailInfo kYCDetailInfo = new KYCDetailInfo
            {
                KYCDetailId = kYCDetail.Id,
                IsActive = true,
                IsDeleted = false
            };

            switch (kYCDetail.Field)
            {
                case AppyFlowGSTDetailConstants.BusinessName:
                    kYCDetailInfo.FieldValue = doc.BusinessName?.ToString();
                    break;
                case AppyFlowGSTDetailConstants.LandingName:
                    kYCDetailInfo.FieldValue = doc.LandingName?.ToString();
                    break;
                case AppyFlowGSTDetailConstants.Address:
                    kYCDetailInfo.FieldValue = doc.Address?.ToString();
                    break;
                case AppyFlowGSTDetailConstants.CityName:
                    kYCDetailInfo.FieldValue = doc.CityName?.ToString();
                    break;
                case AppyFlowGSTDetailConstants.StateName:
                    kYCDetailInfo.FieldValue = doc.StateName?.ToString();
                    break;
                case AppyFlowGSTDetailConstants.ZipCode:
                    kYCDetailInfo.FieldValue = doc.ZipCode?.ToString();
                    break;
                case AppyFlowGSTDetailConstants.GSTNumber:
                    kYCDetailInfo.FieldValue = doc.GSTNumber?.ToString();
                    break;
                default:
                    break;
            }
            return kYCDetailInfo;

        }

        //public async Task<Dictionary<string, dynamic>> GetDoc(string userId)
        //{
        //    var masterInfo = Context.KYCMasterInfos.Where(x => x.UserId == userId && x.IsActive == true && x.IsDeleted == false).OrderByDescending(y => y.LastModified).FirstOrDefault();
        //    if (masterInfo != null)
        //    {
        //        var master = Context.KYCMasters.Where(x => x.Id == masterInfo.KYCMasterId).FirstOrDefault();

        //        if (masterInfo.LastModified.Value.AddDays(master.ValidityDays) < DateTime.Today)
        //        {
        //            masterInfo = null;
        //        }
        //    }

        //    if (masterInfo != null)
        //    {
        //        Dictionary<string, dynamic> keyValuePairs = new Dictionary<string, dynamic>();
        //        keyValuePairs.Add("userId", userId);
        //        keyValuePairs.Add("UniqueId", masterInfo.UniqueId);
        //        foreach (var item in masterInfo.KYCDetailInfoList)
        //        {
        //            switch (item.KYCDetail.FieldType)
        //            {
        //                case (int)FieldTypeEnum.DateTime:
        //                    keyValuePairs.Add(item.KYCDetail.Field, DateTime.Parse(item.FieldValue));
        //                    break;
        //                case (int)FieldTypeEnum.Double:
        //                    keyValuePairs.Add(item.KYCDetail.Field, double.Parse(item.FieldValue));
        //                    break;
        //                case (int)FieldTypeEnum.String:
        //                    keyValuePairs.Add(item.KYCDetail.Field, item.FieldValue);
        //                    break;
        //                case (int)FieldTypeEnum.Integer:
        //                    keyValuePairs.Add(item.KYCDetail.Field, Int32.Parse(item.FieldValue));
        //                    break;
        //                case (int)FieldTypeEnum.Boolean:
        //                    keyValuePairs.Add(item.KYCDetail.Field, bool.Parse(item.FieldValue));
        //                    break;
        //                default:
        //                    break;
        //            }

        //        }
        //        return keyValuePairs;
        //    }

        //    return null;

        //}

        public async Task<ResultViewModel<bool>> ValidateByUniqueId(KYCActivityGST id)
        {
            ResultViewModel<bool> response = null;
            AppyFlowGSTHelper appyFlowGSTHelper = new AppyFlowGSTHelper(Context);
            var res = await appyFlowGSTHelper.AppyFlowGSTVerification(id.UniqueId);

            response = new ResultViewModel<bool>
            {
                IsSuccess = res.Status,
                Message = res.Message,
                Result = res.Status
            };
            return response;
        }

        public async Task<ResultViewModel<AppyFlowGSTDTO>> GetByUniqueId(KYCActivityGST id)
        {
            ResultViewModel<AppyFlowGSTDTO> result = null;
            AppyFlowGSTHelper appyFlowGSTHelper = new AppyFlowGSTHelper(Context);
            var res = await appyFlowGSTHelper.AppyFlowGSTVerification(id.UniqueId);
            result = new ResultViewModel<AppyFlowGSTDTO>
            {
                IsSuccess = res.Status,
                Message = res.Message,
                Result = res.GSTInfo
            };

            //result.Result = res;
            // return res.Status ? res.GSTInfo : null;
            return result;
        }

        public async Task<ResultViewModel<long>> GetAndSaveByUniqueId(KYCActivityGST kycActivityGST, string userId, string CreatedBy, string productCode)
        {
            ResultViewModel<long> result = new ResultViewModel<long>();

            try
            {
                await RemoveDocInfo(userId);
                ResultViewModel<AppyFlowGSTDTO> appyFlowGSTDTO = await GetByUniqueId(kycActivityGST);
                var res = await SaveDoc(appyFlowGSTDTO.Result, userId, CreatedBy, productCode);
                result = new ResultViewModel<long>
                {
                    IsSuccess =true,
                    Message = "Success",
                    Result = res.Result
                };
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

        public Task<ResultViewModel<long>> IsDocumentExist(KYCActivityGST input, string userId)
        {
            throw new NotImplementedException();
        }
    }
}
