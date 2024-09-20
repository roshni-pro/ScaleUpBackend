using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LocationDTO.Master
{
    [DataContract]
    public class LocationResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public dynamic ReturnObject { get; set; }
    }
}
