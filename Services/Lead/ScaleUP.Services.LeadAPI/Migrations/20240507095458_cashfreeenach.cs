using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class cashfreeenach : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AlterColumn<DateTime>(
            //    name: "linkExpiryDate",
            //    table: "CashfreeEnachs",
            //    type: "datetime2",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AlterColumn<int>(
            //    name: "linkExpiryDate",
            //    table: "CashfreeEnachs",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(DateTime),
            //    oldType: "datetime2");
        }
    }
}
