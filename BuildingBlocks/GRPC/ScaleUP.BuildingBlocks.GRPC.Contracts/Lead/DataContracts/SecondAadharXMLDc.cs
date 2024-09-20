using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class SecondAadharXMLDc
    {
        [DataMember(Order =1)]
        public long LeadMasterId { get; set; }
        [DataMember(Order = 2)]
        public string request_id { get; set; }
        [DataMember(Order = 3)]
        public int otp { get; set; }
        [DataMember(Order = 4)]
        public double loan_amt { get; set; }
        [DataMember(Order = 5)]
        public bool insurance_applied { get; set; }
        [DataMember(Order = 6)]
        public ProductCompanyConfigDc ProductCompanyConfig { get; set; }
    }

    [DataContract]
    public class AadharOTPVerifyDc
    {
        [DataMember(Order = 1)]
        public long LeadMasterId { get; set; }
        [DataMember(Order = 2)]
        public string request_id { get; set; }
        [DataMember(Order = 3)]
        public int otp { get; set; }
        [DataMember(Order = 4)]
        public double loan_amt { get; set; }
        [DataMember(Order = 5)]
        public bool insurance_applied { get; set; }
   
    }
}
