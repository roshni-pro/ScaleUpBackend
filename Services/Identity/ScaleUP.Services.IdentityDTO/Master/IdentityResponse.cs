using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.IdentityDTO.Master
{
    public class IdentityResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public dynamic ReturnObject { get; set; }
    }
}
