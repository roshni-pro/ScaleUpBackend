using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    public class ExperianOTPRegistrationInput
    {
        public string CustomerId { get; set; }
        public long? LeadId { get; set; }
        public long? ActivityId { get; set; }
        public long? SubActivityId { get; set; }
        public long? CompanyId { get; set; }
        public string Usersid { get; set; }
        public bool IsTestingPurpose { get; set; }
        public string ProductCode { get; set; }
    }
}
