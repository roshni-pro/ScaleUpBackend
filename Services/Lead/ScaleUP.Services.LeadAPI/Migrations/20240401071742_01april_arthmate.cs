using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _01april_arthmate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArthMateCommonAPIRequestResponses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeadId = table.Column<long>(type: "bigint", nullable: true),
                    LeadNBFCApiId = table.Column<long>(type: "bigint", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    URL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Request = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Response = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatusCode = table.Column<int>(type: "int", nullable: false),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArthMateCommonAPIRequestResponses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AScore",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeadMasterId = table.Column<long>(type: "bigint", nullable: false),
                    product = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    request_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CINSRate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DPD24C2 = table.Column<int>(type: "int", nullable: false),
                    EQ3C1 = table.Column<int>(type: "int", nullable: false),
                    HL12 = table.Column<int>(type: "int", nullable: false),
                    HL24C3 = table.Column<int>(type: "int", nullable: false),
                    HL36CC = table.Column<int>(type: "int", nullable: false),
                    LLPL = table.Column<int>(type: "int", nullable: false),
                    NCVL_AScore = table.Column<int>(type: "int", nullable: false),
                    PSMA24 = table.Column<int>(type: "int", nullable: false),
                    RSKPremium = table.Column<double>(type: "float", nullable: false),
                    VYC3 = table.Column<double>(type: "float", nullable: false),
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
                    table.PrimaryKey("PK_AScore", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CeplrPdfReports",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeadMasterId = table.Column<long>(type: "bigint", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fip_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    callback_url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    file_password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    customer_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    request_id = table.Column<int>(type: "int", nullable: false),
                    ApiRequest_id = table.Column<int>(type: "int", nullable: false),
                    ApiToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("PK_CeplrPdfReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CoLenderResponse",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeadMasterId = table.Column<long>(type: "bigint", nullable: false),
                    request_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    loan_amount = table.Column<double>(type: "float", nullable: false),
                    pricing = table.Column<double>(type: "float", nullable: false),
                    co_lender_shortcode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    loan_app_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    co_lender_assignment_id = table.Column<int>(type: "int", nullable: false),
                    co_lender_full_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AScoreRequest_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ceplr_cust_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SanctionAmount = table.Column<double>(type: "float", nullable: false),
                    ProgramType = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("PK_CoLenderResponse", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KYCValidationResponse",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeadMasterId = table.Column<long>(type: "bigint", nullable: false),
                    DocumentMasterId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    kyc_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResponseJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("PK_KYCValidationResponse", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeadBankStatement",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeadMasterId = table.Column<long>(type: "bigint", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    DocumentMasterId = table.Column<int>(type: "int", nullable: false),
                    StatementFile = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("PK_LeadBankStatement", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArthMateCommonAPIRequestResponses");

            migrationBuilder.DropTable(
                name: "AScore");

            migrationBuilder.DropTable(
                name: "CeplrPdfReports");

            migrationBuilder.DropTable(
                name: "CoLenderResponse");

            migrationBuilder.DropTable(
                name: "KYCValidationResponse");

            migrationBuilder.DropTable(
                name: "LeadBankStatement");
        }
    }
}
