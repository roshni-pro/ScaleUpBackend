using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.KYCActivity;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Services.KYCAPI.Persistence;
using ScaleUP.Services.KYCDTO.Constant;
using ScaleUP.Services.KYCDTO.Transacion;
using ScaleUP.Services.KYCModels.Master;
using ScaleUP.Services.KYCModels.Transaction;
using static IdentityServer4.Models.IdentityResources;

namespace ScaleUP.Services.KYCAPI.KYCFactory.Implementation
{
    public class PersonalDetailDocType : BaseDocType, IDocType<PersonalDetailDTO, KYCPersonalDetailActivity>
    {
        public PersonalDetailDocType(ApplicationDbContext context, string userId, string filePath) : base(context, userId, filePath)
        {
            docTypeCode = KYCMasterConstants.PersonalDetail;
        }

        public async Task<ResultViewModel<long>> GetAndSaveByUniqueId(KYCPersonalDetailActivity input, string userId, string CreatedBy, string productCode)
        {
            ResultViewModel<long> result = null;
            try
            {
               
                ResultViewModel<long> res = null;
                PersonalDetailDTO personalDetailDTO = new PersonalDetailDTO()
                {
                    LeadMasterId = input.LeadMasterId,
                    AlternatePhoneNo = input.AlternatePhoneNo,
                    MiddleName = input.MiddleName,
                    EmailId = input.EmailId,
                    FatherLastName = input.FatherLastName,
                    FatherName = input.FatherName,
                    FirstName = input.FirstName,
                    LastName = input.LastName,
                    Gender = input.Gender,
                    CurrentAddressId = input.CurentLocationId,
                    PermanentAddressId = input.PermanentLocationId,
                    MobileNo = input.MobileNo,
                    Marital = input.Marital,   
                    OwnershipType = input.OwnershipType,
                    ElectricityBillDocumentId = input.ElectricityBillDocumentId,
                    OwnershipTypeAddress = input.OwnershipTypeAddress,
                    IVRSNumber = input.IVRSNumber,
                    OwnershipTypeName = input.OwnershipTypeName,
                    OwnershipTypeProof = input.OwnershipTypeProof,
                    OwnershipTypeResponseId = input.OwnershipTypeResponseId,
                    ElectricityServiceProvider  = input.ElectricityServiceProvider,
                    ElectricityState = input.ElectricityState
                };
                res = await SaveDoc(personalDetailDTO, userId, CreatedBy, productCode);
                if (res != null && input.OwnershipType == "Rented" && res.IsSuccess == true)
                {
                    result = new ResultViewModel<long>
                    {
                        IsSuccess = false,
                        Message = "Process block as ownership type is rented.",
                        Result = res.Result
                    };
                    return result;
                }
                result = res;
            }
            catch(Exception ex)
            {
                result = new ResultViewModel<long>
                {
                    IsSuccess =false,
                    Message = "Fail to save",
                    Result = 0
                };
            }
            return result;
        }

        public Task<ResultViewModel<PersonalDetailDTO>> GetByUniqueId(KYCPersonalDetailActivity input)
        {
            throw new NotImplementedException();
        }

        //public Task<Dictionary<string, dynamic>> GetDoc(string userId)
        //{
        //    throw new NotImplementedException();
        //}

