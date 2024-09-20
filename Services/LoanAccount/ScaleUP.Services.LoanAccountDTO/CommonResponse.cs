using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountDTO
{
    public class CommonResponse
    {
        public bool status { get; set; }
        public string  Message { get; set; }
        public object Result { get; set; }
    }
}
