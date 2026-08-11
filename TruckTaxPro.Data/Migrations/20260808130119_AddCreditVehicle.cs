using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TruckTaxPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditVehicle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CreditVehicles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessTaxPeriodId = table.Column<int>(type: "int", nullable: false),
                    UnitNumber = table.Column<int>(type: "int", nullable: false),
                    Vin = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WeightCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsLogging = table.Column<bool>(type: "bit", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BuyerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstUsedMonthPriorYear = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PreviouslyReportedTax = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TaxAmountUsed = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CreditAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditVehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditVehicles_BusinessTaxPeriods_BusinessTaxPeriodId",
                        column: x => x.BusinessTaxPeriodId,
                        principalTable: "BusinessTaxPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CreditVehicles_BusinessTaxPeriodId",
                table: "CreditVehicles",
                column: "BusinessTaxPeriodId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CreditVehicles");
        }
    }
}
