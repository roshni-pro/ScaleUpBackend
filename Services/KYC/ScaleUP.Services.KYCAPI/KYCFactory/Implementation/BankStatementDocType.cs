using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.KYCActivity;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Services.KYCAPI.Persistence;
using ScaleUP.Services.KYCDTO.Transacion;
using ScaleUP.Services.KYCModels.Master;
using ScaleUP.Services.KYCModels.Transaction;

namespace ScaleUP.Services.KYCAPI.KYCFactory.Implementation
{
    public class BankStatementDocType : BaseDocType, IDocType<BankStatementDTO, KYCBankStatementActivity>
    {
        public BankStatementDocType(ApplicationDbContext context, string userId, string filePath) : base(context, userId, filePath)
        {
            docTypeCode = KYCMasterConstants.BankStatement;
        }

        public async Task<ResultViewModel<long>> GetAndSaveByUniqueId(KYCBankStatementActivity input, string userId, string CreatedBy, string productCode)
        {
            ResultViewModel<long> result = new ResultViewModel<long>();

            try
            {
               // await RemoveDocInfo(userId);
                ResultViewModel<long> res = null;
                BankStatementDTO bankStatementDTO = new BankStatementDTO()
                {
                    //DocumentMasterId = input.DocumentMasterId,
                    DocumentNumber = input.DocumentNumber,
                    //FrontFileUrl = input.FrontFileUrl,
                    //OtherInfo = input.OtherInfo,
                    PdfPassword = input.PdfPassword,
                    BorroBankName = input.BorroBankName,
                    BorroBankIFSC = input.BorroBankIFSC,
                    BorroBankAccNum = input.BorroBankAccNum,
                    //GSTStatement = input.GSTStatement,
                    BankStatement = input.BankStatement,
                    EnquiryAmount = input.EnquiryAmount,
                    LeadMasterId = input.LeadMasterId,
                    DocumentId = input.DocumentId,
                    AccType = input.AccType
                };
                res = await SaveDoc(bankStatementDTO, userId, CreatedBy, productCode);

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

        public Task<ResultViewModel<BankStatementDTO>> GetByUniqueId(KYCBankStatementActivity input)
        {
            throw new NotImplementedException();
        }

        //public Task<Dictionary<string, dynamic>> GetDoc(string userId)
        //{
        //    throw new NotImplementedException();
        //}

        //public bool IsValidTOSave(BankStatementDTO doc, KYCMaster kycMaster)
        //{
        //    var masterInfo = Context.KYCMasterInfos.Where(x => x.IsActive == true && x.IsDeleted == false && x.LastModified.Value.AddDays(kycMaster.ValidityDays) > DateTime.Today).OrderByDescending(y => y.LastModified).FirstOrDefault();
        //    if (doc == null || string.IsNullOrEmpty(doc.LeadMasterId.ToString()))
        //    {
        //        return false;
        //    }
        //    else if (masterInfo != null)
        //    {
        //        return false;
        //    }
        //    else return true;
        //}

        public async Task<ResultViewModel<long>> SaveDoc(BankStatementDTO doc, string userId, string CreatedBy, string productCode)
        {
            ResultViewModel<long> res = null;

            var bankstatementId = Context.KYCMasters.Where(x => x.IsActive == true && x.IsDeleted == false && x.Code == KYCMasterConstants.BankStatement.ToString()).First();
            var thismasterInfo = await Context.KYCMasterInfos
                                    .Include("KYCDetailInfoList")
                                    .Where(x => x.UserId == userId && x.IsActive == true && x.IsDeleted == false && x.KYCMasterId == bankstatementId.Id && x.ProductCode == productCode)
                                    .FirstOrDefaultAsync();

            var kycDetailQuery = from m in Context.KYCMasters
                                 join d in Context.KYCDetails on m.Id equals d.KYCMasterId
                                 where d.IsActive == true && d.IsDeleted == false
                                 && m.Code == KYCMasterConstants.BankStatement
                                 select d;
            var kycDetailList = kycDetailQuery.ToList();

            var kycMaster = Context.KYCMasters.Where(x => x.Code == KYCMasterConstants.BankStatement && x.IsActive == true && x.IsDeleted == false).First();

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

        private KYCDetailInfo GetDetail(BankStatementDTO doc, KYCDetail kYCDetail)
        {
            KYCDetailInfo kYCDetailInfo = new KYCDetailInfo
            {
                KYCDetailId = kYCDetail.Id,
                IsActive = true,
                IsDeleted = false
            };

            switch (kYCDetail.Field)
            {

                case KYCBankStatementConstants.DocumentId:
                    kYCDetailInfo.FieldValue = doc.DocumentId ?? "";
                    break;
                case KYCBankStatementConstants.BorroBankAccNum:
                    kYCDetailInfo.FieldValue = doc.BorroBankAccNum ?? "";
                    break;
                case KYCBankStatementConstants.BorroBankIFSC:
                    kYCDetailInfo.FieldValue = doc.BorroBankIFSC ?? "";
                    break;
                case KYCBankStatementConstants.BorroBankName:
                    kYCDetailInfo.FieldValue = doc.BorroBankName ?? "";
                    break;
                case KYCBankStatementConstants.EnquiryAmount:
                    kYCDetailInfo.FieldValue = doc.EnquiryAmount.ToString();
                    break;
                case KYCBankStatementConstants.PdfPassword:
                    kYCDetailInfo.FieldValue = doc.PdfPassword ?? "";
                    break;
                case KYCBankStatementConstants.AccType:
                    kYCDetailInfo.FieldValue = doc.AccType ?? "";
                    break;
                default:
                    break;
            }
            return kYCDetailInfo;

        }


        public Task<ResultViewModel<bool>> ValidateByUniqueId(KYCBankStatementActivity input)
        {
            throw new NotImplementedException();
        }

        public Task<ResultViewModel<long>> IsDocumentExist(KYCBankStatementActivity input, string userId)
        {
            throw new NotImplementedException();
        }

        public ResultViewModel<bool> IsValidTOSave(BankStatementDTO doc, KYCMaster kycMaster, string userId, string productCode)
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
    }
}
