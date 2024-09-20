using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LocationDTO.State
{
    public class AddStateDTO
    {
        public long? StateId { get; set; }
        public string Name { get; set; }
        public string StateCode { get; set; }
        public long CountryId { get; set; }
    }
}
