using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _10_may_ProductConfigAndArthmate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArthmateDisbursement",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    loan_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    partner_loan_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status_code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    net_disbur_amt = table.Column<double>(type: "float", nullable: false),
                    utr_number = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    utr_date_time = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArthmateDisbursement", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductAnchorCompany",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    ProcessingFeePayableBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProcessingFeeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProcessingFeeRate = table.Column<double>(type: "float", nullable: false),
                    AnnualInterestPayableBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AnnualInterestRate = table.Column<double>(type: "float", nullable: true),
                    DelayPenaltyRate = table.Column<double>(type: "float", nullable: false),
                    BounceCharge = table.Column<long>(type: "bigint", nullable: false),
                    DisbursementTAT = table.Column<long>(type: "bigint", nullable: true),
                    MinTenureInMonth = table.Column<long>(type: "bigint", nullable: true),
                    MaxTenureInMonth = table.Column<long>(type: "bigint", nullable: true),
                    EMIRate = table.Column<double>(type: "float", nullable: true),
                    EMIProcessingFeeRate = table.Column<double>(type: "float", nullable: true),
                    EMIBounceCharge = table.Column<long>(type: "bigint", nullable: true),
                    EMIPenaltyRate = table.Column<double>(type: "float", nullable: true),
                    CommissionPayout = table.Column<double>(type: "float", nullable: true),
                    ConsiderationFee = table.Column<double>(type: "float", nullable: true),
                    DisbursementSharingCommission = table.Column<double>(type: "float", nullable: true),
                    MinLoanAmount = table.Column<long>(type: "bigint", nullable: true),
                    MaxLoanAmount = table.Column<long>(type: "bigint", nullable: true),
                    AgreementStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AgreementEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AgreementURL = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AgreementDocId = table.Column<long>(type: "bigint", nullable: true),
                    OfferMaxRate = table.Column<double>(type: "float", nullable: true),
                    CustomCreditDays = table.Column<long>(type: "bigint", nullable: true),
                    BlackSoilReferralCode = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_ProductAnchorCompany", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductNBFCCompany",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    ProcessingFeeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProcessingFee = table.Column<long>(type: "bigint", nullable: false),
                    AnnualInterestRate = table.Column<double>(type: "float", nullable: false),
                    PenaltyCharges = table.Column<long>(type: "bigint", nullable: false),
                    BounceCharges = table.Column<long>(type: "bigint", nullable: false),
                    PlatformFee = table.Column<long>(type: "bigint", nullable: false),
                    SanctionLetterDocId = table.Column<long>(type: "bigint", nullable: true),
                    SanctionLetterURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AgreementStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AgreementEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CustomerAgreementType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CustomerAgreementURL = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CustomerAgreementDocId = table.Column<long>(type: "bigint", nullable: true),
                    AgreementURL = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AgreementDocId = table.Column<long>(type: "bigint", nullable: true),
                    TAPIKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TAPISecretKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TReferralCode = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsInterestRateCoSharing = table.Column<bool>(type: "bit", nullable: true),
                    IsPenaltyChargeCoSharing = table.Column<bool>(type: "bit", nullable: true),
                    IsBounceChargeCoSharing = table.Column<bool>(type: "bit", nullable: true),
                    IsPlatformFeeCoSharing = table.Column<bool>(type: "bit", nullable: true),
                    DisbursementType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_ProductNBFCCompany", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArthmateDisbursement");

            migrationBuilder.DropTable(
                name: "ProductAnchorCompany");

            migrationBuilder.DropTable(
                name: "ProductNBFCCompany");
        }
    }
}
