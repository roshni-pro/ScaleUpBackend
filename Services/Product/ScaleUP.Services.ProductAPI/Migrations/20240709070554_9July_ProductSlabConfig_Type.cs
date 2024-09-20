using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.ProductAPI.Migrations
{
    /// <inheritdoc />
    public partial class _9July_ProductSlabConfig_Type : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Percentage",
                table: "ProductSlabConfigurations",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "MinValue",
                table: "ProductSlabConfigurations",
                newName: "Min");

            migrationBuilder.RenameColumn(
                name: "MaxValue",
                table: "ProductSlabConfigurations",
                newName: "Max");

            migrationBuilder.AddColumn<bool>(
                name: "IsFixed",
                table: "ProductSlabConfigurations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ValueType",
                table: "ProductSlabConfigurations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFixed",
                table: "ProductSlabConfigurations");

            migrationBuilder.DropColumn(
                name: "ValueType",
                table: "ProductSlabConfigurations");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "ProductSlabConfigurations",
                newName: "Percentage");

            migrationBuilder.RenameColumn(
                name: "Min",
                table: "ProductSlabConfigurations",
                newName: "MinValue");

            migrationBuilder.RenameColumn(
                name: "Max",
                table: "ProductSlabConfigurations",
                newName: "MaxValue");
        }
    }
}
