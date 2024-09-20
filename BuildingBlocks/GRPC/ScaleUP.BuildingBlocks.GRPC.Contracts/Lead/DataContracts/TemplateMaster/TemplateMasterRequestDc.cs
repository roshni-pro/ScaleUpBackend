using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.TemplateMaster
{
    [DataContract]
    public class TemplateMasterRequestDc
    {
        [DataMember(Order = 1)]
        public string TemplateCode { get; set; }
    }
}
