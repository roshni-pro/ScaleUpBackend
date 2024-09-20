using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Services.LoanAccountAPI.NBFCFactory.Implementation;

namespace ScaleUP.Services.LoanAccountAPI.NBFCFactory
{
    public class LoanNBFCFactory
    {

        private readonly IServiceProvider serviceProvider;

        public LoanNBFCFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }


        public ILoanNBFCService GetService(string nbfcServiceName)
        {
            if (nbfcServiceName == LeadNBFCConstants.BlackSoil.ToString())
                return (ILoanNBFCService)serviceProvider.GetService(typeof(BlackSoilLoanNBFCService));
            else if(nbfcServiceName == LeadNBFCConstants.ArthMate.ToString())
                return (ILoanNBFCService)serviceProvider.GetService(typeof(ArthmateLoanNBFCService));
            else if (nbfcServiceName == LeadNBFCConstants.Default.ToString())
                return (ILoanNBFCService)serviceProvider.GetService(typeof(DefaultLoanNBFCService));
            else if (nbfcServiceName == LeadNBFCConstants.AyeFinanceSCF.ToString())
                return (ILoanNBFCService)serviceProvider.GetService(typeof(AyeFinanceSCFLoanNBFCService));
            else
                return null;

        }
    }
}
