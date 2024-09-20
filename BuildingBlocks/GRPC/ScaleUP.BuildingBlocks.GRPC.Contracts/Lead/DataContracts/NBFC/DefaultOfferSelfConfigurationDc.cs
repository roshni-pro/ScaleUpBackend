using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.NBFC
{
    [DataContract]
    public class DefaultOfferSelfConfigurationDc
    {
        [DataMember(Order = 1)]
        public long? Id { get; set; }
        [DataMember(Order = 2)]
        public long CompanyId { get; set; }
        [DataMember(Order = 3)]
        public int MinCibilScore { get; set; }
        [DataMember(Order = 4)]
        public int MaxCibilScore { get; set; }
        [DataMember(Order = 5)]
        public double MultiPlier { get; set; }
        [DataMember(Order = 6)]
        public double MaxCreditLimit { get; set; }
        [DataMember(Order = 7)]
        public double MinCreditLimit { get; set; }
        [DataMember(Order = 8)]
        public int MinVintageDays { get; set; }
        [DataMember(Order = 9)]
        public int MaxVintageDays { get; set; }
        [DataMember(Order = 10)]
        public string CustomerType { get; set; }
        [DataMember(Order = 11)]
        public bool IsActive { get; set; }
    }
}
