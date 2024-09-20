using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;

namespace ScaleUP.Services.LoanAccountAPI.AccountTransactionFactory.Implementations
{
    public class OrderAccountTransactionType : BaseAccountTransactionType, IAccountTransactionType
    {
        public Task<GRPCReply<long>> PostTransaction(GRPCRequest<ACT_PostAccountDisbursementRequestDC> request)
        {
            throw new NotImplementedException();
        }

        public dynamic Run(dynamic account)
        {
            return "NO OrderAccountTransactionType";
        }
    }
}
