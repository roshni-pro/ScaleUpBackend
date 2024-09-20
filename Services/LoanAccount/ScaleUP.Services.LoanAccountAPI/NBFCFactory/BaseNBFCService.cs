using Microsoft.EntityFrameworkCore;
using ScaleUP.Global.Infrastructure.Constants.AccountTransaction;
using ScaleUP.Global.Infrastructure.Constants.Lead;
using ScaleUP.Services.LoanAccountAPI.Persistence;
using ScaleUP.Services.LoanAccountDTO.NBFC;
using ScaleUP.Services.LoanAccountModels.Transaction.NBFC;
using System.Formats.Asn1;

namespace ScaleUP.Services.LoanAccountAPI.NBFCFactory
{
    public abstract class BaseNBFCService
    {

        private readonly LoanAccountApplicationDbContext _context;

        public BaseNBFCService(LoanAccountApplicationDbContext context)
        {
            _context = context;
        }


        public async Task InsertNBFCComapnyAccountTransaction(long invoiceId, string nBFCIdentifcationCode, string transactionTypeCode)
        {
            List<NBFCCompanyAPIMaster> nBFCComapnyAPIMasters = null;

            if (!_context.NBFCComapnyAPIMasters.Any(x => x.InvoiceId == invoiceId && x.IsActive && !x.IsDeleted && x.IdentificationCode == nBFCIdentifcationCode && x.TransactionTypeCode == transactionTypeCode))
            {
                var nBFCComapnyApiDetails = await _context.NBFCCompanyAPIFlows.Where(x => x.NBFCIdentificationCode == nBFCIdentifcationCode && x.IsActive && !x.IsDeleted && x.TransactionTypeCode == transactionTypeCode)
                 .Select(x => new
                 {
                     NBFCCompanyAPIId = x.NBFCCompanyAPIId,
                     Sequence = x.Sequence,
                     IsActive = true,
                     IsDeleted = false,
                     Status = LeadNBFCApiConstants.NotStarted,
                     transactionTypeCode = x.TransactionTypeCode,
                     transactionStatuCode = x.TransactionStatusCode
                 }).ToListAsync();

                if (nBFCComapnyApiDetails != null && nBFCComapnyApiDetails.Any())
                {
                    nBFCComapnyAPIMasters = nBFCComapnyApiDetails.GroupBy(x =>
                                            new { transactionTypeCode = x.transactionTypeCode, transactionStatuCode = x.transactionStatuCode })
                                            .Select(x =>
                                            new NBFCCompanyAPIMaster
                                            {
                                                IdentificationCode = nBFCIdentifcationCode,
                                                TransactionStatuCode = x.Key.transactionStatuCode,
                                                TransactionTypeCode = x.Key.transactionTypeCode,
                                                InvoiceId = invoiceId,
                                                IsActive = true,
                                                IsDeleted = false,
                                                Status = LeadNBFCApiConstants.NotStarted,
                                                NBFCComapnyApiDetails = x.Select(y => new NBFCCompanyApiDetail
                                                {
                                                    IsActive = true,
                                                    IsDeleted = false,
                                                    NBFCCompanyAPIId = y.NBFCCompanyAPIId,
                                                    Sequence = y.Sequence,
                                                    Status = LeadNBFCApiConstants.NotStarted
                                                }).ToList()
                                            }).ToList();
                }


                if (nBFCComapnyAPIMasters != null)
                {
                    _context.NBFCComapnyAPIMasters.AddRange(nBFCComapnyAPIMasters);
                    _context.SaveChanges();
                }
            }
        }

