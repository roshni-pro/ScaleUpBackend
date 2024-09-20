using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _28LeadConvertorAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LeadConverter",
                table: "Leads",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
               name: "LeadGenerator",
               table: "Leads",
               type: "nvarchar(500)",
               maxLength: 500,
               nullable: true);
            migrationBuilder.AddColumn<bool>(
                name: "IsOfferRejected",
                table: "ArthMateUpdates",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LeadConverter",
                table: "Leads");

            migrationBuilder.DropColumn(
               name: "LeadGenerator",
               table: "Leads");
            migrationBuilder.DropColumn(
               name: "IsOfferRejected",
               table: "ArthMateUpdates");
        }
    }
}
