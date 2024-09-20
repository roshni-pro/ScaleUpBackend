using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _04May2024_IsKycVerified : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LeadId",
                table: "ArthMateWebhookResponse");

            migrationBuilder.AddColumn<bool>(
                name: "IsKycVerified",
                table: "KYCValidationResponse",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsKycVerified",
                table: "KYCValidationResponse");

            migrationBuilder.AddColumn<long>(
                name: "LeadId",
                table: "ArthMateWebhookResponse",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
