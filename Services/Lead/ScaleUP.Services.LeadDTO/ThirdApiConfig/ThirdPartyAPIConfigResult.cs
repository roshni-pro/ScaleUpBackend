using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.ThirdApiConfig
{
    public class ThirdPartyAPIConfigResult<T>
    {
        public long Id{ get; set; }
        public string Code { get; set; }
        public T Config { get; set; }
    }
}
