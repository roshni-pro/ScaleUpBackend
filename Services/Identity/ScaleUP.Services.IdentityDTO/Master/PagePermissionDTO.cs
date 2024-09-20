using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.IdentityDTO.Master
{
    public class RolePagePermissionDTO
    {
        public string? RoleId { get; set; }
        public long PageMasterId { get; set; }
        public string PageName { get; set; }
        public bool IsView { get; set; }
        public bool IsAdd { get; set; }
        public bool IsEdit { get; set; }
        public bool IsDelete { get; set; }
        public bool IsAll { get; set; }
    }
    public class PageListDTO
    {
        public long PageMasterId { get; set; }
        public string PageName { get; set; }
        public string RouteName { get; set; }
        public string? ClassName { get; set; }
        public int Sequence { get; set; }
        public bool IsView { get; set; }
        public bool IsAdd { get; set; }
        public bool IsEdit { get; set; }
        public bool IsDelete { get; set; }
        public bool IsAll { get; set; }
        public long? ParentId { get; set; }
        public bool IsMaster { get; set; }
        public List<SubPageListDTO> subPageMaster { get; set; }
    }

    public class SubPageListDTO
    {
        public long PageMasterId { get; set; }
        public string PageName { get; set; }
        public string RouteName { get; set; }
        public string? ClassName { get; set; }
        public int Sequence { get; set; }
        public bool IsView { get; set; }
        public bool IsAdd { get; set; }
        public bool IsEdit { get; set; }
        public bool IsDelete { get; set; }
        public bool IsAll { get; set; }
        public long? ParentId { get; set; }
        public bool IsMaster { get; set; }
    }
}
