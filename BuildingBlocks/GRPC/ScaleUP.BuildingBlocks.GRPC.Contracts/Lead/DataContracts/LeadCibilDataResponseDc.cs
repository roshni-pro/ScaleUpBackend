using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class LeadCibilDataResponseDc
    {
        [DataMember(Order = 1)]
        public double CibilGreaterPercentage { get; set; }
        [DataMember(Order = 2)]
        public double CibilLessPercentage { get; set; }
        [DataMember(Order = 3)]
        public double CibilZeroPercentage { get; set; }
    }
}
