﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _09_Apr_Arthmate5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "InquiryAmount",
                table: "BusinessDetails",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tenure",
                table: "ArthMateUpdates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ArthmateSlaLbaStampDetail",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StampPaperNo = table.Column<int>(type: "int", nullable: true),
                    UsedFor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PartnerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StampAmount = table.Column<double>(type: "float", nullable: false),
                    LoanId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StampUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsStampUsed = table.Column<bool>(type: "bit", nullable: true),
                    DateofUtilisation = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LeadmasterId = table.Column<long>(type: "bigint", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArthmateSlaLbaStampDetail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeadLoan",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeadMasterId = table.Column<long>(type: "bigint", nullable: true),
                    RequestId = table.Column<long>(type: "bigint", nullable: true),
                    ReponseId = table.Column<long>(type: "bigint", nullable: true),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    co_lender_assignment_id = table.Column<int>(type: "int", nullable: true),
                    loan_app_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    loan_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    borrower_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    partner_loan_app_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    partner_loan_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    partner_borrower_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    company_id = table.Column<int>(type: "int", nullable: true),
                    co_lender_id = table.Column<int>(type: "int", nullable: true),
                    co_lend_flag = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    product_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    itr_ack_no = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    loan_app_date = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    penal_interest = table.Column<int>(type: "int", nullable: true),
                    bounce_charges = table.Column<int>(type: "int", nullable: true),
                    sanction_amount = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    gst_on_pf_amt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    gst_on_pf_perc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    repayment_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    first_inst_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    net_disbur_amt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    final_approve_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    final_remarks = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    foir = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    stage = table.Column<int>(type: "int", nullable: true),
                    upfront_interest = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    exclude_interest_till_grace_period = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    customer_type_ntc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    borro_bank_account_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    borro_bank_account_holder_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    business_vintage_overall = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    gst_number = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    abb = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    loan_int_amt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    loan_int_rate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    conv_fees = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    processing_fees_amt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    processing_fees_perc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    tenure = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    tenure_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    int_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    borro_bank_ifsc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    borro_bank_acc_num = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    borro_bank_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    first_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    last_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ninety_plus_dpd_in_last_24_months = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    current_overdue_value = table.Column<int>(type: "int", nullable: true),
                    dpd_in_last_9_months = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    dpd_in_last_3_months = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    dpd_in_last_6_months = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    bureau_score = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    monthly_income = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    bounces_in_three_month = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    loan_amount_requested = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    insurance_company = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    credit_card_settlement_amount = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    emi_amount = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    emi_allowed = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    bene_bank_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    bene_bank_acc_num = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    bene_bank_ifsc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    bene_bank_account_holder_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    igst_amount = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cgst_amount = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sgst_amount = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    emi_count = table.Column<int>(type: "int", nullable: true),
                    broken_interest = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    dpd_in_last_12_months = table.Column<int>(type: "int", nullable: true),
                    dpd_in_last_3_months_credit_card = table.Column<int>(type: "int", nullable: true),
                    dpd_in_last_3_months_unsecured = table.Column<int>(type: "int", nullable: true),
                    broken_period_int_amt = table.Column<double>(type: "float", nullable: true),
                    dpd_in_last_24_months = table.Column<int>(type: "int", nullable: true),
                    avg_banking_turnover_6_months = table.Column<int>(type: "int", nullable: true),
                    enquiries_bureau_30_days = table.Column<int>(type: "int", nullable: true),
                    cnt_active_unsecured_loans = table.Column<int>(type: "int", nullable: true),
                    total_overdues_in_cc = table.Column<int>(type: "int", nullable: true),
                    insurance_amount = table.Column<int>(type: "int", nullable: true),
                    bureau_outstanding_loan_amt = table.Column<int>(type: "int", nullable: true),
                    subvention_fees_amount = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    gst_on_subvention_fees = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cgst_on_subvention_fees = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sgst_on_subvention_fees = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    igst_on_subvention_fees = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    purpose_of_loan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    business_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    co_app_or_guar_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    co_app_or_guar_address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    co_app_or_guar_mobile_no = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    co_app_or_guar_pan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    co_app_or_guar_bureau_score = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    business_address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    business_state = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    business_city = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    business_pin_code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    business_address_ownership = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    business_pan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    bureau_fetch_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    enquiries_in_last_3_months = table.Column<int>(type: "int", nullable: true),
                    gst_on_conv_fees = table.Column<double>(type: "float", nullable: true),
                    cgst_on_conv_fees = table.Column<double>(type: "float", nullable: true),
                    sgst_on_conv_fees = table.Column<double>(type: "float", nullable: true),
                    igst_on_conv_fees = table.Column<double>(type: "float", nullable: true),
                    gst_on_application_fees = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cgst_on_application_fees = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sgst_on_application_fees = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    igst_on_application_fees = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    interest_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    conv_fees_excluding_gst = table.Column<double>(type: "float", nullable: true),
                    application_fees_excluding_gst = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    emi_obligation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    a_score_request_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    a_score = table.Column<int>(type: "int", nullable: true),
                    b_score = table.Column<int>(type: "int", nullable: true),
                    offered_amount = table.Column<int>(type: "int", nullable: true),
                    offered_int_rate = table.Column<double>(type: "float", nullable: false),
                    monthly_average_balance = table.Column<double>(type: "float", nullable: false),
                    monthly_imputed_income = table.Column<double>(type: "float", nullable: false),
                    party_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    co_app_or_guar_dob = table.Column<DateTime>(type: "datetime2", nullable: true),
                    co_app_or_guar_gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    co_app_or_guar_ntc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    udyam_reg_no = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    program_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    written_off_settled = table.Column<int>(type: "int", nullable: true),
                    upi_handle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    upi_reference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fc_offer_days = table.Column<int>(type: "int", nullable: true),
                    foreclosure_charge = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    eligible_loan_amount = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    v = table.Column<int>(type: "int", nullable: true),
                    UrlSlaDocument = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UrlSlaUploadSignedDocument = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsUpload = table.Column<bool>(type: "bit", nullable: true),
                    UrlSlaUploadDocument_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UMRN = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadLoan", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArthmateSlaLbaStampDetail");

            migrationBuilder.DropTable(
                name: "LeadLoan");

            migrationBuilder.DropColumn(
                name: "InquiryAmount",
                table: "BusinessDetails");

            migrationBuilder.DropColumn(
                name: "Tenure",
                table: "ArthMateUpdates");
        }
    }
}