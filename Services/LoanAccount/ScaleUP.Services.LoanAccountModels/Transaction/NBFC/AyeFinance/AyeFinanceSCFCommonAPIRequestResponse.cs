using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountModels.Transaction.NBFC.AyeFinance
{
    public class AyeFinanceSCFCommonAPIRequestResponse : CommonAPIRequestResponse
    {
        public long? LoanAccountId { get; set; }
        public long? NBFCCompanyApiDetailId { get; set; }
    }
}
