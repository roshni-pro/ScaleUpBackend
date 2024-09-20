using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.IdentityDTO.Master
{
    public class RoleDTO
    {
        public string RoleName { get; set; }
    }
    public class RoleListDTO
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
    }
    public class RoleWithPermissionsDTO
    {
        public string RoleName { get; set; }
        public bool IsView { get; set; }
        public bool IsAdd { get; set; }
        public bool IsUpdate { get; set; }
    }
}
