using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _11_Apr_Arthmate7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArthmateDisbursements",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    loan_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    partner_loan_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status_code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    net_disbur_amt = table.Column<double>(type: "float", nullable: false),
                    utr_number = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    utr_date_time = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArthmateDisbursements", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArthmateDisbursements");
        }
    }
}
