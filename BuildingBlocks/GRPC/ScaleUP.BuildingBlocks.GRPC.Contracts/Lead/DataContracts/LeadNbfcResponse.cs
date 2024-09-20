using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class LeadNbfcResponse
    {
        [DataMember(Order = 1)]
        public long NbfcId { get; set; }
        [DataMember(Order = 2)]
        public string CompanyIdentificationCode { get; set; }
        [DataMember(Order = 3)]
        public double? GST { get; set; }
        [DataMember(Order = 4)]
        public string NbfcCompanyName { get; set; }
    }
}
