using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _27Jan2024 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdentificationCode",
                table: "LeadOffers");

            migrationBuilder.AddColumn<string>(
                name: "TleadId",
                table: "LeadOffers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BlackSoilUpdates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessId = table.Column<long>(type: "bigint", nullable: false),
                    BusinessUpdateUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    BusinessAddressId = table.Column<long>(type: "bigint", nullable: false),
                    BusinessAddressUpdateUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    PanId = table.Column<long>(type: "bigint", nullable: false),
                    PanUpdateUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    AadhaarId = table.Column<long>(type: "bigint", nullable: false),
                    AadhaarUpdateUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    PersonId = table.Column<long>(type: "bigint", nullable: false),
                    PersonUpdateUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    PersonAddressId = table.Column<long>(type: "bigint", nullable: false),
                    PersonAddressUpdateUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    LeadId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_BlackSoilUpdates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlackSoilUpdates_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlackSoilUpdates_LeadId",
                table: "BlackSoilUpdates",
                column: "LeadId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlackSoilUpdates");

            migrationBuilder.DropColumn(
                name: "TleadId",
                table: "LeadOffers");

            migrationBuilder.AddColumn<string>(
                name: "IdentificationCode",
                table: "LeadOffers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
