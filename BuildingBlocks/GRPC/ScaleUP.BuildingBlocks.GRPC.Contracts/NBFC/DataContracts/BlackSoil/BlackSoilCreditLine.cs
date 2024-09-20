using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts.BlackSoil
{
    public class BlackSoilCreditLine
    {
        public int count { get; set; }
        public int? next { get; set; }
        public int? previous { get; set; }
        public List<BlackSoilCreditLineDetail> results { get; set; }
    }

    public class BlackSoilAvailableCreditLimit
    {
        public string? available_limit { get; set; }
        
    }



    public class BlackSoilCreditLineDetail
    {
        public long id { get; set; }
        public string update_url { get; set; }
        public double? line_utilized_amount { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public string? doc_id { get; set; }
        public string? system_generated_amount { get; set; }
        public DateTime? system_generated_at { get; set; }
        public string? amount { get; set; }
        public string? usable_limit_amount { get; set; }
        public double? old_amount { get; set; }
        public long? blacksoil_sanction_id { get; set; }
        public DateTime? initiated_date { get; set; }
        public DateTime? activated_date { get; set; }
        public DateTime? inactivated_date { get; set; }
        public string? line_status { get; set; }
        public string? loan_closure_letter_file { get; set; }
        public long? business { get; set; }
    }

}
