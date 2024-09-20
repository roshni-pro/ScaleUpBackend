using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.ProductDTO.Master
{
    public class ProductActivitySubActivityListDTO
    {
        public string Activity { get; set; }
        public int Sequence { get; set; }
        public long ActivityMasterId { get; set; }
        public bool IsActive { get; set; }

        public List<SubActivityMastersDTO> SubActivity { get; set; }
    }
    public class SubActivityMastersDTO
    {
        public long ProductCompanyActivityMasterId { get; set; }
        public string Name { get; set; }
        public int Sequence { get; set; }
        public long SubActivityMasterId { get; set; }
        public bool IsActive { get; set; }
        public int ApiCount { get; set; }
    }
}
