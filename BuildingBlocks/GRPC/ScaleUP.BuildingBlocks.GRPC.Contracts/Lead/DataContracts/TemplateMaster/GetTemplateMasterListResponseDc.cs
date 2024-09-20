using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.TemplateMaster
{
    [DataContract]
    public class GetTemplateMasterListResponseDc
    {
        [DataMember(Order =1)]
        public string TemplateCode { get; set; }
        [DataMember(Order =2)]
        public string TemplateType { get; set; }
        [DataMember(Order =3)]
        public string? DLTID { get; set; }
        [DataMember(Order =4)]
        public string Template { get; set; }
        [DataMember(Order = 5)]
        public long TemplateId { get; set; }
        [DataMember(Order = 6)]
        public bool IsActive { get; set; }
        [DataMember(Order = 7)]
        public DateTime CreatedDate { get; set; }
    }
}
