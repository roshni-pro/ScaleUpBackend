using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.KYCDTO.Transacion
{
    public class KarzaElectricityInputDTO
    {
        public string consumer_id { get; set; }
        public string service_provider { get; set; }
        public string district { get; set; }
        public string consent { get; set; }
        public clientData clientData { get; set; }

    }
    public class clientData
    {
        public string caseId { get; set; }
    }
}
