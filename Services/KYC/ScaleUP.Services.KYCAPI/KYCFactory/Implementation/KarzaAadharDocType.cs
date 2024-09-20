using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.Internal;
using Newtonsoft.Json;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.KYCActivity;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Services.KYCAPI.Helpers;
using ScaleUP.Services.KYCAPI.Managers;
using ScaleUP.Services.KYCAPI.Persistence;
using ScaleUP.Services.KYCDTO.Constant;
using ScaleUP.Services.KYCDTO.Enum;
using ScaleUP.Services.KYCDTO.Transacion;
using ScaleUP.Services.KYCModels.Master;
using ScaleUP.Services.KYCModels.Transaction;

namespace ScaleUP.Services.KYCAPI.KYCFactory.Implementation
{
    public class KarzaAadharDocType : BaseDocType, IDocType<KarzaAadharDTO, KYCActivityAadhar>
    {
        private IHostEnvironment _hostingEnvironment;
        public KarzaAadharDocType(ApplicationDbContext context, string userId, string filePath) : base(context, userId, filePath)
        {
            docTypeCode = KYCMasterConstants.Aadhar;
            _hostingEnvironment = new HostingEnvironment();
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

        public ResultViewModel<bool> IsValidTOSave(KarzaAadharDTO doc, KYCMaster kycMaster, string userId, string productCode)
        {
            HashHelper hashHelper = new HashHelper();
            var uniqueIdHash = hashHelper.QuickHash(doc.DocumentNumber.ToUpper());

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

           // var masterInfo = Context.KYCMasterInfos.Where(x => x.UniqueIdHash == uniqueIdHash && x.IsActive == true && x.IsDeleted == false && x.LastModified.Value.AddDays(kycMaster.ValidityDays) > DateTime.Today).OrderByDescending(y => y.LastModified).FirstOrDefault();
            if (doc == null || string.IsNullOrEmpty(doc.DocumentNumber))
            {
                res = new ResultViewModel<bool>
                {
                    IsSuccess = true,
                    Message = "Aadhar Number not fount",
                    Result = false
                };
                return res;
            }
            else if (masterInfo != null)
            {
                res = new ResultViewModel<bool>
                {
                    IsSuccess = true,
                    Message = "Aadhar Number already exists",
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

        public async Task<ResultViewModel<long>> SaveDoc(KarzaAadharDTO doc, string userId,string CreatedBy, string productCode)
        {
            HashHelper hashHelper = new HashHelper();
            var uniqueIdHash = hashHelper.QuickHash(doc.DocumentNumber.ToUpper());

            ResultViewModel<long> ExistId = null;

            var AAdharId = Context.KYCMasters.Where(x => x.IsActive == true && x.IsDeleted == false && x.Code == KYCMasterConstants.Aadhar.ToString()).First();
            var thismasterInfo = await Context.KYCMasterInfos
                                    .Include("KYCDetailInfoList")
                                    .Where(x => x.UserId == userId && x.IsActive == true && x.IsDeleted == false && x.KYCMasterId == AAdharId.Id && x.ProductCode == productCode)
                                    .FirstOrDefaultAsync();

            var kycDetailQuery = from m in Context.KYCMasters
                                 join d in Context.KYCDetails on m.Id equals d.KYCMasterId
                                 where d.IsActive == true && d.IsDeleted == false
                                 && m.Code == Global.Infrastructure.Constants.KYCMasterConstants.Aadhar
                                 select d;
            var kycDetailList = kycDetailQuery.ToList();

            var kycMaster = Context.KYCMasters.Where(x => x.Code == Global.Infrastructure.Constants.KYCMasterConstants.Aadhar && x.IsActive == true && x.IsDeleted == false).First();

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
                        UniqueId = doc.DocumentNumber.ToUpper(),
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
                    kYCMasterInfo.UniqueId = doc.DocumentNumber.ToUpper();
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

        private KYCDetailInfo GetDetail(KarzaAadharDTO doc, KYCDetail kYCDetail)
        {
            KYCDetailInfo kYCDetailInfo = new KYCDetailInfo
            {
                KYCDetailId = kYCDetail.Id,
                IsActive = true,
                IsDeleted = false
            };

            switch (kYCDetail.Field)
            {
                case Global.Infrastructure.Constants.KYCAadharConstants.GeneratedDateTime:
                    kYCDetailInfo.FieldValue = doc.GeneratedDateTime != null ? doc.GeneratedDateTime.ToString() : "";
                    break;
                case Global.Infrastructure.Constants.KYCAadharConstants.MaskedAadhaarNumber:
                    kYCDetailInfo.FieldValue = doc.MaskedAadhaarNumber ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCAadharConstants.Name:
                    kYCDetailInfo.FieldValue = doc.Name ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCAadharConstants.Gender:
                    kYCDetailInfo.FieldValue = doc.Gender ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCAadharConstants.DOB:
                    kYCDetailInfo.FieldValue = doc.DOB ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCAadharConstants.MobileHash:
                    kYCDetailInfo.FieldValue = doc.MobileHash ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCAadharConstants.EmailHash:
                    kYCDetailInfo.FieldValue = doc.EmailHash ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCAadharConstants.FatherName:
                    kYCDetailInfo.FieldValue = doc.FatherName ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCAadharConstants.HouseNumber:
                    kYCDetailInfo.FieldValue = doc.address.splitAddress.HouseNumber ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCAadharConstants.Street:
                    kYCDetailInfo.FieldValue = doc.address.splitAddress.Street ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCAadharConstants.Landmark:
                    kYCDetailInfo.FieldValue = doc.address.splitAddress.Landmark ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCAadharConstants.Subdistrict:
                    kYCDetailInfo.FieldValue = doc.address.splitAddress.Subdistrict ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCAadharConstants.District:
                    kYCDetailInfo.FieldValue = doc.address.splitAddress.District ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCAadharConstants.VtcName:
                    kYCDetailInfo.FieldValue = doc.address.splitAddress.VtcName ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCAadharConstants.Location:
                    kYCDetailInfo.FieldValue = doc.address.splitAddress.Location ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCAadharConstants.PostOffice:
                    kYCDetailInfo.FieldValue = doc.address.splitAddress.PostOffice ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCAadharConstants.State:
                    kYCDetailInfo.FieldValue = doc.address.splitAddress.State ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCAadharConstants.Country:
                    kYCDetailInfo.FieldValue = doc.address.splitAddress.Country ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCAadharConstants.Pincode:
                    kYCDetailInfo.FieldValue = doc.address.splitAddress.Pincode ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCAadharConstants.CombinedAddress:
                    kYCDetailInfo.FieldValue = doc.address.CombinedAddress ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCAadharConstants.Image:
                    kYCDetailInfo.FieldValue = doc.Image ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCAadharConstants.MaskedVID:
                    kYCDetailInfo.FieldValue = doc.MaskedVID ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCAadharConstants.File:
                    kYCDetailInfo.FieldValue = doc.File ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCAadharConstants.FrontDocumentId:
                    kYCDetailInfo.FieldValue = doc.FrontDocumentId ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCAadharConstants.BackDocumentId:
                    kYCDetailInfo.FieldValue = doc.BackDocumentId ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCAadharConstants.DocumentNumber:
                    kYCDetailInfo.FieldValue = doc.DocumentNumber ?? "";
                    break;
                case Global.Infrastructure.Constants.KYCAadharConstants.LocationId:
                    kYCDetailInfo.FieldValue = doc.GeneratedAddressId.HasValue ? doc.GeneratedAddressId.ToString() : "";
                    break;
                default:
                    kYCDetailInfo.FieldValue = "";
                    break;
            }
            return kYCDetailInfo;

        }
        public async Task<ResultViewModel<bool>> ValidateByUniqueId(KYCActivityAadhar kYCActivityAadhar)
        {
            ResultViewModel<bool> res = null;
            if (kYCActivityAadhar.DocumentNumber != null)
            {
                KarzaAadharHelper KarzaAadharHelper = new KarzaAadharHelper(Context);
                //string jsonString = JsonConvert.SerializeObject(GetAdharDc.Input);
                var karzaPanVerificationData = await KarzaAadharHelper.eAdharDigilockerOTPXml(kYCActivityAadhar, _filePath, _userId);
                if (karzaPanVerificationData.statusCode != 101 || karzaPanVerificationData.error != null) //101 mean succesfully 
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
            return res;
        }

        public async Task<ResultViewModel<KarzaAadharDTO>> GetByUniqueId(KYCActivityAadhar updateAadhaarVerificationRequestDC)
        {
            ResultViewModel<KarzaAadharDTO> karzaPANDTO = new ResultViewModel<KarzaAadharDTO>();
            if (updateAadhaarVerificationRequestDC.DocumentNumber != null)
            {
                //string jsonString = JsonConvert.SerializeObject(updateAadhaarVerificationRequestDC.Input);              
                KarzaAadharHelper karzahelper = new KarzaAadharHelper(Context);
                var karzapanverificationdata = await karzahelper.eAadharDigilockerVerifyOTPXml(updateAadhaarVerificationRequestDC, _filePath, userId: _userId);
                karzaPANDTO.Result = karzapanverificationdata.result.dataFromAadhaar;
                if(karzaPANDTO.Result != null)
                {
                   // karzaPANDTO.Result.DocumentNumber = updateAadhaarVerificationRequestDC.DocumentNumber;
                    karzaPANDTO.Result.FrontDocumentId = updateAadhaarVerificationRequestDC.FrontDocumentId;
                    karzaPANDTO.Result.BackDocumentId = updateAadhaarVerificationRequestDC.BackDocumentId;
                    karzaPANDTO.IsSuccess = true;
                    karzaPANDTO.Message = "Your Aadhar has been successfully verified.";
                }  
                else
                {
                    karzaPANDTO.IsSuccess = false;
                    karzaPANDTO.Message = !string.IsNullOrEmpty(karzapanverificationdata.statusMessage) ? karzapanverificationdata.statusMessage : "Your Aadhar could not be validated due to technical reason.Please re-try after sometime.";
                }
            }
            return karzaPANDTO;
        }

        public async Task<ResultViewModel<long>> GetAndSaveByUniqueId(KYCActivityAadhar getAdharDc, string userId, string CreatedBy, string productCode)
        {
            ResultViewModel<long> result = new ResultViewModel<long>();

            try
            {  
                ResultViewModel<long> res = null;
                if (getAdharDc.DocumentNumber != null)
                {
                    var karzaPANDTOData = getAdharDc.aadharInfo;
                    karzaPANDTOData.GeneratedAddressId = getAdharDc.aadharAddressId;

                    if (karzaPANDTOData != null)
                    {
                      //  await RemoveDocInfo(userId);
                        res = await SaveDoc(karzaPANDTOData, userId, CreatedBy, productCode);
                        result = res;

                        if(result != null && result.IsSuccess)
                        {
                            KycGrpcManager kycGrpcManager = new KycGrpcManager(Context, _hostingEnvironment);

                            var request = new GRPCRequest<KYCSpecificDetailRequest>();
                            var req = new List<KYCSpecificDetailUserRequest>();
                            req.Add(new KYCSpecificDetailUserRequest
                            {
                                UserId = userId,
                                ProductCode = productCode
                            });
                            request.Request = new KYCSpecificDetailRequest
                            {
                                UserIdList = req,
                                KYCReqiredFieldList = new Dictionary<string, List<string>>()
                            };

                            request.Request.KYCReqiredFieldList.Add(KYCMasterConstants.PAN, new List<string>
                            {
                                Global.Infrastructure.Constants.KYCDetailConstants.NameOnCard
                            });

                            var kycInfo = await kycGrpcManager.GetKYCSpecificDetail(request);

                            if(kycInfo != null && kycInfo.Any()) 
                            {
                                string aadharName = "";
                                string panName = "";
                                double matchingPercent = 0;
                                var userKycInfo = kycInfo.First().Value;
                                if(userKycInfo != null && userKycInfo.FirstOrDefault() != null)
                                {
                                    aadharName = RemoveWhitespace(userKycInfo.FirstOrDefault().FieldValue.ToLower().Trim());
                                    panName = RemoveWhitespace(karzaPANDTOData.Name.ToLower().Trim());
                                    var NameMatchPer = CalculateSimilarity(aadharName,panName);
                                    matchingPercent = Math.Round(NameMatchPer * 100);
                                }


                                //if(userKycInfo != null && userKycInfo.FirstOrDefault() != null && !string.IsNullOrEmpty(userKycInfo.FirstOrDefault().FieldValue) && userKycInfo.FirstOrDefault().FieldValue.ToLower().Trim() != karzaPANDTOData.Name.ToLower().Trim()) {
                                if (userKycInfo != null && userKycInfo.FirstOrDefault() != null && !string.IsNullOrEmpty(userKycInfo.FirstOrDefault().FieldValue) && matchingPercent < 100)
                                {
                                    result = new ResultViewModel<long>
                                    {
                                        IsSuccess = false,
                                        Message = "There is a mismatch in the information provided during the PAN and Aadhar KYC verification process, leading to an unsuccessful verification.",
                                        Result = 0
                                    };
                                    KYCMasterInfo selectedSavedAadharData = Context.KYCMasterInfos.FirstOrDefault(x => x.Id == res.Result && x.IsActive && !x.IsDeleted);
                                    if(selectedSavedAadharData !=null)
                                    {
                                        selectedSavedAadharData.IsActive = false;
                                        selectedSavedAadharData.IsDeleted = true;
                                        selectedSavedAadharData.LastModifiedBy = CreatedBy;
                                        Context.Entry(selectedSavedAadharData).State = EntityState.Modified;
                                        Context.SaveChanges();
                                    }

                                    return result;
                                }
                            }
                        }
                    }
                    else
                    {
                        result = new ResultViewModel<long>
                        {
                            IsSuccess = false,
                            Message = "Unable to fetch aadhar info from third party",
                            Result = 0
                        };
                    }
                    //}
                }
                else
                {
                    result = new ResultViewModel<long>
                    {
                        IsSuccess = false,
                        Message = "Document number not found",
                        Result = 0
                    };
                }
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

        public Task<ResultViewModel<long>> IsDocumentExist(KYCActivityAadhar input, string userId)
        {
            throw new NotImplementedException();
        }

        private static string RemoveWhitespace(string str)
        {
            return string.Join("", str.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
        }

        private static double CalculateSimilarity(string source, string target)
        {
            if ((source == null) || (target == null)) return 0.0;
            if ((source.Length == 0) || (target.Length == 0)) return 0.0;
            if (source == target) return 1.0;

            int stepsToSame = LevenshteinDistance(source, target);
            return (1.0 - ((double)stepsToSame / (double)Math.Max(source.Length, target.Length)));
        }
        private static int LevenshteinDistance(string source, string target)
        {
            // degenerate cases
            if (source == target) return 0;
            if (source.Length == 0) return target.Length;
            if (target.Length == 0) return source.Length;

            // create two work vectors of integer distances
            int[] v0 = new int[target.Length + 1];
            int[] v1 = new int[target.Length + 1];

            // initialize v0 (the previous row of distances)
            // this row is A[0][i]: edit distance for an empty s
            // the distance is just the number of characters to delete from t
            for (int i = 0; i < v0.Length; i++)
                v0[i] = i;

            for (int i = 0; i < source.Length; i++)
            {
                // calculate v1 (current row distances) from the previous row v0

                // first element of v1 is A[i+1][0]
                //   edit distance is delete (i+1) chars from s to match empty t
                v1[0] = i + 1;

                // use formula to fill in the rest of the row
                for (int j = 0; j < target.Length; j++)
                {
                    var cost = (source[i] == target[j]) ? 0 : 1;
                    v1[j + 1] = Math.Min(v1[j] + 1, Math.Min(v0[j + 1] + 1, v0[j] + cost));
                }

                // copy v1 (current row) to v0 (previous row) for next iteration
                for (int j = 0; j < v0.Length; j++)
                    v0[j] = v1[j];
            }

            return v1[target.Length];
        }
    }
}
