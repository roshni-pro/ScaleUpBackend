using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts.BlackSoil
{
    public class BlackSoilLoanAccountProcessed
    {
        public BlackSoilLoanAccountProcessedData data {  get; set; }
    }
    public class BlackSoilLoanAccountProcessedData
    {
        public long? id { get; set; }
        public string? update_url { get; set; }
        public long? alert_level_amount { get; set; }
        public string? created_at { get; set; }
        public string? updated_at { get; set; }
        public string? loan_account_id { get; set; }
        public string? status { get; set; }
        public string? doc_id { get; set; }
        public long? pool_amount { get; set; }
        public string? nach_debit_amount { get; set; }
        public string? loan_recall_notice_file { get; set; }
        public string? cheque_legal_notice_file { get; set; }
        public string? nach_legal_notice_file { get; set; }
        public bool do_not_refresh { get; set; }
        public long? dehaat_farmer_due_date { get; set; }
        public long? business { get; set; }
        public string? account_manager { get; set; }
        public BlackSoilLoanAccountProcessedIdentifiers identifiers { get; set; }
    }

    public class BlackSoilLoanAccountProcessedIdentifiers
    {
        public string? business_id { get; set; }
        public string? partner_lead_id { get; set; }
        public string? mobile { get; set; }
    }
}
