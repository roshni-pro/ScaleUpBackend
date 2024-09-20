using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.KYCActivity;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Services.KYCAPI.Helpers;
using ScaleUP.Services.KYCAPI.Migrations;
using ScaleUP.Services.KYCAPI.Persistence;
using ScaleUP.Services.KYCDTO.Constant;
using ScaleUP.Services.KYCDTO.Enum;
using ScaleUP.Services.KYCDTO.Transacion;
using ScaleUP.Services.KYCModels.Master;
using ScaleUP.Services.KYCModels.Transaction;

namespace ScaleUP.Services.KYCAPI.KYCFactory.Implementation
{
    public class KarzaPANDocType : BaseDocType, IDocType<KarzaPANDTO, KYCActivityPAN>
    {

        public KarzaPANDocType(ApplicationDbContext context, string userId, string filePath) : base(context, userId, filePath)
        {
            docTypeCode = KYCMasterConstants.PAN;
        }

        public async Task<ResultViewModel<long>> SaveDoc(KarzaPANDTO doc, string userId, string CreatedBy, string productCode)
        {
            HashHelper hashHelper = new HashHelper();
            var uniqueIdHash = hashHelper.QuickHash(doc.id_number.ToUpper());
            //long ExistId = await IsDocumentExist(doc.id_number, userId);
            ResultViewModel<long> ExistId = null;


            KYCMasterInfo thismasterInfo = null;

            var panId = Context.KYCMasters.Where(x => x.IsActive == true && x.IsDeleted == false && x.Code == KYCMasterConstants.PAN.ToString()).First();
            try
            {
                thismasterInfo = await Context.KYCMasterInfos
                                        .Include("KYCDetailInfoList")
                                        .Where(x => x.UserId == userId && x.IsActive == true && x.IsDeleted == false && x.KYCMasterId == panId.Id && x.ProductCode == productCode)
                                        .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            //var masterInfo = await Context.KYCMasterInfos.Where(x => x.UniqueIdHash == uniqueIdHash && x.UserId == userId && x.IsActive == true && x.IsDeleted == false).OrderByDescending(y => y.LastModified).FirstOrDefaultAsync();
            //if (masterInfo != null)
            //{
            //    ExistId = new ResultViewModel<long>
            //    {
            //        IsSuccess = true,
            //        Message = "Success",
            //        Result = masterInfo.Id
            //    };
            //}
            //if (ExistId != null && ExistId.Result > 0)
            //{
            //    return ExistId;
            //}
            var kycDetailQuery = from m in Context.KYCMasters
                                 join d in Context.KYCDetails on m.Id equals d.KYCMasterId
                                 where d.IsActive == true && d.IsDeleted == false
                                 && m.Code == Global.Infrastructure.Constants.KYCMasterConstants.PAN
                                 select d;
            var kycDetailList = kycDetailQuery.ToList();

            var kycMaster = Context.KYCMasters.Where(x => x.Code == Global.Infrastructure.Constants.KYCMasterConstants.PAN && x.IsActive == true && x.IsDeleted == false).First();

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
                        UniqueId = doc.id_number.ToUpper(),
                        UserId = userId,
                        KYCMasterId = kycMaster.Id,
                        Created = DateTime.Now,
                        KYCDetailInfoList = new List<KYCDetailInfo>(),
                        UniqueIdHash = uniqueIdHash,
                        CreatedBy = CreatedBy,
                        ProductCode = productCode

                    };
                }
                else
                {
                    kYCMasterInfo = thismasterInfo;
                    kYCMasterInfo.LastModified = DateTime.Now;
                    kYCMasterInfo.ResponseJSON = Newtonsoft.Json.JsonConvert.SerializeObject(doc);
                    kYCMasterInfo.UniqueId = doc.id_number.ToUpper();
                    kYCMasterInfo.UniqueIdHash = uniqueIdHash;
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
                        Context.Entry(kycDetailInfoTemp).State = EntityState.Modified;
                    }
                    else
                    {
                        Context.Entry(kycDetailInfo).State = EntityState.Added;
                        kYCMasterInfo.KYCDetailInfoList.Add(kycDetailInfo);

                        kYCMasterInfo.CreatedBy = CreatedBy;

                    }

