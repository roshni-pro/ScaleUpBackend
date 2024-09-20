using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.KYCDTO.Transacion
{
    public class KarzaElectricityServiceProviderDTO
    {
        public string State { get; set; }
        public string ServiceProvider { get; set; }
        public string Code { get; set; }
    }
    public class KarzaElectricityStateDTO
    {
        public string DistrictName { get; set; }
        public string State { get; set; }
        public int DistrictCode { get; set; }
    }
}
