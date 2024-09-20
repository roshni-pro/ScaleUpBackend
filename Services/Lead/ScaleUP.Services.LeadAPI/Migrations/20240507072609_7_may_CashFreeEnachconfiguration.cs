using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _7_may_CashFreeEnachconfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "linkExpiry",
                table: "CashfreeEnachs",
                newName: "linkExpiryDate");

            migrationBuilder.AddColumn<string>(
                name: "returnUrl",
                table: "cashFreeEnachconfigurations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "BlackSoilPFCollections",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeadId = table.Column<long>(type: "bigint", nullable: false),
                    processing_fee = table.Column<double>(type: "float", nullable: false),
                    processing_fee_tax = table.Column<double>(type: "float", nullable: false),
                    total_processing_fee = table.Column<double>(type: "float", nullable: false),
                    status = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlackSoilPFCollections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlackSoilPFCollections_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlackSoilPFCollections_LeadId",
                table: "BlackSoilPFCollections",
                column: "LeadId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlackSoilPFCollections");

            migrationBuilder.DropColumn(
                name: "returnUrl",
                table: "cashFreeEnachconfigurations");

            migrationBuilder.RenameColumn(
                name: "linkExpiryDate",
                table: "CashfreeEnachs",
                newName: "linkExpiry");
        }
    }
}
