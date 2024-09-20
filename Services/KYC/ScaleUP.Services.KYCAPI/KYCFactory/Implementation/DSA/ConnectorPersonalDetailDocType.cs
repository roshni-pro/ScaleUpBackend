
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
    public class ConnectorPersonalDetailDocType : BaseDocType, IDocType<ConnectorPersonalDetailDTO, KYCConnectorPersonalDetailActivity>
    {
        public ConnectorPersonalDetailDocType(ApplicationDbContext context, string userId, string filePath) : base(context, userId, filePath)
        {
            docTypeCode = KYCMasterConstants.ConnectorPersonalDetail;
        }

        public async Task<ResultViewModel<long>> GetAndSaveByUniqueId(KYCConnectorPersonalDetailActivity input, string userId, string CreatedBy, string productCode)
        {
            ResultViewModel<long> result = null;
            try
            {

                ResultViewModel<long> res = null;
                ConnectorPersonalDetailDTO personalDetailDTO = new ConnectorPersonalDetailDTO()
                {
                    LeadMasterId = input.LeadMasterId,
                    AlternatePhoneNo = input.AlternatePhoneNo,
                    EmailId = input.EmailId,
                    FatherName = input.FatherName,
                    //Address = input.Address,
                    //Age = input.Age,
                    //DOB = input.DOB,
                    FullName = input.FullName,
                    LanguagesKnown = input.LanguagesKnown,
                    PresentEmployment = input.PresentEmployment,
                    ReferenceName = input.ReferenceName,
                    WorkingLocation = input.WorkingLocation,
                    ReferneceContact = input.ReferneceContact,
                    WorkingWithOther = input.WorkingWithOther,
                    CurrentAddressId = input.PermanentLocationId,
                    MobileNo = input.MobileNo
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


        public ResultViewModel<bool> IsValidTOSave(ConnectorPersonalDetailDTO doc, KYCMaster kycMaster, string userId, string productCode)
        {
            ResultViewModel<bool> res = null;
            var query = from m in Context.KYCMasters
                        join mi in Context.KYCMasterInfos on m.Id equals mi.KYCMasterId
                        join kd in Context.KYCDetailInfos on mi.Id equals kd.KYCMasterInfoId
                        join d in Context.KYCDetails on kd.KYCDetailId equals d.Id
                        where d.Field == ConnectorPersonalDetailConstants.EmailId
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

        public async Task<ResultViewModel<long>> SaveDoc(ConnectorPersonalDetailDTO doc, string userId, string CreatedBy, string productCode)
        {
            ResultViewModel<long> res = null;

            var personalId = Context.KYCMasters.Where(x => x.IsActive == true && x.IsDeleted == false && x.Code == KYCMasterConstants.ConnectorPersonalDetail.ToString()).First();
            var thismasterInfo = await Context.KYCMasterInfos
                                    .Include("KYCDetailInfoList")
                                    .Where(x => x.UserId == userId && x.IsActive == true && x.IsDeleted == false && x.KYCMasterId == personalId.Id && x.ProductCode == productCode)
                                    .FirstOrDefaultAsync();

            var kycDetailQuery = from m in Context.KYCMasters
                                 join d in Context.KYCDetails on m.Id equals d.KYCMasterId
                                 where d.IsActive == true && d.IsDeleted == false
                                 && m.Code == KYCMasterConstants.ConnectorPersonalDetail
                                 select d;
            var kycDetailList = kycDetailQuery.ToList();

            var kycMaster = Context.KYCMasters.Where(x => x.Code == KYCMasterConstants.ConnectorPersonalDetail && x.IsActive == true && x.IsDeleted == false).First();

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

        private KYCDetailInfo GetDetail(ConnectorPersonalDetailDTO doc, KYCDetail kYCDetail)
        {
            KYCDetailInfo kYCDetailInfo = new KYCDetailInfo
            {
                KYCDetailId = kYCDetail.Id,
                IsActive = true,
                IsDeleted = false
            };

            switch (kYCDetail.Field)
            {
                case ConnectorPersonalDetailConstants.FullName:
                    kYCDetailInfo.FieldValue = doc.FullName ?? "";
                    break;
                case ConnectorPersonalDetailConstants.FatherName:
                    kYCDetailInfo.FieldValue = doc.FatherName ?? "";
                    break;
                //case Global.Infrastructure.Constants.DSA.ConnectorPersonalDetailConstants.dob:
                //    kYCDetailInfo.FieldValue = doc.DOB ?? "";
                //    break;
                //case Global.Infrastructure.Constants.DSA.ConnectorPersonalDetailConstants.age:
                //    kYCDetailInfo.FieldValue = doc.Age ?? "";
                //    break;
                case Global.Infrastructure.Constants.DSA.ConnectorPersonalDetailConstants.CurrentAddressId:
                    kYCDetailInfo.FieldValue = doc.CurrentAddressId.HasValue ? doc.CurrentAddressId.ToString() : "";
                    break;
                case ConnectorPersonalDetailConstants.AlternateContactNumber:
                    kYCDetailInfo.FieldValue = doc.AlternatePhoneNo ?? "";
                    break;
                case ConnectorPersonalDetailConstants.EmailId:
                    kYCDetailInfo.FieldValue = doc.EmailId ?? "";
                    break;
                case ConnectorPersonalDetailConstants.PresentEmployment:
                    kYCDetailInfo.FieldValue = doc.PresentEmployment ?? "";
                    break;
                case ConnectorPersonalDetailConstants.LanguagesKnown:
                    kYCDetailInfo.FieldValue = doc.LanguagesKnown ?? "";
                    break;
                case ConnectorPersonalDetailConstants.WorkingWithOther:
                    kYCDetailInfo.FieldValue = doc.WorkingWithOther ?? "";
                    break;
                default:
                case ConnectorPersonalDetailConstants.ReferneceName:
                    kYCDetailInfo.FieldValue = doc.ReferenceName ?? "";
                    break;
                case ConnectorPersonalDetailConstants.ReferneceContact:
                    kYCDetailInfo.FieldValue = doc.ReferneceContact ?? "";
                    break;
                case ConnectorPersonalDetailConstants.WorkingLocation:
                    kYCDetailInfo.FieldValue = doc.WorkingLocation ?? "";
                    break;
                case ConnectorPersonalDetailConstants.MobileNo:
                    kYCDetailInfo.FieldValue = doc.MobileNo ?? "";
                    break;
                    kYCDetailInfo.FieldValue = "";
                    break;
            }
            return kYCDetailInfo;

        }
        public Task<ResultViewModel<bool>> ValidateByUniqueId(KYCConnectorPersonalDetailActivity input)
        {
            throw new NotImplementedException();
        }
        public Task<ResultViewModel<ConnectorPersonalDetailDTO>> GetByUniqueId(KYCConnectorPersonalDetailActivity input)
        {
            throw new NotImplementedException();
        }
    }
}