        public async Task<NBFCCompanyAPIMasterDTO> GetNBFCCompanyAPIMaster(long invoiceId, string transactionTypeCode, string transactionStatuCode)
        {

            var query = from m in _context.NBFCComapnyAPIMasters.Where(x => x.InvoiceId == invoiceId && x.TransactionTypeCode == transactionTypeCode
                                                       && x.TransactionStatuCode == transactionStatuCode && x.IsActive && !x.IsDeleted)
                        join d in _context.NBFCComapnyApiDetails.Where(x => x.IsActive && !x.IsDeleted)
                           on m.Id equals d.NBFCComapnyApiMasterId
                        join c in _context.NBFCCompanyAPIs
                           on d.NBFCCompanyAPIId equals c.Id
                        group new { m, d, c } by new
                        {
                            NBFCCompanyAPIMasterId = m.Id,
                            InvoiceId = m.InvoiceId,
                            IdentificationCode = m.IdentificationCode,
                            Status = m.Status,
                            TransactionStatuCode = m.TransactionStatuCode,

                        } into grp
                        select new NBFCCompanyAPIMasterDTO
                        {
                            NBFCCompanyAPIMasterId = grp.Key.NBFCCompanyAPIMasterId,
                            InvoiceId = grp.Key.InvoiceId,
                            IdentificationCode = grp.Key.IdentificationCode,
                            Status = grp.Key.Status,
                            TransactionStatuCode = grp.Key.TransactionStatuCode,
                            NBFCCompanyAPIDetailDTOList = grp.ToList().Select(y => new NBFCCompanyAPIDetailDTO
                            {
                                APIUrl = y.c.APIUrl,
                                Code = y.c.Code,
                                NBFCCompanyAPIDetailId = y.d.Id,
                                Sequence = y.d.Sequence,
                                Status = y.d.Status,
                                TAPIKey = y.c.TAPIKey,
                                TAPISecretKey = y.c.TAPISecretKey,
                                TReferralCode = y.c.TReferralCode
                            }).ToList(),
                        };
            var nBFCCompanyAPIMaster = await query.FirstOrDefaultAsync();
            return nBFCCompanyAPIMaster;
        }

        public async Task<bool> UpdateNBFCCompanyMaster(NBFCCompanyAPIMasterDTO nBFCCompanyAPIMasterDTO)
        {
            var nBFCComapnyAPIMaster = await _context.NBFCComapnyAPIMasters.Where(x => x.Id == nBFCCompanyAPIMasterDTO.NBFCCompanyAPIMasterId).Include(x => x.NBFCComapnyApiDetails).FirstOrDefaultAsync();

            if (nBFCComapnyAPIMaster != null)
            {
                nBFCComapnyAPIMaster.Status = nBFCCompanyAPIMasterDTO.Status;
                foreach (var item in nBFCCompanyAPIMasterDTO.NBFCCompanyAPIDetailDTOList)
                {
                    var detail = nBFCComapnyAPIMaster.NBFCComapnyApiDetails.FirstOrDefault(x => x.Id == item.NBFCCompanyAPIDetailId);
                    if (detail != null)
                    {
                        detail.Status = item.Status;
                        _context.Entry(detail).State= EntityState.Modified;
                    }
                }

                nBFCComapnyAPIMaster.Status = nBFCComapnyAPIMaster.NBFCComapnyApiDetails.All(x => x.Status == LeadNBFCSubActivityConstants.Completed)? LeadNBFCSubActivityConstants.Completed: LeadNBFCSubActivityConstants.Error;
                _context.Entry(nBFCComapnyAPIMaster).State = EntityState.Modified;

            }

            return _context.SaveChanges() > 0;
        }

        public async Task<NBFCCompanyAPIDetailDTO> GetAvailableCreditLimit(string code)
        {
            var apiData = await _context.NBFCCompanyAPIs.Where(x => x.Code == code).Select(y => new NBFCCompanyAPIDetailDTO
            {
                APIUrl = y.APIUrl,
                Code = y.Code,
                NBFCCompanyAPIDetailId = 0,
                Sequence = 1,
                Status = "NotStarted",
                TAPIKey = y.TAPIKey,
                TAPISecretKey = y.TAPISecretKey,
                TReferralCode = y.TReferralCode
            }).FirstOrDefaultAsync();

            return apiData;
        }
    }
}