        public ResultViewModel<bool> IsValidTOSave(PersonalDetailDTO doc, KYCMaster kycMaster, string userId, string productCode)
        {
            ResultViewModel<bool> res = null;
            var query = from m in Context.KYCMasters
                        join mi in Context.KYCMasterInfos on m.Id equals mi.KYCMasterId
                        join kd in Context.KYCDetailInfos on mi.Id equals kd.KYCMasterInfoId
                        join d in Context.KYCDetails on kd.KYCDetailId equals d.Id
                        where d.Field == KYCPersonalDetailConstants.EmailId
                            && mi.ProductCode == productCode
                            && mi.IsActive == true
                            && mi.IsDeleted == false
                            && m.Code == this.docTypeCode
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

        public async Task<ResultViewModel<long>> SaveDoc(PersonalDetailDTO doc, string userId, string CreatedBy, string productCode)
        {
            ResultViewModel<long> res = null;

            var personalId = Context.KYCMasters.Where(x => x.IsActive == true && x.IsDeleted == false && x.Code == KYCMasterConstants.PersonalDetail.ToString()).First();
            var thismasterInfo = await Context.KYCMasterInfos
                                    .Include("KYCDetailInfoList")
                                    .Where(x => x.UserId == userId && x.IsActive == true && x.IsDeleted == false && x.KYCMasterId == personalId.Id && x.ProductCode == productCode)
                                    .FirstOrDefaultAsync();

            var kycDetailQuery = from m in Context.KYCMasters
                                 join d in Context.KYCDetails on m.Id equals d.KYCMasterId
                                 where d.IsActive == true && d.IsDeleted == false
                                 && m.Code == KYCMasterConstants.PersonalDetail
                                 select d;
            var kycDetailList = kycDetailQuery.ToList();

            var kycMaster = Context.KYCMasters.Where(x => x.Code == KYCMasterConstants.PersonalDetail && x.IsActive == true && x.IsDeleted == false).First();

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

        private KYCDetailInfo GetDetail(PersonalDetailDTO doc, KYCDetail kYCDetail)
        {
            KYCDetailInfo kYCDetailInfo = new KYCDetailInfo
            {
                KYCDetailId = kYCDetail.Id,
                IsActive = true,
                IsDeleted = false
            };

            switch (kYCDetail.Field)
            {
                case Global.Infrastructure.Constants.KYCPersonalDetailConstants.FirstName:
                    kYCDetailInfo.FieldValue = doc.FirstName ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCPersonalDetailConstants.MiddleName:
                    kYCDetailInfo.FieldValue = doc.MiddleName ?? "";
                    break;

                case Global.Infrastructure.Constants.KYCPersonalDetailConstants.LastName:
                    kYCDetailInfo.FieldValue = doc.LastName ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCPersonalDetailConstants.FatherName:
                    kYCDetailInfo.FieldValue = doc.FatherName ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCPersonalDetailConstants.FatherLastName:
                    kYCDetailInfo.FieldValue = doc.FatherLastName ?? "";
                    break;
                //case Global.Infrastructure.Constants.KYCPersonalDetailConstants.DOB:
                //    kYCDetailInfo.FieldValue = doc.DOB ?? "";
                //    break;
                case Global.Infrastructure.Constants.KYCPersonalDetailConstants.Gender:
                    kYCDetailInfo.FieldValue = doc.Gender ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCPersonalDetailConstants.AlternatePhoneNo:
                    kYCDetailInfo.FieldValue = doc.AlternatePhoneNo ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCPersonalDetailConstants.EmailId:
                    kYCDetailInfo.FieldValue = doc.EmailId ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCPersonalDetailConstants.CurrentAddressId:
                    kYCDetailInfo.FieldValue = doc.CurrentAddressId.HasValue ? doc.CurrentAddressId.ToString() : "";
                    break;
                case Global.Infrastructure.Constants.KYCPersonalDetailConstants.PermanentAddressId:
                    kYCDetailInfo.FieldValue = doc.PermanentAddressId.HasValue ? doc.PermanentAddressId.ToString() : "";
                    break;
                case Global.Infrastructure.Constants.KYCPersonalDetailConstants.MobileNo:
                    kYCDetailInfo.FieldValue = doc.MobileNo ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCPersonalDetailConstants.OwnershipType:
                    kYCDetailInfo.FieldValue = doc.OwnershipType ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCPersonalDetailConstants.Marital:
                    kYCDetailInfo.FieldValue = doc.Marital ?? "";
                    break;
                default:
                case Global.Infrastructure.Constants.KYCPersonalDetailConstants.OwnershipTypeProof:
                    kYCDetailInfo.FieldValue = doc.OwnershipTypeProof ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCPersonalDetailConstants.IVRSNumber:
                    kYCDetailInfo.FieldValue = doc.IVRSNumber ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCPersonalDetailConstants.OwnershipTypeName:
                    kYCDetailInfo.FieldValue = doc.OwnershipTypeName ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCPersonalDetailConstants.OwnershipTypeAddress:
                    kYCDetailInfo.FieldValue = doc.OwnershipTypeAddress ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCPersonalDetailConstants.OwnershipTypeResponseId:
                    kYCDetailInfo.FieldValue = doc.OwnershipTypeResponseId ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCPersonalDetailConstants.ElectricityBillDocumentId:
                    kYCDetailInfo.FieldValue = doc.ElectricityBillDocumentId.ToString() ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCPersonalDetailConstants.ElectricityServiceProvider:
                    kYCDetailInfo.FieldValue = doc.ElectricityServiceProvider ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCPersonalDetailConstants.ElectricityState:
                    kYCDetailInfo.FieldValue = doc.ElectricityState ?? "";
                    break;
                    kYCDetailInfo.FieldValue = "";
                    break;
            }
            return kYCDetailInfo;

        }


        public Task<ResultViewModel<bool>> ValidateByUniqueId(KYCPersonalDetailActivity input)
        {
            throw new NotImplementedException();
        }

        public Task<ResultViewModel<long>> IsDocumentExist(KYCPersonalDetailActivity input, string userId)
        {
            throw new NotImplementedException();
        }
    }
}
