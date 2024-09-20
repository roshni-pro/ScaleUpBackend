using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.ProductModels.Master
{
    public class NBFCCompanyApiType : BaseAuditableEntity
    {
        [StringLength(500)]
        public string ApiType { get; set; }  //Like Callurl ,creditlimit,Order
    }
}
