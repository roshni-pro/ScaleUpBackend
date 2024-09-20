
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ScaleUP.Services.ProductDTO.Master
{
    [DataContract]
    public class ProductDTO
    {
        public long Id { get; set; }
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(100)]
        public string Type { get; set; } 

        [StringLength(1000)]
        public string Description { get; set; }

        public ICollection<ProductCompanyDTO> ProductCompanies { get; set; }

        public ICollection<ProductActivityMasterDTO> ProductActivityMasters { get; set; }
    }
}
