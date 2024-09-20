using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountModels.Transaction.NBFC
{
    public class BlackSoilCommonAPIRequestResponse : CommonAPIRequestResponse
    {
        public long? NBFCCompanyApiDetailId { get; set; }
    }
}
