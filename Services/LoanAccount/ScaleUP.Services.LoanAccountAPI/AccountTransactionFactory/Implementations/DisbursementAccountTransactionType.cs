using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;
using ScaleUP.Services.LoanAccountAPI.Managers;

namespace ScaleUP.Services.LoanAccountAPI.AccountTransactionFactory.Implementations
{
    public class DisbursementAccountTransactionType : BaseAccountTransactionType, IAccountTransactionType
    {
        //public readonly AccountTransactionManager _accountTransactionManager;
        public readonly PostDisbursementManager _postDisbursementManager;
        public readonly OrderPlacementManager _orderPlacementManager;

        //public DisbursementAccountTransactionType(AccountTransactionManager accountTransactionManager)
        public DisbursementAccountTransactionType(PostDisbursementManager postDisbursementManager, OrderPlacementManager orderPlacementManager)
        {
            //_accountTransactionManager = accountTransactionManager;
            _postDisbursementManager = postDisbursementManager;
            _orderPlacementManager = orderPlacementManager; 
        }

        public async Task<GRPCReply<long>> PostTransaction(GRPCRequest<ACT_PostAccountDisbursementRequestDC> request)
        {
            //var response = await _accountTransactionManager.PostAccountDisbursement(request);
            var response = await _postDisbursementManager.PostAccountDisbursement(request);
            return response;
        }
             

        public dynamic Run(dynamic account)
        {
            return "YES DisbursementAccountTransactionType";
        }
    }
}
