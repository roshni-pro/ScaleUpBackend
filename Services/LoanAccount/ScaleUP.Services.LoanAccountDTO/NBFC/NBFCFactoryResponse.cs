using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountDTO.NBFC
{
    public class NBFCFactoryResponse
    {

        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public double? Limit { get; set; }
    }
}
