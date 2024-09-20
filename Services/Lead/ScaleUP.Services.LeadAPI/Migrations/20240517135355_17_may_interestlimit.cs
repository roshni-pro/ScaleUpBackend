using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _17_may_interestlimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "MaxInterestRate",
                table: "LoanConfiguration",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "InterestRate",
                table: "Leads",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxInterestRate",
                table: "LoanConfiguration");

            migrationBuilder.DropColumn(
                name: "InterestRate",
                table: "Leads");
        }
    }
}
