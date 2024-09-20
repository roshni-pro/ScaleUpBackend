using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _13Feb2024BlackSoil : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CommonAPIRequestResponses",
                table: "CommonAPIRequestResponses");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "CommonAPIRequestResponses");

            migrationBuilder.RenameTable(
                name: "CommonAPIRequestResponses",
                newName: "BlackSoilCommonAPIRequestResponses");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BlackSoilCommonAPIRequestResponses",
                table: "BlackSoilCommonAPIRequestResponses",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_BlackSoilCommonAPIRequestResponses",
                table: "BlackSoilCommonAPIRequestResponses");

            migrationBuilder.RenameTable(
                name: "BlackSoilCommonAPIRequestResponses",
                newName: "CommonAPIRequestResponses");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "CommonAPIRequestResponses",
                type: "nvarchar(34)",
                maxLength: 34,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CommonAPIRequestResponses",
                table: "CommonAPIRequestResponses",
                column: "Id");
        }
    }
}
