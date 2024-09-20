using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _9july2024_HopDashboardData_dropcolumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisbursementAmount",
                table: "HopDashboardData");

            migrationBuilder.DropColumn(
                name: "DisbursementDate",
                table: "HopDashboardData");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DisbursementAmount",
                table: "HopDashboardData",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DisbursementDate",
                table: "HopDashboardData",
                type: "datetime2",
                nullable: true);
        }
    }
}
