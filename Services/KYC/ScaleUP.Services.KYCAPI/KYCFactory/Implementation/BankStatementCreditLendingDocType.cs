using Microsoft.EntityFrameworkCore;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.KYCActivity;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Services.KYCAPI.Persistence;
using ScaleUP.Services.KYCDTO.Transacion;
using ScaleUP.Services.KYCModels.Master;
using ScaleUP.Services.KYCModels.Transaction;
using static MassTransit.ValidationResultExtensions;

namespace ScaleUP.Services.KYCAPI.KYCFactory.Implementation
{
    public class BankStatementCreditLendingDocType : BaseDocType, IDocType<BankStatementCreditLendingDTO, BankStatementCreditLendingActivity>
    {
        public BankStatementCreditLendingDocType(ApplicationDbContext context, string userId, string filePath) : base(context, userId, filePath)
        {
            docTypeCode = KYCMasterConstants.BankStatementCreditLending;
        }

        public async Task<ResultViewModel<long>> GetAndSaveByUniqueId(BankStatementCreditLendingActivity input, string userId, string CreatedBy, string productCode)
        {
            ResultViewModel<long> result = new ResultViewModel<long>();
            try
            {
                //await RemoveDocInfo(userId);
                ResultViewModel<long> res = null;
                BankStatementCreditLendingDTO bankStatementCreditLendingDTO = new BankStatementCreditLendingDTO
                {
                    BankDocumentId = input.BankDocumentId,
                    SarrogateDocId = input.SarrogateDocId,
                    SurrogateType = input.SurrogateType
                };
                res = await SaveDoc(bankStatementCreditLendingDTO, userId, CreatedBy, productCode);
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

        public Task<ResultViewModel<BankStatementCreditLendingDTO>> GetByUniqueId(BankStatementCreditLendingActivity input)
        {
            throw new NotImplementedException();
        }

        public ResultViewModel<bool> IsValidTOSave(BankStatementCreditLendingDTO doc, KYCMaster kycMaster, string userId, string productCode)
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

        public async Task<ResultViewModel<long>> SaveDoc(BankStatementCreditLendingDTO doc, string userId, string CreatedBy, string productCode)
        {
            ResultViewModel<long> res = null;

            var creaditlendingId = Context.KYCMasters.Where(x => x.IsActive == true && x.IsDeleted == false && x.Code == KYCMasterConstants.BankStatementCreditLending.ToString()).First();
            var thismasterInfo = await Context.KYCMasterInfos
                                    .Include("KYCDetailInfoList")
                                    .Where(x => x.UserId == userId && x.IsActive == true && x.IsDeleted == false && x.KYCMasterId == creaditlendingId.Id && x.ProductCode == productCode)
                                    .FirstOrDefaultAsync();

            var kycDetailQuery = from m in Context.KYCMasters
                                 join d in Context.KYCDetails on m.Id equals d.KYCMasterId
                                 where d.IsActive == true && d.IsDeleted == false
                                 && m.Code == KYCMasterConstants.BankStatementCreditLending
                                 select d;
            var kycDetailList = kycDetailQuery.ToList();

            var kycMaster = Context.KYCMasters.Where(x => x.Code == KYCMasterConstants.BankStatementCreditLending && x.IsActive == true && x.IsDeleted == false).First();

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
                        UniqueId = doc.BankDocumentId.ToString(),
                        UserId = userId,
                        KYCMasterId = kycMaster.Id,
                        Created = DateTime.Now,
                        KYCDetailInfoList = new List<KYCDetailInfo>(),
                        CreatedBy = CreatedBy,
                        ProductCode = productCode
                    };
                }
                else
                {
                    kYCMasterInfo = thismasterInfo;
                    kYCMasterInfo.LastModified = DateTime.Now;
                    kYCMasterInfo.ResponseJSON = Newtonsoft.Json.JsonConvert.SerializeObject(doc);
                    kYCMasterInfo.UniqueId = doc.BankDocumentId.ToString();
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

        private KYCDetailInfo GetDetail(BankStatementCreditLendingDTO doc, KYCDetail kYCDetail)
        {
            KYCDetailInfo kYCDetailInfo = new KYCDetailInfo
            {
                KYCDetailId = kYCDetail.Id,
                IsActive = true,
                IsDeleted = false
            };

            switch (kYCDetail.Field)
            {
                case KYCBankStatementCreditLendingConstant.DocumentId:
                    kYCDetailInfo.FieldValue = doc.BankDocumentId != null ? doc.BankDocumentId.ToString() : "";
                    break;
                case KYCBankStatementCreditLendingConstant.SarrogateDocId:
                    kYCDetailInfo.FieldValue = doc.SarrogateDocId != null ? doc.SarrogateDocId.ToString() : "";
                    break;
                //case KYCBankStatementCreditLendingConstant.ITRDocumentId:
                //    kYCDetailInfo.FieldValue = doc.ITRDocumentId != null ? doc.ITRDocumentId.ToString() : "";
                //    break;
                case KYCBankStatementCreditLendingConstant.SurrogateType:
                    kYCDetailInfo.FieldValue = doc.SurrogateType != null ? doc.SurrogateType.ToString() : "";
                    break;
                default:
                    break;
            }
            return kYCDetailInfo;
        }

        public Task<ResultViewModel<bool>> ValidateByUniqueId(BankStatementCreditLendingActivity input)
        {
            throw new NotImplementedException();
        }
    }
}
