using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Global.Infrastructure.Constants.AccountLocation;
using ScaleUP.Services.LeadAPI.NBFCFactory.Implementation;

namespace ScaleUP.Services.LeadAPI.NBFCFactory
{
    public class LeadNBFCFactory
    {
        private readonly IServiceProvider serviceProvider;

        public LeadNBFCFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }


        public ILeadNBFCService GetService(string nbfcServiceName)
        {
            if (nbfcServiceName == LeadNBFCConstants.BlackSoil.ToString())
                return (ILeadNBFCService)serviceProvider.GetService(typeof(BlackSoilLeadNBFCService));
            else if (nbfcServiceName == LeadNBFCConstants.Default.ToString())
                return (ILeadNBFCService)serviceProvider.GetService(typeof(DefaultLeadNBFCService));
            else if (nbfcServiceName == LeadNBFCConstants.ArthMate.ToString())
                return (ILeadNBFCService)serviceProvider.GetService(typeof(ArthMateNBFCService));
            else if (nbfcServiceName == LeadNBFCConstants.AyeFinanceSCF.ToString())
                return (ILeadNBFCService)serviceProvider.GetService(typeof(AyeFinanceSCFLeadNBFCService));
            else
                return null;
        }

    }
}
