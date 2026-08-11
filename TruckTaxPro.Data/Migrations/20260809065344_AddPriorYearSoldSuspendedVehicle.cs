using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TruckTaxPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPriorYearSoldSuspendedVehicle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PriorYearSoldSuspendedVehicles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessTaxPeriodId = table.Column<int>(type: "int", nullable: false),
                    UnitNumber = table.Column<int>(type: "int", nullable: false),
                    Vin = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MileageLimit = table.Column<int>(type: "int", nullable: false),
                    DateSold = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BuyerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriorYearSoldSuspendedVehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriorYearSoldSuspendedVehicles_BusinessTaxPeriods_BusinessTaxPeriodId",
                        column: x => x.BusinessTaxPeriodId,
                        principalTable: "BusinessTaxPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PriorYearSoldSuspendedVehicles_BusinessTaxPeriodId",
                table: "PriorYearSoldSuspendedVehicles",
                column: "BusinessTaxPeriodId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PriorYearSoldSuspendedVehicles");
        }
    }
}
