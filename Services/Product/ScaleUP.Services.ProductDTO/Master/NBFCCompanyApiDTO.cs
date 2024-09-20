using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.ProductDTO.Master
{
    public class NBFCCompanyApiDTO
    {
    }

    public class NBFCCompanyApiRequest
    {
        public long NBFCCompanyId { get; set; }
        [StringLength(300)]
        public string APIUrl { get; set; }
        [StringLength(300)]
        public string Code { get; set; }
    }
    public class NBFCCompanyGetData
    {
        public long NBFCCompanyApiId { get; set; }
        public long NBFCCompanyId { get; set; }
        [StringLength(300)]
        public string APIUrl { get; set; }
        [StringLength(300)]
        public string Code { get; set; }
    }
    public class NBFCCompanyResponseMsg 
    {
        public string Msg { get; set; }
        public bool Status { get; set; }
    }
}
