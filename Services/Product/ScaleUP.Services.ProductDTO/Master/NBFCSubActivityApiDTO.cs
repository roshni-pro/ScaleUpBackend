using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.ProductDTO.Master
{
    public class NBFCSubActivityApiDTO
    {
        public long NBFCSubActivityApiId { get; set; }
        public long NBFCCompanyApiId { get; set; }//CompanyId
        public string? APIUrl { get; set; }
        public string? Code { get; set; }
        public  long ActivityMasterId { get; set; }
        public long? SubActivityMasterId { get; set; }
        public required int Sequence { get; set; }
        public long? ProductCompanyActivityMasterId { get; set; }
    }

    public class NBFCSubActivityApiRequest
    {
        public long ActivityMasterId { get; set; }
        public long? SubActivityMasterId { get; set; }
        public long? ProductCompanyActivityMasterId { get; set; }
    }
}
