using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts;

namespace ScaleUP.Services.LoanAccountAPI.AccountTransactionFactory
{
    public interface IAccountTransactionType
    {
        public dynamic Run(dynamic account);

        public Task<GRPCReply<long>> PostTransaction(GRPCRequest<ACT_PostAccountDisbursementRequestDC> request);
    }
}
