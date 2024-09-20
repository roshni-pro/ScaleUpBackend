using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts
{
    [DataContract]
    public class SaveLoanAccountCompanyLeadRequestDC
    {
        [DataMember(Order = 1)]
        public required long LoanAccountId { get; set; }
        [DataMember(Order = 2)]
        public long CompanyId { get; set; }
        [DataMember(Order = 3)]
        public int? LeadProcessStatus { get; set; } //0-Initiated, 1-InProcessed, 2-Completed
        [DataMember(Order = 4)]
        public string? UserUniqueCode { get; set; }
        [DataMember(Order = 5)]
        public string? AnchorName { get; set; }
        [DataMember(Order = 6)] 
        public string? LogoURL { get; set; }
        [DataMember(Order = 7)]
        public string? CustUniqueCode { get;set; }
    }

    [DataContract]
    public class SaveLoanAccountCompLeadReqDC
    {
        [DataMember(Order = 1)]
        public long LeadId { get; set; }
        [DataMember(Order = 2)]
        public long CompanyId { get; set; }
        [DataMember(Order = 3)]
         public List<SaveLoanAccountCompanyLeadRequestDC>? SaveLoanAccountCompanyLeadListDC { get; set; }
        
    }
}
