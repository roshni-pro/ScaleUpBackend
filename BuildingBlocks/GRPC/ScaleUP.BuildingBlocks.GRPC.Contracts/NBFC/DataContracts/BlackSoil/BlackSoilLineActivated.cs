using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts.BlackSoil
{
    public class BlackSoilLineActivated
    {
        public BlackSoilLineActivatedData data {  get; set; }
    }

    public class BlackSoilLineActivatedData
    {
        public long? id { get; set; }
        public string? update_url { get; set; }
        public string? processing_fee_tax { get; set; }
        public string? total_processing_fee { get; set; }
        public string? nach_fee_tax { get; set; }
        public string? total_nach_fee { get; set; }
        public string? processing_fee_absolute { get; set; }
        public string? security_deposit_absolute { get; set; }
        public string? total_security_deposit { get; set; }
        public string? created_at { get; set; }
        public string? updated_at { get; set; }
        public string? application_id { get; set; }
        public string? close_application_reasons { get; set; }
        public string? product_type { get; set; }
        public string? status { get; set; }
        public string? esign_status { get; set; }
        public bool is_emandate_completed { get; set; }
        public bool is_all_document_verified { get; set; }
        public string? credit_line_amount { get; set; }
        public string? credit_line_initiated_at { get; set; }
        public string? credit_line_activated_at { get; set; }
        public string? processing_fee { get; set; }
        public string? security_deposit { get; set; }
        public string? nach_fee { get; set; }
        public string? security_deposit_type { get; set; }
        public string? processing_fee_type { get; set; }
        public string? processing_fee_receipt_file { get; set; }
        public string? processing_fee_receipt_no { get; set; }
        public string? processing_fee_settlement_utr { get; set; }
        public string? processing_fee_payment_mode { get; set; }
        public string? processing_fee_payment_date { get; set; }
        public string? processing_fee_status { get; set; }
        public string? nach_fee_receipt_file { get; set; }
        public string? nach_fee_receipt_no { get; set; }
        public string? nach_fee_settlement_utr { get; set; }
        public string? nach_fee_payment_mode { get; set; }
        public string? nach_fee_payment_date { get; set; }
        public string? nach_fee_status { get; set; }
        public string? security_deposit_receipt_file { get; set; }
        public string? security_deposit_receipt_no { get; set; }
        public string? security_deposit_settlement_utr { get; set; }
        public string? security_deposit_payment_mode { get; set; }
        public string? security_deposit_payment_date { get; set; }
        public string? security_deposit_status { get; set; }
        public long? monthly_interest_rate { get; set; }
        public long? credit_period_in_days { get; set; }
        public long? subvention_days { get; set; }
        public bool is_first_lender_approved { get; set; }
        public bool is_second_lender_approved { get; set; }
        public string? doc_id { get; set; }
        public string? comment { get; set; }
        public BlackSoilLineActivatedBusiness business { get; set; }
        public long? loan_data { get; set; }
        public long? distributor_business { get; set; }
        public BlackSoilLineActivatedIdentifiers identifiers { get; set; }
    }
    public class BlackSoilLineActivatedBusiness
    {
        public long? id { get; set; }
        public string? update_url { get; set; }
        public string? number_of_active_years { get; set; }
        public bool sanity_checks_status { get; set; }
        public string? created_at { get; set; }
        public string? updated_at { get; set; }
        public string? business_id { get; set; }
        public string? name { get; set; }
        public string? business_type { get; set; }
        public string? product_type { get; set; }
        public string? website { get; set; }
        public string? date_of_incorporation { get; set; }
        public string? doc_id { get; set; }
        public string? status { get; set; }
        public bool is_deleted { get; set; }
        public bool is_duplicate { get; set; }
        public string? referral_link { get; set; }
        public string? is_new_to_distributor { get; set; }
        public string? close_line_reasons { get; set; }
        public string? loan_status { get; set; }
        public string? reopen_comment { get; set; }
        public string? business_detail { get; set; }
        public bool is_msme_registered { get; set; }
        public bool is_cibil_check_enabled { get; set; }
        public string? los_failure_reason { get; set; }
        public long? business_vertical { get; set; }
        public string? dehaat_center { get; set; }
        public string? business_instance { get; set; }
    }

    public class BlackSoilLineActivatedIdentifiers
    {
        public string? business_id { get; set; }
        public string? partner_lead_id { get; set; }
        public string? mobile { get; set; }
        public string? application_id { get; set; }
    }
}
