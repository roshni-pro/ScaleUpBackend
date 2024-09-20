using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class HopDashboardTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HopDashboardData",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    AnchorCompanyId = table.Column<long>(type: "bigint", nullable: false),
                    AnchorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LTDUtilizedAmount = table.Column<double>(type: "float", nullable: false),
                    PrincipleOutstanding = table.Column<double>(type: "float", nullable: false),
                    InterestOutstanding = table.Column<double>(type: "float", nullable: false),
                    PenalOutStanding = table.Column<double>(type: "float", nullable: false),
                    OverdueInterestOutStanding = table.Column<double>(type: "float", nullable: false),
                    ODPrincipleOutstanding = table.Column<double>(type: "float", nullable: false),
                    ODInterestOutstanding = table.Column<double>(type: "float", nullable: false),
                    ODPenalOutStanding = table.Column<double>(type: "float", nullable: false),
                    ODOverdueInterestOutStanding = table.Column<double>(type: "float", nullable: false),
                    ScaleupShareAmount = table.Column<double>(type: "float", nullable: false),
                    DisbursementAmount = table.Column<double>(type: "float", nullable: false),
                    DisbursementDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_HopDashboardData", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HopDashboardData");
        }
    }
}
