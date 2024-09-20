using ScaleUP.Global.Infrastructure.Constants.AccountLocation;
using ScaleUP.Services.LoanAccountAPI.AccountTransactionFactory.Implementations;

namespace ScaleUP.Services.LoanAccountAPI.AccountTransactionFactory
{
    public class TransactionFactory
    {
        private readonly IServiceProvider serviceProvider;

        public TransactionFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IAccountTransactionType GetAccountTransactionType(string transactionType)
        {
            if (transactionType == TransactionTypeConstants.Disbursement.ToString())
                return (IAccountTransactionType)serviceProvider.GetService(typeof(DisbursementAccountTransactionType));
            else if (transactionType == TransactionTypeConstants.Order.ToString())
                return (IAccountTransactionType)serviceProvider.GetService(typeof(OrderAccountTransactionType));
            else
                return (IAccountTransactionType)serviceProvider.GetService(typeof(OrderAccountTransactionType));
        }
    }
}
