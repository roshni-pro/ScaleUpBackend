using Microsoft.EntityFrameworkCore;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.DSA;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Global.Infrastructure.Constants.DSA;
using ScaleUP.Services.KYCAPI.Persistence;
using ScaleUP.Services.KYCDTO.Transacion.DSA;
using ScaleUP.Services.KYCModels.Master;
using ScaleUP.Services.KYCModels.Transaction;

namespace ScaleUP.Services.KYCAPI.KYCFactory.Implementation.DSA
{
    public class DSAProfileDocType : BaseDocType, IDocType<DSAProfileTypeDTO, KYCDSAProfileTypeActivity>
    {
        public DSAProfileDocType(ApplicationDbContext context, string userId, string filePath) : base(context, userId, filePath)
        {
            docTypeCode = KYCMasterConstants.DSAProfileType;
        }

        public async Task<ResultViewModel<long>> GetAndSaveByUniqueId(KYCDSAProfileTypeActivity input, string userId, string CreatedBy, string productCode)
        {
            ResultViewModel<long> result = null;
            try
            {
                ResultViewModel<long> res = null;
                DSAProfileTypeDTO dsaProfileTypeDTO = new DSAProfileTypeDTO
                {
                    DSAType = input.DSAType,
                    //LeadId = input.LeadId
                };
                res = await SaveDoc(dsaProfileTypeDTO, userId, CreatedBy, productCode);
                result = res;
            }
            catch (Exception ex)
            {
                result = new ResultViewModel<long>
                {
                    IsSuccess = false,
                    Message = "Error occured while saving",
                    Result = 0
                };
            }
            return result;
        }

        public Task<ResultViewModel<DSAProfileTypeDTO>> GetByUniqueId(KYCDSAProfileTypeActivity input)
        {
            throw new NotImplementedException();
        }

        public ResultViewModel<bool> IsValidTOSave(DSAProfileTypeDTO doc, KYCMaster kycMaster, string userId, string productCode)
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

        public async Task<ResultViewModel<long>> SaveDoc(DSAProfileTypeDTO doc, string userId, string CreatedBy, string productCode)
        {
            ResultViewModel<long> res = null;

            var selfieId = Context.KYCMasters.Where(x => x.IsActive == true && x.IsDeleted == false && x.Code == KYCMasterConstants.DSAProfileType.ToString()).First();
            var thismasterInfo = await Context.KYCMasterInfos
                                    .Include("KYCDetailInfoList")
                                    .Where(x => x.UserId == userId && x.IsActive == true && x.IsDeleted == false && x.KYCMasterId == selfieId.Id && x.ProductCode == productCode)
                                    .FirstOrDefaultAsync();

            var kycDetailQuery = from m in Context.KYCMasters
                                 join d in Context.KYCDetails on m.Id equals d.KYCMasterId
                                 where d.IsActive == true && d.IsDeleted == false
                                 && m.Code == Global.Infrastructure.Constants.KYCMasterConstants.DSAProfileType
                                 select d;
            var kycDetailList = kycDetailQuery.ToList();

            var kycMaster = Context.KYCMasters.Where(x => x.Code == Global.Infrastructure.Constants.KYCMasterConstants.DSAProfileType && x.IsActive == true && x.IsDeleted == false).First();
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
                        UniqueId = doc.LeadId.ToString(),
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
                    kYCMasterInfo.UniqueId = doc.LeadId.ToString();
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
                Context.SaveChanges();
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

        private KYCDetailInfo GetDetail(DSAProfileTypeDTO doc, KYCDetail kYCDetail)
        {
            KYCDetailInfo kYCDetailInfo = new KYCDetailInfo
            {
                KYCDetailId = kYCDetail.Id,
                IsActive = true,
                IsDeleted = false
            };

            switch (kYCDetail.Field)
            {
                //case SelfieConstant.FrontImageUrl:
                //    kYCDetailInfo.FieldValue = doc.FrontImageUrl ?? "";
                //    break;
                case DSAProfileTypeConstants.DSA:
                    kYCDetailInfo.FieldValue = doc.DSAType.ToString() ?? "";
                    break;
                default:
                    break;
            }
            return kYCDetailInfo;

        }

        public Task<ResultViewModel<bool>> ValidateByUniqueId(KYCDSAProfileTypeActivity input)
        {
            throw new NotImplementedException();
        }

    }
}
