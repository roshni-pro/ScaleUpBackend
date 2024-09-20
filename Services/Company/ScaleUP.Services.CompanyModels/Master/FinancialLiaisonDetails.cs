using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IdentityServer4.Models.IdentityResources;

namespace ScaleUP.Services.CompanyModels.Master
{
    public class FinancialLiaisonDetails: BaseAuditableEntity
    {
        [StringLength(30)]
        public string FirstName { get; set; }
        [StringLength(30)]
        public string LastName { get; set; }
        [StringLength(12)]
        public string ContactNo { get; set; }
        [StringLength(30)]
        public string EmailAddress { get; set; }
        [ForeignKey("CompanyId")]
        public Companies Companies { get; set; }
        public long CompanyId { get; set; }
    }
}
