using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Global.Infrastructure.Constants.Lead;
using ScaleUP.Global.Infrastructure.Constants.Product;
using ScaleUP.Global.Infrastructure.Enum;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using ScaleUP.Services.LeadAPI.Persistence;
using ScaleUP.Services.LeadDTO.Lead;
using ScaleUP.Services.LeadModels;
using static iTextSharp.text.pdf.AcroFields;

namespace ScaleUP.Services.LeadAPI.Manager
{
    public class LeadBankDetailManager
    {
        private readonly LeadApplicationDbContext _context;
        private readonly LeadHistoryManager _leadHistoryManager;
        private readonly IMassTransitService _massTransitService;
        public LeadBankDetailManager(LeadApplicationDbContext context, LeadHistoryManager leadHistoryManager, IMassTransitService massTransitService)
        {
            _context = context;
            _leadHistoryManager = leadHistoryManager;
            _massTransitService = massTransitService;
        }
        #region Old Code
        //public async Task<ResultViewModel<bool>> SaveLeadBankDetail(LeadBankInfoDTO leadBankInfoDTO)
        //{
        //    ResultViewModel<bool> res = new ResultViewModel<bool>();


        //    var info = leadBankInfoDTO.leadBankDetailDTOs.FirstOrDefault();
        //    if (info == null || !leadBankInfoDTO.leadBankDetailDTOs.Any())
        //    {
        //        res = new ResultViewModel<bool>
        //        {
        //            Result = false,
        //            IsSuccess = false,
        //            Message = "Something went wrong!"
        //        };
        //        return res;
        //    }



        //    var Lead = await _context.Leads.FirstOrDefaultAsync(x => x.Id == info.LeadId);

        //    foreach (var item in leadBankInfoDTO.leadBankDetailDTOs)
        //    {

        //        var IsExists = await _context.LeadBankDetails.AnyAsync(x => x.AccountNumber == item.AccountNumber
        //                                        && x.IFSCCode == item.IFSCCode && x.IsActive && !x.IsDeleted
        //                                        && x.Leads.ProductId == Lead.ProductId
        //                                        && x.LeadId != info.LeadId);
        //        if (IsExists)
        //        {

        //            res = new ResultViewModel<bool>
        //            {
        //                Result = false,
        //                IsSuccess = false,
        //                Message = "Account number already exists! For Acc No : " + item.AccountNumber
        //            };
        //            return res;

        //        }
        //    }


        //    var ExistingList = await _context.LeadBankDetails.Where(x => x.LeadId == info.LeadId && x.IsActive && !x.IsDeleted).ToListAsync();
        //    if (ExistingList != null && ExistingList.Any())
        //    {
        //        foreach (var item in ExistingList)
        //        {
        //            item.IsActive = false;
        //            item.IsDeleted = true;
        //            _context.Entry(item).State = EntityState.Modified;


        //        }
        //        await _context.SaveChangesAsync();
        //    }

        //    try
        //    {
        //        var ExistingDocList = await _context.LeadDocumentDetails.Where(x => x.LeadId == info.LeadId && x.IsActive && !x.IsDeleted && (x.DocumentName == BlackSoilBusinessDocNameConstants.SurrogateGstCertificate || x.DocumentName == BlackSoilBusinessDocNameConstants.SurrogateITRCertificate || x.DocumentName == BlackSoilBusinessDocNameConstants.Statement)).ToListAsync();

        //    if (ExistingDocList != null && ExistingDocList.Any())
        //    {
        //        foreach (var item in ExistingDocList)
        //        {
        //            item.IsActive = false;
        //            item.IsDeleted = true;
        //            _context.Entry(item).State = EntityState.Modified;
        //        }
        //        await _context.SaveChangesAsync();
        //    }

        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    List<LeadBankDetail> AddleadBankDetails = new List<LeadBankDetail>();
        //    List<LeadDocumentDetail> AddleadDocumentDetail = new List<LeadDocumentDetail>();


        //    foreach (var item in leadBankInfoDTO.leadBankDetailDTOs)
        //    {
        //        LeadBankDetail leadBankDetail = new LeadBankDetail
        //        {
        //            IFSCCode = item.IFSCCode,
        //            IsActive = true,
        //            IsDeleted = false,
        //            AccountType = item.AccountType,
        //            BankName = item.BankName,
        //            Type = item.Type,
        //            LeadId = item.LeadId,
        //            AccountHolderName = item.AccountHolderName,
        //            AccountNumber = item.AccountNumber,
        //            PdfPassword = item.PdfPassword,
        //            SurrogateType = item.SurrogateType
        //        };
        //        AddleadBankDetails.Add(leadBankDetail);
        //    }
        //    await _context.AddRangeAsync(AddleadBankDetails);

