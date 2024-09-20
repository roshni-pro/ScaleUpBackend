using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _30July_nbfcOfferUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nbfcOfferUpdate",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeadId = table.Column<long>(type: "bigint", nullable: false),
                    NBFCCompanyId = table.Column<long>(type: "bigint", nullable: false),
                    AnchorCompanyId = table.Column<long>(type: "bigint", nullable: false),
                    CompanyIdentificationCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoanId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoanAmount = table.Column<double>(type: "float", nullable: true),
                    Tenure = table.Column<int>(type: "int", nullable: true),
                    InterestRate = table.Column<double>(type: "float", nullable: true),
                    LoanInterestAmount = table.Column<double>(type: "float", nullable: true),
                    MonthlyEMI = table.Column<double>(type: "float", nullable: true),
                    ProcessingFeeRate = table.Column<double>(type: "float", nullable: true),
                    ProcessingFeeAmount = table.Column<double>(type: "float", nullable: true),
                    ProcessingFeeTax = table.Column<double>(type: "float", nullable: true),
                    PFType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GST = table.Column<double>(type: "float", nullable: true),
                    NBFCRemark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OfferStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("PK_nbfcOfferUpdate", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nbfcOfferUpdate");
        }
    }
}