                    // Context.KYCDetailInfos.Add(kycDetailInfo);
                    //Context.SaveChanges();
                }
                //kYCMasterInfo.KYCDetailInfoList = KYCDetailInfos;
                if (kYCMasterInfo.Id == 0)
                {
                    Context.KYCMasterInfos.Add(kYCMasterInfo);
                }
                else
                {
                    Context.Entry(kYCMasterInfo).State = EntityState.Modified;
                }
                Context.SaveChanges();
                ExistId = new ResultViewModel<long>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Result = kYCMasterInfo.Id
                };
                return ExistId;
            }
            else
            {
                ExistId = new ResultViewModel<long>
                {
                    IsSuccess = false,
                    Message = isValid.Message,
                    Result = 0
                };
                return ExistId;
            }
        }

        public ResultViewModel<bool> IsValidTOSave(KarzaPANDTO doc, KYCMaster kycMaster, string userId, string productCode)
        {
            HashHelper hashHelper = new HashHelper();
            var uniqueIdHash = hashHelper.QuickHash(doc.id_number.ToUpper());
            DateTime zeroTime = new DateTime(1, 1, 1);
            TimeSpan span = DateTime.Today - doc.date_of_birth.Value;
            int years = (zeroTime + span).Year - 1;

            if (years < 18)
            {
                return new ResultViewModel<bool>
                {
                    IsSuccess = true,
                    Message = "Thank you for your interest. We can only process applications from individuals who are 18 years of age or older.",
                    Result = false
                };
            }

            ResultViewModel<bool> res = null;
            var query = from m in Context.KYCMasters
                        join mi in Context.KYCMasterInfos on m.Id equals mi.KYCMasterId
                        where mi.UniqueIdHash == uniqueIdHash
                            && mi.ProductCode == productCode
                            && mi.UserId != userId
                            && mi.IsActive == true
                            && mi.IsDeleted == false
                            && m.Code == this.docTypeCode
                        select mi;
            var masterInfo = query.FirstOrDefault();
            //var masterInfo = Context.KYCMasterInfos.Where(x => x.UniqueId == doc.id_number && x.IsActive == true && x.IsDeleted == false && x.LastModified.Value.AddDays(kycMaster.ValidityDays) > DateTime.Today).OrderByDescending(y => y.LastModified).FirstOrDefault();
            if (doc == null || string.IsNullOrEmpty(doc.id_number))
            {
                res = new ResultViewModel<bool>
                {
                    IsSuccess = true,
                    Message = "PAN Number not fount",
                    Result = false
                };
                return res;
            }
            else if (masterInfo != null)
            {
                res = new ResultViewModel<bool>
                {
                    IsSuccess = true,
                    Message = "PAN Number already exists",
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

        private KYCDetailInfo GetDetail(KarzaPANDTO doc, KYCDetail kYCDetail)
        {
            KYCDetailInfo kYCDetailInfo = new KYCDetailInfo
            {
                KYCDetailId = kYCDetail.Id,
                IsActive = true,
                IsDeleted = false
            };

            switch (kYCDetail.Field)
            {
                case KYCDetailConstants.PanType:
                    kYCDetailInfo.FieldValue = doc.pan_type != null ? doc.pan_type.ToString() : "Default";
                    break;
                case KYCDetailConstants.DOB:
                    kYCDetailInfo.FieldValue = doc.date_of_birth.HasValue ? doc.date_of_birth.Value.ToString("dd/MM/yyyy") : "";
                    break;
                case KYCDetailConstants.Minor:
                    kYCDetailInfo.FieldValue = doc.minor == true ? doc.minor.ToString() : false.ToString();
                    break;
                case KYCDetailConstants.Age:
                    kYCDetailInfo.FieldValue = doc.age.HasValue ? doc.age.ToString() : "";
                    break;
                case KYCDetailConstants.FatherName:
                    kYCDetailInfo.FieldValue = !string.IsNullOrEmpty(doc.fathers_name) ? doc.fathers_name.ToString() : "";
                    break;
                case KYCDetailConstants.DateOfIssue:
                    kYCDetailInfo.FieldValue = doc.date_of_issue.HasValue ? doc.date_of_issue.Value.ToString("dd/MM/yyyy") : "";
                    break;
                case KYCDetailConstants.IdScanned:
                    kYCDetailInfo.FieldValue = doc.id_scanned == true ? doc.id_scanned.ToString() : false.ToString();
                    break;
                case KYCDetailConstants.NameOnCard:
                    kYCDetailInfo.FieldValue = doc.name_on_card ?? "";
                    break;
                case KYCDetailConstants.DocumentId:
                    kYCDetailInfo.FieldValue = doc.DocumentId ?? "";
                    break;
                default:
                    break;
            }
            return kYCDetailInfo;

        }

        //public async Task<Dictionary<string, dynamic>> GetDoc(string userId)
        //{
        //    var kycTypeId = Context.KYCMasters.Where(x => x.Code == KYCMasterConstants.PAN).Select(x => x.Id).First();

        //    var masterInfo = Context.KYCMasterInfos.Where(x => x.UserId == userId && x.KYCMasterId == kycTypeId && x.IsActive == true && x.IsDeleted == false).Include(y => y.KYCDetailInfoList)
        //        .ThenInclude(z => z.KYCDetail).OrderByDescending(y => y.LastModified).FirstOrDefault(); 
        //    if(masterInfo != null)
        //    {
        //        var master = Context.KYCMasters.Where(x => x.Id == masterInfo.KYCMasterId).FirstOrDefault();

        //        if (masterInfo.LastModified.Value.AddDays(master.ValidityDays) < DateTime.Today)
        //        {
        //            masterInfo = null;
        //        }
        //    }

        //    if(masterInfo != null)
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

        public async Task<ResultViewModel<bool>> ValidateByUniqueId(KYCActivityPAN id)
        {
            ResultViewModel<bool> res = null;
            KarzaHelper karzaHelper = new KarzaHelper(Context);
            var karzaPanVerificationData = await karzaHelper.KarzaPanVerification(id.UniqueId, _filePath, this._userId);
            if (karzaPanVerificationData.StatusCode != 101 || karzaPanVerificationData.error != null) //101 mean succesfully 
            {
                res = new ResultViewModel<bool>
                {
                    IsSuccess = true,
                    Message = "Failed",
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
                return res;
            }
        }

        public async Task<ResultViewModel<KarzaPANDTO>> GetByUniqueId(KYCActivityPAN id)
        {
            string basePath = _filePath;
            string userid = _userId;
            ResultViewModel<KarzaPANDTO> karzaPANDTO = new ResultViewModel<KarzaPANDTO>();
            KarzaHelper karzaHelper = new KarzaHelper(Context);
            var karzaPanVerificationData = await karzaHelper.KarzaOCRVerificationAsync(id.ImagePath, _filePath, _userId);
            KarzaPanProfileHelper karzaPanProfileHelper = new KarzaPanProfileHelper(Context);
            //KarzaPanOcrResDTO karzaPanVerificationData = null;
            //var karzaPanProfileInfo = await karzaPanProfileHelper.KarzaPanProfile(id.UniqueId, basePath, userid);
            if (karzaPanVerificationData != null && karzaPanVerificationData.OtherInfo != null && !string.IsNullOrEmpty(karzaPanVerificationData.OtherInfo.name_on_card))
            {

            }
            else
            {
                karzaPanVerificationData = new KarzaPanOcrResDTO
                {
                    OtherInfo = new KarzaPANDTO
                    {
                        age = 0,
                        date_of_birth = id.DOB,
                        date_of_issue = null,
                        DocumentId = id.DocumentId.ToString(),
                        fathers_name = id.FathersName,
                        id_number = id.UniqueId,
                        id_scanned = false,
                        minor = false,
                        name_on_card = id.Name,
                        pan_type = ""
                    }
                };
            }
            if (karzaPanVerificationData != null && karzaPanVerificationData.OtherInfo != null)
            {
                karzaPANDTO.IsSuccess = true;
                karzaPANDTO.Message = "Success";
                karzaPANDTO.Result = karzaPanVerificationData.OtherInfo;

                if (karzaPanVerificationData.OtherInfo.id_number != id.UniqueId)
                {
                    karzaPANDTO.IsSuccess = false;
                    karzaPANDTO.Message = "Entered PAN not matched with uploaded image";
                    karzaPANDTO.Result = null;
                }
            }
            else
            {
                karzaPANDTO.IsSuccess = false;
                karzaPANDTO.Message = "PAN API down or data not found";
                karzaPANDTO.Result = null;
            }
            //karzaPANDTO.Result = karzaPanVerificationData.OtherInfo;
            return karzaPANDTO;
        }

        public async Task<ResultViewModel<long>> GetAndSaveByUniqueId(KYCActivityPAN kycActivityPAN, string userId, string CreatedBy, string productCode)
        {
            ResultViewModel<long> result = new ResultViewModel<long>();
            try
            {
                ResultViewModel<long> res = null;
                //await RemoveDocInfo(userId);
                ResultViewModel<bool> isValidatePAN = await ValidateByUniqueId(kycActivityPAN);
                if (isValidatePAN.Result)
                {
                    var karzaPANDTOData = await GetByUniqueId(kycActivityPAN);
                    if (karzaPANDTOData == null || karzaPANDTOData.IsSuccess == false)
                    {
                        result = new ResultViewModel<long>
                        {
                            IsSuccess = false,
                            Message = karzaPANDTOData.Message,
                            Result = 0
                        };
                        return result;
                    }
                    karzaPANDTOData.Result.DocumentId = kycActivityPAN.DocumentId.ToString();
                    if (karzaPANDTOData != null)
                    {
                        //await RemoveDocInfo(userId);
                        res = await SaveDoc(karzaPANDTOData.Result, userId, CreatedBy, productCode);
                        result = res;
                        //result = new ResultViewModel<long>
                        //{
                        //    IsSuccess = res > 0 ? true : false,
                        //    Message = res > 0 ? "Success" : "Failed to save",
                        //    Result = res
                        //};
                    }
                    else
                    {
                        result = new ResultViewModel<long>
                        {
                            IsSuccess = false,
                            Message = "Error while saving",
                            Result = 0
                        };
                    }
                }
                else
                {
                    result = new ResultViewModel<long>
                    {
                        IsSuccess = false,
                        Message = "Pan Already exists",
                        Result = 0
                    };
                }
            }
            catch (Exception ex)
            {
                result = new ResultViewModel<long>
                {
                    IsSuccess = false,
                    Message = "Error occurs",
                    Result = 0
                };
            }
            return result;
            //throw new NotImplementedException();
        }
    }
}