        //    foreach (var item in leadBankInfoDTO.BankDocs)
        //    {
        //        LeadDocumentDetail leadDocumentDetail = new LeadDocumentDetail
        //        {
        //           DocumentName = item.DocumentName,
        //           DocumentType = item.DocumentType,
        //           FileUrl = item.FileURL,
        //           IsActive = true,
        //           IsDeleted = false,
        //           LeadId = Lead.Id,
        //           PdfPassword  = item.PdfPassword,
        //           Created = DateTime.Now,
        //           DocumentNumber = item.DocumentNumber,
        //           Sequence = item.Sequence,
        //        };
        //        AddleadDocumentDetail.Add(leadDocumentDetail);
        //    }
        //    await _context.AddRangeAsync(AddleadDocumentDetail);


        //    await _context.SaveChangesAsync();
        //    var leadActivityMasterProgresses = await _context.LeadActivityMasterProgresses.FirstOrDefaultAsync(x => x.LeadMasterId == info.LeadId &&
        //                                                                              x.ActivityMasterId == info.ActivityId &&
        //                                                                              x.SubActivityMasterId == info.SubActivityId &&
        //                                                                              x.IsActive && !x.IsDeleted);
        //    if (leadActivityMasterProgresses != null)
        //    {
        //        leadActivityMasterProgresses.IsCompleted = true;
        //        if (leadActivityMasterProgresses.IsApproved == 2)
        //        {
        //            leadActivityMasterProgresses.IsApproved = 0;
        //        }
        //        _context.Entry(leadActivityMasterProgresses).State = EntityState.Modified;

        //        if(Lead.ProductCode == ProductCodeConstants.DSA)
        //        {
        //            Lead.Status = LeadStatusEnum.Submitted.ToString();
        //            _context.Entry(Lead).State = EntityState.Modified;
        //        }

        //    }

        //    await _context.SaveChangesAsync();
        //    res = new ResultViewModel<bool>
        //    {
        //        Result = true,
        //        IsSuccess = true,
        //        Message = "SuccesFully!"
        //    };


        //    //------------------S : Make log---------------------
        //    #region Make History
        //    var result = await _leadHistoryManager.GetLeadHistroy(info.LeadId, "SaveLeadBankDetail");
        //    LeadUpdateHistoryEvent histroyEvent = new LeadUpdateHistoryEvent
        //    {
        //        LeadId = info.LeadId,
        //        UserID = result.UserId,
        //        UserName = "",
        //        EventName = "SaveLeadBankDetail",//context.Message.KYCMasterCode, //result.EntityIDofKYCMaster.ToString(),
        //        Narretion = result.Narretion,
        //        NarretionHTML = result.NarretionHTML,
        //        CreatedTimeStamp = result.CreatedTimeStamp
        //    };
        //    await _massTransitService.Publish(histroyEvent);
        //    #endregion
        //    //------------------E : Make log---------------------

        //    return res;

