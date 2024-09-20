using Microsoft.EntityFrameworkCore;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.Global.Infrastructure.Constants.AccountTransaction;
using ScaleUP.Services.LoanAccountAPI.NBFCFactory;
using ScaleUP.Services.LoanAccountAPI.Persistence;
using System.Security.Cryptography;
using System.Text;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.AyeFinSCF;

namespace ScaleUP.Services.LoanAccountAPI.Helpers
{
    public class LoanAccountHelper
    {

        private readonly LoanAccountApplicationDbContext _context;
        private readonly LoanNBFCFactory _loanNBFCFactory;

        public LoanAccountHelper(LoanAccountApplicationDbContext context, LoanNBFCFactory loanNBFCFactory)
        {
            _loanNBFCFactory = loanNBFCFactory;
            _context = context;
        }
        public static string GetMd5Hash(MD5 md5Hash, string input)
        {
            input += Guid.NewGuid().ToString();
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
        public static string HashString(string text)
        {
            const string chars = "0234589ABCDEFGHOPQRTUWXYZ";
            byte[] bytes = Encoding.UTF8.GetBytes(text);

            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);

            char[] hash2 = new char[16];

            for (int i = 0; i < hash2.Length; i++)
            {
                hash2[i] = chars[hash[i] % chars.Length];
            }

            return new string(hash2);
        }
        public static string GenerateRandomOTP(int IOTPLength, string[] saAllowedCharacters)
        {
            string sOTP = String.Empty;
            string sTempChars = String.Empty;
            Random rand = new Random();

            for (int i = 0; i < IOTPLength; i++)
            {
                int p = rand.Next(0, saAllowedCharacters.Length);
                sTempChars = saAllowedCharacters[rand.Next(0, saAllowedCharacters.Length)];
                sOTP += sTempChars;
            }
            return sOTP;
        }

        public async Task<GRPCReply<LoanCreditLimit>> GetAvailableCreditLimitByLoanId(long LoanAccountId,string? NbfcToken="")
        {
            GRPCReply<LoanCreditLimit> res = new GRPCReply<LoanCreditLimit>();
            LoanCreditLimit data = new LoanCreditLimit();

            var LeadAccount = await _context.LoanAccounts.Where(x => x.Id == LoanAccountId && x.IsActive && !x.IsDeleted).Include(y => y.LoanAccountCredits).FirstOrDefaultAsync();
            if (LeadAccount != null)
            {
                if (!LeadAccount.IsAccountActive)
                {
                    data.IsBlock = true;
                    data.IsBlockHideLimit = LeadAccount.IsBlockHideLimit;
                    res.Message = "Your account is temporary inactive. Please contact customer care.";
                }
                if (LeadAccount.IsBlock)
                {
                    res.Message = "Your account is block. Please contact customer care.";
                    data.IsBlock = true;
                    data.IsBlockHideLimit = LeadAccount.IsBlockHideLimit;
                }

                //we always show limit form scaleup database 
                double IntransitPaidAmt = 0;
                if (LeadAccount.IsDefaultNBFC)
                {
                    IntransitPaidAmt = await _context.AccountTransactions.Where(x => x.LoanAccountId == LeadAccount.Id && x.IsActive && !x.IsDeleted && x.TransactionType.Code == TransactionTypesConstants.OrderPlacement
                                         && (x.TransactionStatus.Code == TransactionStatuseConstants.Initiate)).SumAsync(x => x.TransactionAmount);


                    data.CreditLimit = LeadAccount.LoanAccountCredits.CreditLimitAmount; // add credit limit column                
                    res.Status = true;
                }
                else
                {
                    IntransitPaidAmt = await _context.AccountTransactions.Where(x => x.LoanAccountId == LeadAccount.Id && x.IsActive && !x.IsDeleted && x.TransactionType.Code == TransactionTypesConstants.OrderPlacement
                                          && (x.TransactionStatus.Code == TransactionStatuseConstants.Initiate || x.TransactionStatus.Code == TransactionStatuseConstants.Intransit)).SumAsync(x => x.TransactionAmount);

                    var loanfactory = _loanNBFCFactory.GetService(LeadAccount.NBFCIdentificationCode);
                    if (!string.IsNullOrEmpty(NbfcToken) && LeadAccount.NBFCIdentificationCode == LeadNBFCConstants.AyeFinanceSCF )
                    {

                        var totalAvailableLmt = await loanfactory.CheckTotalAndAvailableLimit(new GRPCRequest<AyeloanReq> { Request = new AyeloanReq { loanId = LeadAccount.Id, token = NbfcToken } });
                        if (totalAvailableLmt.Status)
                        {
                            data.CreditLimit = totalAvailableLmt.Response.data.availableLimit;
                            data.TotalCreditLimit = totalAvailableLmt.Response.data.totalLimit;
                        }
                    }
                    else
                    {
                        data.CreditLimit = await loanfactory.GetAvailableCreditLimit(LeadAccount.Id);
                    }

                    res.Status = true;
                }
                if (data.CreditLimit > 0 && data.CreditLimit > IntransitPaidAmt)
                {
                    if (IntransitPaidAmt > 0)
                        data.CreditLimit -= IntransitPaidAmt;
                }
                else
                {
                    data.CreditLimit = 0;
                }
            }
            else
            {
                res.Status = true;
                res.Message = "Your account not found.";
            }

            res.Response = data;
            return res;
        }

    }
}
