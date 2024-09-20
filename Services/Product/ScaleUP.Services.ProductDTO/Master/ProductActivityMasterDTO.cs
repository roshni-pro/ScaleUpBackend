

using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace ScaleUP.Services.ProductDTO.Master
{
    [DataContract]
    public class ProductActivityMasterDTO
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public long ActivityMasterId { get; set; }
        public string ActivityMasterName { get; set; }
        public long CompanyId { get; set; }
        public long? SubActivityMasterId { get; set; }
        public string? SubActivityMasterName { get; set; }
        public  int Sequence { get; set; }
        public bool IsActive { get; set; }
    }
}