        //}
        #endregion
        #region New Code
        public async Task<ResultViewModel<bool>> SaveLeadBankDetail(LeadBankInfoDTO leadBankInfoDTO,string UserId)
        {
            ResultViewModel<bool> res = new ResultViewModel<bool>();
            List<LeadBankDetail> AddleadBankDetails = new List<LeadBankDetail>();
            List<LeadDocumentDetail> AddleadDocumentDetail = new List<LeadDocumentDetail>();

            var info = leadBankInfoDTO.leadBankDetailDTOs.FirstOrDefault();
            if (info == null || !leadBankInfoDTO.leadBankDetailDTOs.Any())
            {
                res = new ResultViewModel<bool>
                {
                    Result = false,
                    IsSuccess = false,
                    Message = "Something went wrong!"
                };
                return res;
            }



            var Lead = await _context.Leads.FirstOrDefaultAsync(x => x.Id == info.LeadId);

            foreach (var item in leadBankInfoDTO.leadBankDetailDTOs)
            {

                var IsExists = await _context.LeadBankDetails.AnyAsync(x => x.AccountNumber == item.AccountNumber
                                                && x.IFSCCode == item.IFSCCode && x.IsActive && !x.IsDeleted
                                                && x.Leads.ProductId == Lead.ProductId
                                                && x.LeadId != info.LeadId);
                if (IsExists)
                {

                    res = new ResultViewModel<bool>
                    {
                        Result = false,
                        IsSuccess = false,
                        Message = "Account number already exists! For Acc No : " + item.AccountNumber
                    };
                    return res;

                }
            }


            var ExistingList = await _context.LeadBankDetails.Where(x => x.LeadId == info.LeadId && x.IsActive && !x.IsDeleted).ToListAsync();
            if (ExistingList != null && ExistingList.Any())
            {                
                foreach (var item in leadBankInfoDTO.leadBankDetailDTOs)
                {
                    var borrowerExistList = ExistingList.FirstOrDefault(x => x.Type == item.Type);
                    if(borrowerExistList != null)
                    {
                        borrowerExistList.AccountHolderName = item.AccountHolderName;
                        borrowerExistList.AccountNumber = item.AccountNumber;
                        borrowerExistList.AccountType = item.AccountType;
                        borrowerExistList.BankName = item.BankName;
                        borrowerExistList.IFSCCode = item.IFSCCode;
                        borrowerExistList.LeadId = item.LeadId;
                        borrowerExistList.PdfPassword = item.PdfPassword;
                        borrowerExistList.SurrogateType = item.SurrogateType;
                        borrowerExistList.Type = item.Type;
                        borrowerExistList.LastModified = DateTime.Now;
                        borrowerExistList.LastModifiedBy = UserId;
                        _context.Entry(borrowerExistList).State = EntityState.Modified;
                    }
                    else
                    {
                        LeadBankDetail leadBankDetail = new LeadBankDetail
                        {
                            IFSCCode = item.IFSCCode,
                            IsActive = true,
                            IsDeleted = false,
                            AccountType = item.AccountType,
                            BankName = item.BankName,
                            Type = item.Type,
                            LeadId = item.LeadId,
                            AccountHolderName = item.AccountHolderName,
                            AccountNumber = item.AccountNumber,
                            PdfPassword = item.PdfPassword,
                            SurrogateType = item.SurrogateType
                        };
                        _context.Add(leadBankDetail);
                    }                
                }                
            }
            else
            {
                foreach (var item in leadBankInfoDTO.leadBankDetailDTOs)
                {
                    LeadBankDetail leadBankDetail = new LeadBankDetail
                    {
                        IFSCCode = item.IFSCCode,
                        IsActive = true,
                        IsDeleted = false,
                        AccountType = item.AccountType,
                        BankName = item.BankName,
                        Type = item.Type,
                        LeadId = item.LeadId,
                        AccountHolderName = item.AccountHolderName,
                        AccountNumber = item.AccountNumber,
                        PdfPassword = item.PdfPassword,
                        SurrogateType = item.SurrogateType
                    };
                    AddleadBankDetails.Add(leadBankDetail);
                }
                await _context.AddRangeAsync(AddleadBankDetails);
                
            }

            try
            {
                var ExistingDocList = await _context.LeadDocumentDetails.Where(x => x.LeadId == info.LeadId && x.IsActive && !x.IsDeleted && (x.DocumentName == BlackSoilBusinessDocNameConstants.SurrogateGstCertificate || x.DocumentName == BlackSoilBusinessDocNameConstants.SurrogateITRCertificate || x.DocumentName == BlackSoilBusinessDocNameConstants.Statement)).ToListAsync();

                if (ExistingDocList != null && ExistingDocList.Any())
                {
                    List<long?> DocIds = new List<long?>();
                    DocIds = leadBankInfoDTO.BankDocs.Select(x=>x.DocId).ToList();
                    var deactivateDoc = ExistingDocList.Where(x => !DocIds.Contains(x.Id));
                    if (deactivateDoc != null)
                    {
                        foreach (var item in deactivateDoc)
                        {
                            item.IsActive = false;
                            item.IsDeleted = true;
                            item.LastModified = DateTime.UtcNow;
                            item.LastModifiedBy = UserId;
                            _context.Entry(item).State = EntityState.Modified;
                            _context.SaveChanges();
                        }
                    }
                    foreach (var docitem in leadBankInfoDTO.BankDocs)
                    {
                        var DocExistingData = ExistingDocList.FirstOrDefault(x=>x.Id == docitem.DocId);
                        if(DocExistingData != null)
                        {
                            DocExistingData.DocumentName = docitem.DocumentName;
                            DocExistingData.LeadId = info.LeadId;
                            DocExistingData.DocumentNumber = docitem.DocumentNumber;
                            DocExistingData.DocumentType = docitem.DocumentType;
                            DocExistingData.FileUrl = docitem.FileURL;
                            DocExistingData.PdfPassword = docitem.PdfPassword;
                            DocExistingData.Sequence = docitem.Sequence;
                            DocExistingData.LastModified = DateTime.Now;
                            DocExistingData.LastModifiedBy = UserId;
                            _context.Entry(DocExistingData).State = EntityState.Modified;
                        }
                        else
                        {
                            LeadDocumentDetail leadDocumentDetail = new LeadDocumentDetail
                            {
                                DocumentName = docitem.DocumentName,
                                DocumentType = docitem.DocumentType,
                                FileUrl = docitem.FileURL,
                                IsActive = true,
                                IsDeleted = false,
                                LeadId = Lead.Id,
                                PdfPassword = docitem.PdfPassword,
                                Created = DateTime.Now,
                                DocumentNumber = docitem.DocumentNumber,
                                Sequence = docitem.Sequence,
                            };
                            _context.Add(leadDocumentDetail);
                        }                   
                    }
                    await _context.SaveChangesAsync();
                }
                else
                {
                    foreach (var item in leadBankInfoDTO.BankDocs)
                    {
                        LeadDocumentDetail leadDocumentDetail = new LeadDocumentDetail
                        {
                            DocumentName = item.DocumentName,
                            DocumentType = item.DocumentType,
                            FileUrl = item.FileURL,
                            IsActive = true,
                            IsDeleted = false,
                            LeadId = Lead.Id,
                            PdfPassword = item.PdfPassword,
                            Created = DateTime.Now,
                            DocumentNumber = item.DocumentNumber,
                            Sequence = item.Sequence,
                        };
                        AddleadDocumentDetail.Add(leadDocumentDetail);
                    }
                    await _context.AddRangeAsync(AddleadDocumentDetail);

                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            var leadActivityMasterProgresses = await _context.LeadActivityMasterProgresses.FirstOrDefaultAsync(x => x.LeadMasterId == info.LeadId &&
                                                                                      x.ActivityMasterId == info.ActivityId &&
                                                                                      x.SubActivityMasterId == info.SubActivityId &&
                                                                                      x.IsActive && !x.IsDeleted);
            if (leadActivityMasterProgresses != null)
            {
                leadActivityMasterProgresses.IsCompleted = true;
                if (leadActivityMasterProgresses.IsApproved == 2)
                {
                    leadActivityMasterProgresses.IsApproved = 0;
                }
                _context.Entry(leadActivityMasterProgresses).State = EntityState.Modified;


                if (Lead.ProductCode.ToUpper() == ProductCodeConstants.DSA.ToUpper())
                {
                    Lead.Status = LeadStatusEnum.Submitted.ToString();
                    _context.Entry(Lead).State = EntityState.Modified;
                }

            }

            await _context.SaveChangesAsync();
            res = new ResultViewModel<bool>
            {
                Result = true,
                IsSuccess = true,
                Message = "SuccesFully!"
            };


            //------------------S : Make log---------------------
            #region Make History
            var result = await _leadHistoryManager.GetLeadHistroy(info.LeadId, "SaveLeadBankDetail");
            LeadUpdateHistoryEvent histroyEvent = new LeadUpdateHistoryEvent
            {
                LeadId = info.LeadId,
                UserID = result.UserId,
                UserName = "",
                EventName = "SaveLeadBankDetail",//context.Message.KYCMasterCode, //result.EntityIDofKYCMaster.ToString(),
                Narretion = result.Narretion,
                NarretionHTML = result.NarretionHTML,
                CreatedTimeStamp = result.CreatedTimeStamp
            };
            await _massTransitService.Publish(histroyEvent);
            #endregion
            //------------------E : Make log---------------------

            return res;

        }

        #endregion
        public async Task<ResultViewModel<List<LeadBankDetailDTO>>> GetLeadBankDetail(long LeadId)
        {
            ResultViewModel<List<LeadBankDetailDTO>> res = new ResultViewModel<List<LeadBankDetailDTO>>();
            //LeadBankDetailDTO leadBankDetailDTO = new LeadBankDetailDTO();
            var list = new List<LeadBankDetailDTO>();
            var GetList = _context.LeadBankDetails.Where(x => x.LeadId == LeadId && x.IsActive && !x.IsDeleted).ToList();
            if (GetList.Count > 0)
            {
                //leadBankDetailDTO.Channel = GetList.Channel;
                foreach (var item in GetList)
                {
                    var obj = new LeadBankDetailDTO
                    {
                        BankName = item.BankName,
                        Type = item.Type,
                        LeadId = item.LeadId,
                        IFSCCode = item.IFSCCode,
                        AccountType = item.AccountType,
                        AccountHolderName = item.AccountHolderName,
                        AccountNumber = item.AccountNumber,
                        PdfPassword = item.PdfPassword,
                        SurrogateType = item.SurrogateType,
                    };
                    list.Add(obj);
                }
                res = new ResultViewModel<List<LeadBankDetailDTO>>
                {
                    Result = list,
                    IsSuccess = true,
                    Message = ""
                };
            }
            else
            {
                res = new ResultViewModel<List<LeadBankDetailDTO>>
                {
                    Result = null,
                    IsSuccess = false,
                    Message = "Data not found!"
                };
            }
            return res;
        }

        public async Task<ResultViewModel<List<LeadDocumentDetailDTO>>> GetLeadDocumentDetail(long LeadId)
        {
            ResultViewModel<List<LeadDocumentDetailDTO>> res = new ResultViewModel<List<LeadDocumentDetailDTO>>();
            var list = new List<LeadDocumentDetailDTO>();
            var GetList = _context.LeadDocumentDetails.Where(x => x.LeadId == LeadId && x.IsActive && !x.IsDeleted && (x.DocumentName == BlackSoilBusinessDocNameConstants.SurrogateGstCertificate || x.DocumentName == BlackSoilBusinessDocNameConstants.SurrogateITRCertificate || x.DocumentName == BlackSoilBusinessDocNameConstants.Statement)).OrderByDescending(x=>x.Id).ToList();
            if (GetList.Count > 0)
            {
                //leadBankDetailDTO.Channel = GetList.Channel;
                foreach (var item in GetList)
                {
                    var obj = new LeadDocumentDetailDTO
                    {
                        DocumentName = item.DocumentName,
                        FileUrl = item.FileUrl,
                        LeadId = item.LeadId,
                        Sequence = item.Sequence,
                        DocumentNumber = item.DocumentNumber,
                        DocId = item.Id
                    };
                    list.Add(obj);
                }
                res = new ResultViewModel<List<LeadDocumentDetailDTO>>
                {
                    Result = list,
                    IsSuccess = true,
                    Message = ""
                };
            }
            else
            {
                res = new ResultViewModel<List<LeadDocumentDetailDTO>>
                {
                    Result = null,
                    IsSuccess = false,
                    Message = "Data not found!"
                };
            }
            return res;
        }

        public async Task<ResultViewModel<LeadBankInfoDTO>> GetBankDetail(long LeadId)
        {
            ResultViewModel<LeadBankInfoDTO> res = new ResultViewModel<LeadBankInfoDTO>();
            var leadBankInfo = new LeadBankInfoDTO();
            var list = new List<LeadBankDetailDTO>();
            var bankDocList = new List<BankDoc>();
            var bankDetails = _context.LeadBankDetails.Where(x => x.LeadId == LeadId && x.IsActive && !x.IsDeleted).ToList();
            if(bankDetails != null && bankDetails.Any())
            {
                foreach (var item in bankDetails)
                {
                    var bankData = new LeadBankDetailDTO
                    {
                        LeadId = LeadId,
                        AccountHolderName = item.AccountHolderName,
                        AccountNumber = item.AccountNumber,
                        AccountType = item.AccountType,
                        BankName = item.BankName,
                        Type = item.Type,
                        IFSCCode = item.IFSCCode,
                        PdfPassword = item.PdfPassword,
                        SurrogateType = item.SurrogateType,
                    };
                    list.Add(bankData);
                }
                leadBankInfo.leadBankDetailDTOs = list;
            }

            var GetList = _context.LeadDocumentDetails.Where(x => x.LeadId == LeadId && x.IsActive && !x.IsDeleted && (x.DocumentName == BlackSoilBusinessDocNameConstants.SurrogateGstCertificate || x.DocumentName == BlackSoilBusinessDocNameConstants.SurrogateITRCertificate || x.DocumentName == BlackSoilBusinessDocNameConstants.Statement)).OrderByDescending(x => x.Id).ToList();
            if (GetList.Count > 0)
            {
                foreach (var item in GetList)
                {
                    var obj = new BankDoc
                    {
                        DocumentName = item.DocumentName,
                        FileURL = item.FileUrl,
                        Sequence = item.Sequence,
                        DocumentNumber = item.DocumentNumber,
                        DocumentType = item.DocumentType,
                        PdfPassword = item.PdfPassword,
                        DocId = item.Id
                    };
                    bankDocList.Add(obj);
                }
                leadBankInfo.BankDocs = bankDocList;
                res = new ResultViewModel<LeadBankInfoDTO>
                {
                    Result = leadBankInfo,
                    IsSuccess = true,
                    Message = ""
                };
            }
            else
            {
                res = new ResultViewModel<LeadBankInfoDTO>
                {
                    Result = null,
                    IsSuccess = false,
                    Message = "Data not found!"
                };
            }
            return res;
        }


    }
}
