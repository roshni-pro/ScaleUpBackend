using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.Lead
{
    public class LeadPanDTO : LeadBaseDTO
    {
        [Required]
        public string UniqueId { get; set; }
        public string ImagePath { get; set; }
        [Required]
        public long DocumentId { get; set; }
        [Required]
        public string? FathersName { get; set; }
        [Required]
        public DateTime? DOB { get; set; }
        [Required]
        public string? Name { get; set; }
        public string? UserName { get; set; }

    }
}
