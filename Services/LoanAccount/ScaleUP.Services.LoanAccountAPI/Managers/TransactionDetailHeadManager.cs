using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;
using ScaleUP.Services.LoanAccountAPI.Persistence;

namespace ScaleUP.Services.LoanAccountAPI.Managers
{
    public class TransactionDetailHeadManager
    {
        private readonly LoanAccountApplicationDbContext _context;
        public TransactionDetailHeadManager(LoanAccountApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<GRPCReply<LoanTransactionTypeAndDetailReply>> GetBYId(GRPCRequest<long> request)
        {
            GRPCReply<LoanTransactionTypeAndDetailReply> loanTransactionTypeAndDetailReply = new GRPCReply<LoanTransactionTypeAndDetailReply>();
            var input = _context.TransactionDetailHeads.Where(x => x.Id == request.Request && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
            if (input != null) 
            {
                loanTransactionTypeAndDetailReply.Response = new LoanTransactionTypeAndDetailReply();
                loanTransactionTypeAndDetailReply.Response.Id = input.Id;
                loanTransactionTypeAndDetailReply.Response.Code = input.Code;
                loanTransactionTypeAndDetailReply.Status = true;
                loanTransactionTypeAndDetailReply.Message = "Success";
            }
            else
            {
                loanTransactionTypeAndDetailReply.Status = false;
                loanTransactionTypeAndDetailReply.Message = "Not Found";
            }
            return loanTransactionTypeAndDetailReply;
        }

        public async Task<GRPCReply<LoanTransactionTypeAndDetailReply>> GetBYCode(GRPCRequest<string> request)
        {
            GRPCReply<LoanTransactionTypeAndDetailReply> loanTransactionTypeAndDetailReply = new GRPCReply<LoanTransactionTypeAndDetailReply>();
            var input = _context.TransactionDetailHeads.Where(x => x.Code == request.Request && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
            if (input != null)
            {
                loanTransactionTypeAndDetailReply.Response = new LoanTransactionTypeAndDetailReply();
                loanTransactionTypeAndDetailReply.Response.Id = input.Id;
                loanTransactionTypeAndDetailReply.Response.Code = input.Code;
                loanTransactionTypeAndDetailReply.Status = true;
                loanTransactionTypeAndDetailReply.Message = "Success";
            }
            else
            {
                loanTransactionTypeAndDetailReply.Status = false;
                loanTransactionTypeAndDetailReply.Message = "Not Found";
            }
            return loanTransactionTypeAndDetailReply;
        }
    }
}
