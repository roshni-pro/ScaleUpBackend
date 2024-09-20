using Microsoft.EntityFrameworkCore;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.DSA;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.KYCActivity;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Global.Infrastructure.Constants.DSA;
using ScaleUP.Services.KYCAPI.Persistence;
using ScaleUP.Services.KYCDTO.Transacion.DSA;
using ScaleUP.Services.KYCModels.Master;
using ScaleUP.Services.KYCModels.Transaction;

namespace ScaleUP.Services.KYCAPI.KYCFactory.Implementation.DSA
{
    public class DSAPersonalDetailDocType : BaseDocType, IDocType<DSAPersonalDetailDTO, KYCDSAPersonalDetailActivity>
    {
        public DSAPersonalDetailDocType(ApplicationDbContext context, string userId, string filePath) : base(context, userId, filePath)
        {
            docTypeCode = KYCMasterConstants.DSAPersonalDetail;
        }

        public async Task<ResultViewModel<long>> GetAndSaveByUniqueId(KYCDSAPersonalDetailActivity input, string userId, string CreatedBy, string productCode)
        {
            ResultViewModel<long> result = null;
            try
            {

                ResultViewModel<long> res = null;
                DSAPersonalDetailDTO personalDetailDTO = new DSAPersonalDetailDTO()
                {
                    LeadMasterId = input.LeadMasterId,
                    WorkingWithOther = input.WorkingWithOther,
                    LanguagesKnown = input.LanguagesKnown,
                    AlternatePhoneNo = input.AlternatePhoneNo,
                    CompanyName = input.CompanyName,
                    BuisnessDocument = input.BuisnessDocument,
                    FatherOrHusbandName = input.FatherOrHusbandName,
                    DocumentId = input.DocumentId,
                    //Address = input.Address,
                    //Age = input.Age,
                    //City = input.City,
                    NoOfYearsInCurrentEmployment = input.NoOfYearsInCurrentEmployment,
                    PresentOccupation = input.PresentOccupation,
                    GSTNumber = input.GSTNumber,
                    GSTRegistrationStatus = input.GSTRegistrationStatus,
                    EmailId = input.EmailId,
                    FirmType = input.FirmType,
                    //PinCode = input.PinCode,
                    FullName = input.FullName,
                    Qualification = input.Qualification,
                    //DOB = input.DOB,
                    //State = input.State,
                    WorkingLocation = input.WorkingLocation,
                    ReferneceContact = input.ReferneceContact,
                    ReferneceName = input.ReferneceName,
                    CurrentAddressId = input.CurentLocationId,
                    MobileNo = input.MobileNo,
                    PermanentLocationId = input.PermanentLocationId,
                    CompanyAddress = input.CompanyAddress,
                    CompanyCity = input.CompanyCity,
                    CompanyPincode = input.CompanyPincode,
                    CompanyState = input.CompanyState

                };
                res = await SaveDoc(personalDetailDTO, userId, CreatedBy, productCode);
                //if (res != null && input.OwnershipType == "Rented" && res.IsSuccess == true)
                //{
                //    result = new ResultViewModel<long>
                //    {
                //        IsSuccess = false,
                //        Message = "Process block as ownership type is rented.",
                //        Result = res.Result
                //    };
                //    return result;
                //}
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


        public ResultViewModel<bool> IsValidTOSave(DSAPersonalDetailDTO doc, KYCMaster kycMaster, string userId, string productCode)
        {
            ResultViewModel<bool> res = null;
            var query = from m in Context.KYCMasters
                        join mi in Context.KYCMasterInfos on m.Id equals mi.KYCMasterId
                        join kd in Context.KYCDetailInfos on mi.Id equals kd.KYCMasterInfoId
                        join d in Context.KYCDetails on kd.KYCDetailId equals d.Id
                        where d.Field == DSAPersonalDetailConstants.EmailId
                            && mi.ProductCode == productCode
                            && mi.IsActive == true
                            && mi.IsDeleted == false
                            && m.Code == docTypeCode
                            && kd.FieldValue == doc.EmailId
                            && mi.UserId != _userId
                            && kd.IsActive == true && kd.IsDeleted == false
                            && d.IsActive == true && d.IsDeleted == false
                        select mi;
            var masterInfo = query.ToList();
            if (doc == null || string.IsNullOrEmpty(doc.EmailId))
            {
                res = new ResultViewModel<bool>
                {
                    IsSuccess = false,
                    Message = "Not found!!",
                    Result = false
                };
                return res;
            }
            else if (masterInfo != null && masterInfo.Count >= 1)
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
                    IsSuccess = true,
                    Message = "Success",
                    Result = true
                };
            }
            return res;
        }

