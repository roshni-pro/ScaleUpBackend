using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _6sept2024_AddTableField_HopDashboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "InterestRepayment",
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

            migrationBuilder.AddColumn<double>(
                name: "TotalRepayment",
                table: "HopDashboardData",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BounceRePayment",
                table: "HopDashboardData");

            migrationBuilder.DropColumn(
                name: "ExtraPayment",
                table: "HopDashboardData");

            migrationBuilder.DropColumn(
                name: "InterestRepayment",
                table: "HopDashboardData");

            migrationBuilder.DropColumn(
                name: "OverdueInterestPayment",
                table: "HopDashboardData");

            migrationBuilder.DropColumn(
                name: "PenalRePayment",
                table: "HopDashboardData");

            migrationBuilder.DropColumn(
                name: "PrincipalRepayment",
                table: "HopDashboardData");

            migrationBuilder.DropColumn(
                name: "TotalRepayment",
                table: "HopDashboardData");
        }
    }
}
