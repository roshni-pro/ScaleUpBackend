using Microsoft.EntityFrameworkCore;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.KYCActivity;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Services.KYCAPI.Helpers;
using ScaleUP.Services.KYCAPI.Persistence;
using ScaleUP.Services.KYCDTO.Enum;
using System.Collections.Generic;

namespace ScaleUP.Services.KYCAPI.KYCFactory
{
    public abstract class BaseDocType
    {
        public readonly ApplicationDbContext Context;
        public readonly string _userId;
        public readonly string _filePath;
        public string docTypeCode { get; set; }
        public BaseDocType(ApplicationDbContext context, string userId, string filePath)
        {
            Context = context;
            _userId = userId;
            _filePath = filePath;
        }



        public async Task<ResultViewModel<Dictionary<string, dynamic>?>> GetDoc(string userId)
        {
            ResultViewModel<Dictionary<string, dynamic>> res = null; 
            var kycTypeId = await Context.KYCMasters.Where(x => x.Code == this.docTypeCode).Select(x => x.Id).FirstOrDefaultAsync();

            if (kycTypeId == 0)
            {
                res = new ResultViewModel<Dictionary<string, dynamic>>
                {
                    Result = null,
                    Message = "Failed",
                    IsSuccess = false
                };
                return res;
            }
            var masterInfo = Context.KYCMasterInfos.Where(x => x.UserId == userId && x.KYCMasterId == kycTypeId && x.IsActive == true && x.IsDeleted == false)
                .Include(y => y.KYCDetailInfoList)
                .ThenInclude(z => z.KYCDetail).OrderByDescending(y => y.LastModified).FirstOrDefault();
            if (masterInfo != null)
            {
                var master = Context.KYCMasters.Where(x => x.Id == masterInfo.KYCMasterId).FirstOrDefault();

                if (masterInfo.LastModified.Value.AddDays(master.ValidityDays) < DateTime.Today)
                {
                    masterInfo = null;
                }
            }

            if (masterInfo != null)
            {
                Dictionary<string, dynamic> keyValuePairs = new Dictionary<string, dynamic>();
                keyValuePairs.Add("userId", userId);
                keyValuePairs.Add("UniqueId", masterInfo.UniqueId);
                foreach (var item in masterInfo.KYCDetailInfoList)
                {
                    switch (item.KYCDetail.FieldType)
                    {
                        case (int)FieldTypeEnum.DateTime:
                            keyValuePairs.Add(item.KYCDetail.Field, DateFormatReturn(item.FieldValue));
                            break;
                        case (int)FieldTypeEnum.Double:
                            keyValuePairs.Add(item.KYCDetail.Field, double.Parse(item.FieldValue));
                            break;
                        case (int)FieldTypeEnum.String:
                            keyValuePairs.Add(item.KYCDetail.Field, item.FieldValue.ToString());
                            break;
                        case (int)FieldTypeEnum.Integer:
                            keyValuePairs.Add(item.KYCDetail.Field, Int32.Parse(item.FieldValue));
                            break;
                        case (int)FieldTypeEnum.Boolean:
                            keyValuePairs.Add(item.KYCDetail.Field, bool.Parse(item.FieldValue));
                            break;
                        default:
                            break;
                    }

                }
                res = new ResultViewModel<Dictionary<string, dynamic>>
                {
                    Result = keyValuePairs,
                    Message = "Success",
                    IsSuccess = true
                };
                return res;
            }
            res = new ResultViewModel<Dictionary<string, dynamic>>
            {
                Result = null,
                Message = "Failed",
                IsSuccess = false
            };
            return res;

        }

        public DateTime DateFormatReturn(string sdate)
        {
            DateTime date;
            string[] formats = { "dd/MM/yyyy", "dd/M/yyyy", "d/M/yyyy", "d/MM/yyyy",
                                "dd/MM/yy", "dd/M/yy", "d/M/yy", "d/MM/yy", "MM/dd/yyyy",

            "dd-MM-yyyy", "dd-M-yyyy", "d-M-yyyy", "d-MM-yyyy",
                                "dd-MM-yy", "dd-M-yy", "d-M-yy", "d-MM-yy", "MM-dd-yyyy"
                                , "yyyy-MM-dd", "yyyy/MM/dd"
            };

            DateTime.TryParseExact(sdate, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out date);
            return date;
        }

        public async Task<ResultViewModel<long>> IsDocumentExist(string UniqueId, string userId)
        {
            HashHelper hashHelper = new HashHelper();
            var uniqueIdHash = hashHelper.QuickHash(UniqueId);
            ResultViewModel<long> result = null;
            long res = 0;
            var masterInfo = await Context.KYCMasterInfos.Where(x => x.UniqueIdHash == uniqueIdHash && x.UserId == userId && x.IsActive == true && x.IsDeleted == false).OrderByDescending(y => y.LastModified).FirstOrDefaultAsync();
            if (masterInfo != null)
            {
                res = masterInfo.Id;
                result = new ResultViewModel<long>
                {
                    Result = res,
                    Message = "Success",
                    IsSuccess = true
                };
                return result;
            }
            else
            {
                result = new ResultViewModel<long>
                {
                    Result = res,
                    Message = "Success",
                    IsSuccess = true
                };
                return result;
            }
        }

        public async Task<ResultViewModel<bool>> RemoveDocInfo(string userId)
        {
            var query = from m in Context.KYCMasters
                        join mi in Context.KYCMasterInfos on m.Id equals mi.KYCMasterId
                        where mi.UserId == userId
                            && mi.IsActive == true
                            && mi.IsDeleted == false
                            && m.Code == this.docTypeCode
                        select mi;
            var masterInfo = query.FirstOrDefault();

            if (masterInfo != null)
            {
                var DetailQuery = from di in Context.KYCDetailInfos.Where(x => x.KYCMasterInfoId == masterInfo.Id && x.IsActive == true && x.IsDeleted == false)
                                  join d in Context.KYCDetails on di.KYCDetailId equals d.Id
                                  select new { di, d.Field };

                var DetailList = DetailQuery.ToList();
              
               // var details = Context.KYCDetailInfos.Where(x => x.KYCMasterInfoId == masterInfo.Id && x.IsActive == true && x.IsDeleted == false).ToList();

                masterInfo.IsActive = false;
                masterInfo.IsDeleted = true;
                masterInfo.LastModified = DateTime.Now;
                masterInfo.UniqueId = "";
                Context.Entry(masterInfo).State = EntityState.Modified;
                //if (details != null)
                //{
                //    foreach (var detail in details)
                //    {
                //        detail.IsActive = false;
                //        detail.IsDeleted = true;
                //        detail.LastModified = DateTime.Now;
                //        Context.Entry(detail).State = EntityState.Modified;
                //    }

                //}
                if (DetailList != null)
                {
                    foreach (var item in DetailList)
                    {
                        var detail = item.di;

                        if(item.Field==KYCAadharConstants.FrontDocumentId || item.Field == KYCAadharConstants.BackDocumentId || item.Field == KYCDetailConstants.DocumentId)
                        {
                            detail.FieldValue = "";
                        }
                        detail.IsActive = false;
                        detail.IsDeleted = true;
                        detail.LastModified = DateTime.Now;
                        Context.Entry(detail).State = EntityState.Modified;
                    }
                }

                Context.SaveChanges();
            }
            ResultViewModel<bool> res = new ResultViewModel<bool>
            {
                IsSuccess = true,
                Message = "Successs",
                Result = true
            };
            return res;
        }

    }
}