        public async Task<ResultViewModel<long>> SaveDoc(DSAPersonalDetailDTO doc, string userId, string CreatedBy, string productCode)
        {
            ResultViewModel<long> res = null;

            var personalId = Context.KYCMasters.Where(x => x.IsActive == true && x.IsDeleted == false && x.Code == KYCMasterConstants.DSAPersonalDetail.ToString()).First();
            var thismasterInfo = await Context.KYCMasterInfos
                                    .Include("KYCDetailInfoList")
                                    .Where(x => x.UserId == userId && x.IsActive == true && x.IsDeleted == false && x.KYCMasterId == personalId.Id && x.ProductCode == productCode)
                                    .FirstOrDefaultAsync();

            var kycDetailQuery = from m in Context.KYCMasters
                                 join d in Context.KYCDetails on m.Id equals d.KYCMasterId
                                 where d.IsActive == true && d.IsDeleted == false
                                 && m.Code == KYCMasterConstants.DSAPersonalDetail
                                 select d;
            var kycDetailList = kycDetailQuery.ToList();

            var kycMaster = Context.KYCMasters.Where(x => x.Code == KYCMasterConstants.DSAPersonalDetail && x.IsActive == true && x.IsDeleted == false).First();

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
            }
            return res;
        }

        private KYCDetailInfo GetDetail(DSAPersonalDetailDTO doc, KYCDetail kYCDetail)
        {
            KYCDetailInfo kYCDetailInfo = new KYCDetailInfo
            {
                KYCDetailId = kYCDetail.Id,
                IsActive = true,
                IsDeleted = false
            };

            switch (kYCDetail.Field)
            {
                case DSAPersonalDetailConstants.GSTRegistrationStatus:
                    kYCDetailInfo.FieldValue = doc.GSTRegistrationStatus ?? "";
                    break;
                case DSAPersonalDetailConstants.GSTNumber:
                    kYCDetailInfo.FieldValue = doc.GSTNumber ?? "";
                    break;

                case DSAPersonalDetailConstants.FirmType:
                    kYCDetailInfo.FieldValue = doc.FirmType ?? "";
                    break;
                case DSAPersonalDetailConstants.BuisnessDocument:
                    kYCDetailInfo.FieldValue = doc.BuisnessDocument ?? "";
                    break;
                case DSAPersonalDetailConstants.DocumentId:
                    kYCDetailInfo.FieldValue = doc.DocumentId ?? "";
                    break;
                case DSAPersonalDetailConstants.CompanyName:
                    kYCDetailInfo.FieldValue = doc.CompanyName ?? "";
                    break;
                case DSAPersonalDetailConstants.FullName:
                    kYCDetailInfo.FieldValue = doc.FullName ?? "";
                    break;
                case DSAPersonalDetailConstants.FatherOrHusbandName:
                    kYCDetailInfo.FieldValue = doc.FatherOrHusbandName ?? "";
                    break;
                case Global.Infrastructure.Constants.DSA.DSAPersonalDetailConstants.CurrentAddressId:
                    kYCDetailInfo.FieldValue = doc.CurrentAddressId.HasValue ? doc.CurrentAddressId.ToString() : "";
                    break;
                case Global.Infrastructure.Constants.DSA.DSAPersonalDetailConstants.PermanentAddressId:
                    kYCDetailInfo.FieldValue = doc.PermanentLocationId.HasValue ? doc.PermanentLocationId.ToString() : "";
                    break;
                case DSAPersonalDetailConstants.AlternatePhoneNo:
                    kYCDetailInfo.FieldValue = doc.AlternatePhoneNo ?? "";
                    break;
                case DSAPersonalDetailConstants.EmailId:
                    kYCDetailInfo.FieldValue = doc.EmailId ?? "";
                    break;
                case DSAPersonalDetailConstants.PresentOccupation:
                    kYCDetailInfo.FieldValue = doc.PresentOccupation ?? "";
                    break;
                default:
                case DSAPersonalDetailConstants.NoOfYearsInCurrentEmployment:
                    kYCDetailInfo.FieldValue = doc.NoOfYearsInCurrentEmployment ?? "";
                    break;
                case DSAPersonalDetailConstants.Qualification:
                    kYCDetailInfo.FieldValue = doc.Qualification ?? "";
                    break;
                case DSAPersonalDetailConstants.LanguagesKnown:
                    kYCDetailInfo.FieldValue = doc.LanguagesKnown ?? "";
                    break;
                case DSAPersonalDetailConstants.WorkingWithOther:
                    kYCDetailInfo.FieldValue = doc.WorkingWithOther ?? "";
                    break;
                case DSAPersonalDetailConstants.ReferneceName:
                    kYCDetailInfo.FieldValue = doc.ReferneceName ?? "";
                    break;
                case DSAPersonalDetailConstants.ReferneceContact:
                    kYCDetailInfo.FieldValue = doc.ReferneceContact ?? "";
                    break;
                case DSAPersonalDetailConstants.WorkingLocation:
                    kYCDetailInfo.FieldValue = doc.WorkingLocation ?? "";
                    break;
                case DSAPersonalDetailConstants.MobileNo:
                    kYCDetailInfo.FieldValue = doc.MobileNo ?? "";
                    break;

                    kYCDetailInfo.FieldValue = "";
                    break;
            }
            return kYCDetailInfo;

        }
        public Task<ResultViewModel<bool>> ValidateByUniqueId(KYCDSAPersonalDetailActivity input)
        {
            throw new NotImplementedException();
        }
        public Task<ResultViewModel<DSAPersonalDetailDTO>> GetByUniqueId(KYCDSAPersonalDetailActivity input)
        {
            throw new NotImplementedException();
        }

    }

}
