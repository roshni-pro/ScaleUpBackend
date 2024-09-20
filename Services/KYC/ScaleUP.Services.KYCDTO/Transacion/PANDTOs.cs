using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.KYCDTO.Transacion
{
    public class KarzaPANDTO
    {
        public string DocumentId { get; set; }
        public int? age { get; set; }
        public DateTime? date_of_birth { get; set; }
        public DateTime? date_of_issue { get; set; }
        public string? fathers_name { get; set; }
        public bool? id_scanned { get; set; }
        public bool? minor { get; set; }
        public string? name_on_card { get; set; }
        public string? pan_type { get; set; }
        public string? id_number { get; set; }
    }

}
