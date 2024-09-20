using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.Lead
{
    public class LeadUpdateHistorieDTO
    {
        public long Id { get; set; }
        public long LeadId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string EventName { get; set; }
        public string Narration { get; set; }
        public string NarrationHTML { get; set; }
        public DateTime CreateDate { get; set; }
    }

    public class LeadUpdateHistorieRequestDTO
    {
        public long LeadId { get; set; }
        public string EventName { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
    }
}
