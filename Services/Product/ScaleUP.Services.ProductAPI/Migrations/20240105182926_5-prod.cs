using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.ProductAPI.Migrations
{
    /// <inheritdoc />
    public partial class _5prod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityMasters",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    FrontOrBack = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyType = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("PK_ActivityMasters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CreditDayMasters",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
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
                    table.PrimaryKey("PK_CreditDayMasters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EMIOptionMasters",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
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
                    table.PrimaryKey("PK_EMIOptionMasters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Product",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_Product", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubActivityMasters",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    KycMasterCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ActivityMasterId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_SubActivityMasters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubActivityMasters_ActivityMasters_ActivityMasterId",
                        column: x => x.ActivityMasterId,
                        principalTable: "ActivityMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductAnchorCompany",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    ProcessingFeePayableBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProcessingFeeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProcessingFeeRate = table.Column<double>(type: "float", nullable: false),
                    TransactionFeePayableBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TransactionFeeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TransactionFeeRate = table.Column<double>(type: "float", nullable: true),
                    DelayPenaltyRate = table.Column<double>(type: "float", nullable: false),
                    BounceCharge = table.Column<long>(type: "bigint", nullable: false),
                    CreditDays = table.Column<long>(type: "bigint", nullable: true),
                    DisbursementTAT = table.Column<long>(type: "bigint", nullable: true),
                    AnnualInterestRate = table.Column<double>(type: "float", nullable: true),
                    MinTenureInMonth = table.Column<long>(type: "bigint", nullable: true),
                    MaxTenureInMonth = table.Column<long>(type: "bigint", nullable: true),
                    EMIRate = table.Column<double>(type: "float", nullable: true),
                    EMIProcessingFeeRate = table.Column<double>(type: "float", nullable: true),
                    EMIBounceCharge = table.Column<long>(type: "bigint", nullable: true),
                    EMIPenaltyRate = table.Column<double>(type: "float", nullable: true),
                    CommissionPayout = table.Column<double>(type: "float", nullable: true),
                    ConsiderationFee = table.Column<double>(type: "float", nullable: true),
                    DisbursementSharingCommission = table.Column<double>(type: "float", nullable: true),
                    MinLoanAmount = table.Column<long>(type: "bigint", nullable: true),
                    MaxLoanAmount = table.Column<long>(type: "bigint", nullable: true),
                    AgreementStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AgreementEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AgreementURL = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AgreementDocId = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("PK_ProductAnchorCompany", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductAnchorCompany_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductCompanies",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false),
                    GstRate = table.Column<double>(type: "float", nullable: false),
                    ConvenienceFee = table.Column<double>(type: "float", nullable: false),
                    ProcessingFee = table.Column<double>(type: "float", nullable: false),
                    DelayPenaltyFee = table.Column<double>(type: "float", nullable: false),
                    BounceCharges = table.Column<double>(type: "float", nullable: false),
                    ProcessingCreditDays = table.Column<double>(type: "float", nullable: false),
                    CreditDays = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_ProductCompanies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductCompanies_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductNBFCCompany",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    ProcessingFee = table.Column<long>(type: "bigint", nullable: false),
                    InterestRate = table.Column<double>(type: "float", nullable: false),
                    PenaltyCharges = table.Column<long>(type: "bigint", nullable: false),
                    BounceCharges = table.Column<long>(type: "bigint", nullable: false),
                    PlatformFee = table.Column<long>(type: "bigint", nullable: false),
                    AgreementStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AgreementEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CustomerAgreementURL = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CustomerAgreementDocId = table.Column<long>(type: "bigint", nullable: true),
                    AgreementURL = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AgreementDocId = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("PK_ProductNBFCCompany", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductNBFCCompany_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductActivityMasters",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    ActivityMasterId = table.Column<long>(type: "bigint", nullable: false),
                    SubActivityMasterId = table.Column<long>(type: "bigint", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_ProductActivityMasters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductActivityMasters_ActivityMasters_ActivityMasterId",
                        column: x => x.ActivityMasterId,
                        principalTable: "ActivityMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductActivityMasters_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductActivityMasters_SubActivityMasters_SubActivityMasterId",
                        column: x => x.SubActivityMasterId,
                        principalTable: "SubActivityMasters",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductCompanyActivityMasters",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    ActivityMasterId = table.Column<long>(type: "bigint", nullable: false),
                    SubActivityMasterId = table.Column<long>(type: "bigint", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_ProductCompanyActivityMasters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductCompanyActivityMasters_ActivityMasters_ActivityMasterId",
                        column: x => x.ActivityMasterId,
                        principalTable: "ActivityMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductCompanyActivityMasters_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductCompanyActivityMasters_SubActivityMasters_SubActivityMasterId",
                        column: x => x.SubActivityMasterId,
                        principalTable: "SubActivityMasters",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CompanyCreditDays",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductAnchorCompanyId = table.Column<long>(type: "bigint", nullable: false),
                    CreditDaysMasterId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_CompanyCreditDays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyCreditDays_CreditDayMasters_CreditDaysMasterId",
                        column: x => x.CreditDaysMasterId,
                        principalTable: "CreditDayMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompanyCreditDays_ProductAnchorCompany_ProductAnchorCompanyId",
                        column: x => x.ProductAnchorCompanyId,
                        principalTable: "ProductAnchorCompany",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanyEMIOptions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductAnchorCompanyId = table.Column<long>(type: "bigint", nullable: false),
                    EMIOptionMasterId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_CompanyEMIOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyEMIOptions_EMIOptionMasters_EMIOptionMasterId",
                        column: x => x.EMIOptionMasterId,
                        principalTable: "EMIOptionMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompanyEMIOptions_ProductAnchorCompany_ProductAnchorCompanyId",
                        column: x => x.ProductAnchorCompanyId,
                        principalTable: "ProductAnchorCompany",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyCreditDays_CreditDaysMasterId",
                table: "CompanyCreditDays",
                column: "CreditDaysMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyCreditDays_ProductAnchorCompanyId",
                table: "CompanyCreditDays",
                column: "ProductAnchorCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyEMIOptions_EMIOptionMasterId",
                table: "CompanyEMIOptions",
                column: "EMIOptionMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyEMIOptions_ProductAnchorCompanyId",
                table: "CompanyEMIOptions",
                column: "ProductAnchorCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductActivityMasters_ActivityMasterId",
                table: "ProductActivityMasters",
                column: "ActivityMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductActivityMasters_ProductId",
                table: "ProductActivityMasters",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductActivityMasters_SubActivityMasterId",
                table: "ProductActivityMasters",
                column: "SubActivityMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAnchorCompany_ProductId",
                table: "ProductAnchorCompany",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCompanies_ProductId",
                table: "ProductCompanies",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCompanyActivityMasters_ActivityMasterId",
                table: "ProductCompanyActivityMasters",
                column: "ActivityMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCompanyActivityMasters_ProductId",
                table: "ProductCompanyActivityMasters",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCompanyActivityMasters_SubActivityMasterId",
                table: "ProductCompanyActivityMasters",
                column: "SubActivityMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductNBFCCompany_ProductId",
                table: "ProductNBFCCompany",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SubActivityMasters_ActivityMasterId",
                table: "SubActivityMasters",
                column: "ActivityMasterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyCreditDays");

            migrationBuilder.DropTable(
                name: "CompanyEMIOptions");

            migrationBuilder.DropTable(
                name: "ProductActivityMasters");

            migrationBuilder.DropTable(
                name: "ProductCompanies");

            migrationBuilder.DropTable(
                name: "ProductCompanyActivityMasters");

            migrationBuilder.DropTable(
                name: "ProductNBFCCompany");

            migrationBuilder.DropTable(
                name: "CreditDayMasters");

            migrationBuilder.DropTable(
                name: "EMIOptionMasters");

            migrationBuilder.DropTable(
                name: "ProductAnchorCompany");

            migrationBuilder.DropTable(
                name: "SubActivityMasters");

            migrationBuilder.DropTable(
                name: "Product");

            migrationBuilder.DropTable(
                name: "ActivityMasters");
        }
    }
}
