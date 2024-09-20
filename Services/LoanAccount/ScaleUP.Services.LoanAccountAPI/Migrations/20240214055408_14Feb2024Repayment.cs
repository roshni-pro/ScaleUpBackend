using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _14Feb2024Repayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LoanAccountRepayments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ThirdPartyPaymentId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LoanAccountId = table.Column<long>(type: "bigint", nullable: false),
                    ThirdPartyLoanAccountId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaymentMode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BankRefNo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ThirdPartyTxnId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Amount = table.Column<double>(type: "float", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalAmount = table.Column<double>(type: "float", nullable: false),
                    InterestAmount = table.Column<double>(type: "float", nullable: false),
                    ProcessingFees = table.Column<double>(type: "float", nullable: false),
                    PenalInterest = table.Column<double>(type: "float", nullable: false),
                    OverdueInterest = table.Column<double>(type: "float", nullable: false),
                    PrincipalAmount = table.Column<double>(type: "float", nullable: false),
                    ExtraPaymentAmount = table.Column<double>(type: "float", nullable: false),
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
                    table.PrimaryKey("PK_LoanAccountRepayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoanAccountRepayments_LoanAccounts_LoanAccountId",
                        column: x => x.LoanAccountId,
                        principalTable: "LoanAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoanAccountRepayments_LoanAccountId",
                table: "LoanAccountRepayments",
                column: "LoanAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoanAccountRepayments");
        }
    }
}
