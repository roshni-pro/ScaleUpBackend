using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ScaleUP.Services.ProductDTO.Master
{
    [DataContract]
    public class ActivityMasterDTO
    {
        public long ActivityMasterId { get; set; }
        [StringLength(300)]
        public string ActivityName { get; set; }
        public int? KycId { get; set; }
    }
}
