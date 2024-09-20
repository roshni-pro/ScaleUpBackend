using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class modifyFields_HopDashboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnchorName",
                table: "HopDashboardData");

            migrationBuilder.DropColumn(
                name: "BounceRePayment",
                table: "HopDashboardData");

            migrationBuilder.DropColumn(
                name: "ExtraPayment",
                table: "HopDashboardData");

            migrationBuilder.DropColumn(
                name: "InterestOutstanding",
                table: "HopDashboardData");

            migrationBuilder.DropColumn(
                name: "InterestRepayment",
                table: "HopDashboardData");

            migrationBuilder.DropColumn(
                name: "ODInterestOutstanding",
                table: "HopDashboardData");

            migrationBuilder.DropColumn(
                name: "ODOverdueInterestOutStanding",
                table: "HopDashboardData");

            migrationBuilder.DropColumn(
                name: "ODPenalOutStanding",
                table: "HopDashboardData");

            migrationBuilder.DropColumn(
                name: "ODPrincipleOutstanding",
                table: "HopDashboardData");

            migrationBuilder.DropColumn(
                name: "OverdueInterestOutStanding",
                table: "HopDashboardData");

            migrationBuilder.DropColumn(
                name: "OverdueInterestPayment",
                table: "HopDashboardData");

            migrationBuilder.DropColumn(
                name: "PenalOutStanding",
                table: "HopDashboardData");

            migrationBuilder.DropColumn(
                name: "PenalRePayment",
                table: "HopDashboardData");

            migrationBuilder.DropColumn(
                name: "PrincipalRepayment",
                table: "HopDashboardData");

            migrationBuilder.RenameColumn(
                name: "TotalRepayment",
                table: "HopDashboardData",
                newName: "OverdueAmount");

            migrationBuilder.RenameColumn(
                name: "ScaleupShareAmount",
                table: "HopDashboardData",
                newName: "Outstanding");

            migrationBuilder.RenameColumn(
                name: "PrincipleOutstanding",
                table: "HopDashboardData",
                newName: "CreditLimitAmount");

            migrationBuilder.RenameColumn(
                name: "AnchorCompanyId",
                table: "HopDashboardData",
                newName: "NBFCCompanyId");

            migrationBuilder.CreateTable(
                name: "HopDashboardRevenue",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NBFCCompanyId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    ProductType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ScaleupShare = table.Column<double>(type: "float", nullable: false),
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
                    table.PrimaryKey("PK_HopDashboardRevenue", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HopDashboardRevenue");

            migrationBuilder.RenameColumn(
                name: "OverdueAmount",
                table: "HopDashboardData",
                newName: "TotalRepayment");

            migrationBuilder.RenameColumn(
                name: "Outstanding",
                table: "HopDashboardData",
                newName: "ScaleupShareAmount");

            migrationBuilder.RenameColumn(
                name: "NBFCCompanyId",
                table: "HopDashboardData",
                newName: "AnchorCompanyId");

            migrationBuilder.RenameColumn(
                name: "CreditLimitAmount",
                table: "HopDashboardData",
                newName: "PrincipleOutstanding");

            migrationBuilder.AddColumn<string>(
                name: "AnchorName",
                table: "HopDashboardData",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "BounceRePayment",
                table: "HopDashboardData",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ExtraPayment",
                table: "HopDashboardData",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "InterestOutstanding",
                table: "HopDashboardData",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "InterestRepayment",
                table: "HopDashboardData",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ODInterestOutstanding",
                table: "HopDashboardData",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ODOverdueInterestOutStanding",
                table: "HopDashboardData",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ODPenalOutStanding",
                table: "HopDashboardData",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ODPrincipleOutstanding",
                table: "HopDashboardData",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "OverdueInterestOutStanding",
                table: "HopDashboardData",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "OverdueInterestPayment",
                table: "HopDashboardData",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PenalOutStanding",
                table: "HopDashboardData",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PenalRePayment",
                table: "HopDashboardData",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PrincipalRepayment",
                table: "HopDashboardData",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
