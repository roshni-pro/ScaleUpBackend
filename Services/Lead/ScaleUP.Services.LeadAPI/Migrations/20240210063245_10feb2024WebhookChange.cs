using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _10feb2024WebhookChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlackSoilWebhookResponses_Leads_LeadId",
                table: "BlackSoilWebhookResponses");

            migrationBuilder.DropColumn(
                name: "eventRequestId",
                table: "BlackSoilWebhookResponses");

            migrationBuilder.DropColumn(
                name: "eventResponseId",
                table: "BlackSoilWebhookResponses");

            migrationBuilder.AlterColumn<long>(
                name: "LeadId",
                table: "BlackSoilWebhookResponses",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "FK_BlackSoilWebhookResponses_Leads_LeadId",
                table: "BlackSoilWebhookResponses",
                column: "LeadId",
                principalTable: "Leads",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlackSoilWebhookResponses_Leads_LeadId",
                table: "BlackSoilWebhookResponses");

            migrationBuilder.AlterColumn<long>(
                name: "LeadId",
                table: "BlackSoilWebhookResponses",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "eventRequestId",
                table: "BlackSoilWebhookResponses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "eventResponseId",
                table: "BlackSoilWebhookResponses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BlackSoilWebhookResponses_Leads_LeadId",
                table: "BlackSoilWebhookResponses",
                column: "LeadId",
                principalTable: "Leads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
