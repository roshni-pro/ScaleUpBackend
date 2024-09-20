using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.ApiGateways.Aggregator.Migrations
{
    /// <inheritdoc />
    public partial class AppHomeItemName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ItemName",
                table: "AppHomeItemDb",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ItemName",
                table: "AppHomeContentDb",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ItemName",
                table: "AppHomeItemDb");

            migrationBuilder.DropColumn(
                name: "ItemName",
                table: "AppHomeContentDb");
        }
    }
}
