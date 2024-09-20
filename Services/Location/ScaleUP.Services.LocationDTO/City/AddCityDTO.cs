using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LocationDTO.City
{
    public class AddCityDTO
    {
        public long? Id { get; set; }
        public string CityName { get; set; }
        public long StateId { get; set; }
    }

    public class CityMasterListDTO
    {
        public long Id { get; set; }
        public string CityName { get; set; }
        public string StateName { get; set; }
        public bool status { get; set; }
    }

}
