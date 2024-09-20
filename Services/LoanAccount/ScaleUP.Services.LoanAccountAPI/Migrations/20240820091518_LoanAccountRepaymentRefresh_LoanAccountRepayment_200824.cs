using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class LoanAccountRepaymentRefresh_LoanAccountRepayment_200824 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRunning",
                table: "LoanAccountRepayments",
                type: "bit",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LoanAccountRepaymentRefreshs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoanAccountId = table.Column<long>(type: "bigint", nullable: false),
                    IsRunning = table.Column<bool>(type: "bit", nullable: false),
                    LastRunningDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsError = table.Column<bool>(type: "bit", nullable: true),
                    ErrorMsg = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_LoanAccountRepaymentRefreshs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoanAccountRepaymentRefreshs");

            migrationBuilder.DropColumn(
                name: "IsRunning",
                table: "LoanAccountRepayments");
        }
    }
}
