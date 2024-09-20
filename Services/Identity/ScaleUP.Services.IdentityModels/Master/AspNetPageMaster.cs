using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ScaleUP.Services.IdentityModels.Master
{
    public class AspNetPageMaster : BaseAuditableEntity
    {
        [StringLength(50)]
        public required string PageName { get; set; }
        [StringLength(100)]
        public required string RouteName { get; set; }
        [StringLength(30)]
        public string? ClassName { get; set; }
        public required int Sequence { get; set; }
        public long? ParentId { get; set; }
        public required bool IsMaster { get; set; }
        public ICollection<AspNetRolePagePermission> aspNetRolePagePermissions { get; set; }
    }
}
