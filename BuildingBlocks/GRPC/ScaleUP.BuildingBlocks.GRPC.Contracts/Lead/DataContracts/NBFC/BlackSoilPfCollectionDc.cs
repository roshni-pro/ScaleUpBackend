using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.NBFC
{
    [DataContract]
    public class BlackSoilPfCollectionDc
    {
        [DataMember(Order = 1)]
        public double? processing_fee { get; set; }
        [DataMember(Order = 2)]
        public double? processing_fee_tax { get; set; }
        [DataMember(Order = 3)]
        public double? total_processing_fee { get; set; }
        [DataMember(Order = 4)]
        public string? processing_fee_status { get; set; }
        [DataMember(Order = 5)]
        public double? processing_fee_absolute { get; set; }
    }
}
