using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.CompanyDTO.Master
{
    [DataContract]
    public class CompanyResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public dynamic ReturnObject { get; set; }
    }
    public class CompanyResponse<T>
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public T ReturnObject { get; set; }
    }
}
