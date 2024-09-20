using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LocationDTO.Country
{
    public class AddCountryDTO
    {
        public long? CountryId { get; set; }
        public string Name { get; set; }
        public string CountryCode { get; set; }
        public string CurrencyCode { get; set; }
     
    }
}
