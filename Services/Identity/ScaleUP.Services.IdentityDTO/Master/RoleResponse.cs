using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.IdentityDTO.Master
{
    public class RoleResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public string RoleId { get; set; }
    }
}
