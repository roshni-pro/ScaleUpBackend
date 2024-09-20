using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.IdentityModels.Master
{
    public class AspNetRolePagePermission : BaseAuditableEntity
    {
        public required string RoleId { get; set; }
        public required bool IsView { get; set; }
        public required bool IsAdd { get; set; }
        public required bool IsEdit { get; set; }
        public required bool IsDelete { get; set; }
        public required bool IsAll { get; set; }
        [ForeignKey("PageMasterId")]
        public AspNetPageMaster AspNetPageMaster { get; set; }
        public required long PageMasterId { get; set; }
    }
}
