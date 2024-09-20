using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.DSA
{
    [DataContract]
    public class eSignDocumentStatusDc
    {
        [DataMember(Order =1)]
        public long LeadId { get; set; }
        [DataMember(Order =2)]
        public string DocumentId { get; set; }
        [DataMember(Order =3)]
        public string RequestJson { get; set; }
    }
}
