using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _6_may_CashfreeEnach : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enachs_Leads_LeadId",
                table: "Enachs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Enachs",
                table: "Enachs");

            migrationBuilder.RenameTable(
                name: "Enachs",
                newName: "CashfreeEnachs");

            migrationBuilder.RenameIndex(
                name: "IX_Enachs_LeadId",
                table: "CashfreeEnachs",
                newName: "IX_CashfreeEnachs_LeadId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CashfreeEnachs",
                table: "CashfreeEnachs",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CashfreeEnachs_Leads_LeadId",
                table: "CashfreeEnachs",
                column: "LeadId",
                principalTable: "Leads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CashfreeEnachs_Leads_LeadId",
                table: "CashfreeEnachs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CashfreeEnachs",
                table: "CashfreeEnachs");

            migrationBuilder.RenameTable(
                name: "CashfreeEnachs",
                newName: "Enachs");

            migrationBuilder.RenameIndex(
                name: "IX_CashfreeEnachs_LeadId",
                table: "Enachs",
                newName: "IX_Enachs_LeadId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Enachs",
                table: "Enachs",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Enachs_Leads_LeadId",
                table: "Enachs",
                column: "LeadId",
                principalTable: "Leads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
