using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _18Jan2024 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BusinessDetails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    DOI = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BusGSTNO = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    BusEntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BusMaskPan = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IncomeSlab = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OwnershipType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ElectricityNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ElectricityOwnerName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ElectricityOwnerAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BuisnessMonthlySalary = table.Column<double>(type: "float", nullable: true),
                    AddressLineOne = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    AddressLineTwo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    AddressLineThree = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ZipCode = table.Column<int>(type: "int", nullable: false),
                    CityName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StateName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CountryName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LeadId = table.Column<long>(type: "bigint", maxLength: 200, nullable: false),
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
                    table.PrimaryKey("PK_BusinessDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessDetails_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonalDetails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FatherName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FatherLastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MiddleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MobileNo = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    PanMaskNO = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    AadhaarMaskNO = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    DOB = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    AlternatePhoneNo = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    EmailId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CurrentAddressLineOne = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CurrentAddressLineTwo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CurrentAddressLineThree = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CurrentZipCode = table.Column<int>(type: "int", nullable: false),
                    CurrentCityName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CurrentStateName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CurrentCountryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PermanentAddressLineOne = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    PermanentAddressLineTwo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PermanentAddressLineThree = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PermanentZipCode = table.Column<int>(type: "int", nullable: false),
                    PermanentCityName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PermanentStateName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PermanentCountryName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
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
                    table.PrimaryKey("PK_PersonalDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonalDetails_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessDetails_LeadId",
                table: "BusinessDetails",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalDetails_LeadId",
                table: "PersonalDetails",
                column: "LeadId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BusinessDetails");

            migrationBuilder.DropTable(
                name: "PersonalDetails");
        }
    }
}
