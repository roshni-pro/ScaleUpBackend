using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Media.DataContracts
{
    [DataContract]
    public class DocRequest
    {
        [DataMember(Order = 1)]
        public long DocId { get; set; }

    }

    [DataContract]
    public class MultiDocRequest
    {
        [DataMember(Order = 1)]
        public List<long> DocIdList { get; set; }

    }

    [DataContract]
    public class GRPCHtmlConvertRequest
    {
        [DataMember(Order = 1)]
        public string HtmlContent { get; set; }
    }

    [DataContract]
    public class GRPCPdfResponse
    {
        [DataMember(Order = 1)]
        public string PdfUrl { get; set; }
    }
}
