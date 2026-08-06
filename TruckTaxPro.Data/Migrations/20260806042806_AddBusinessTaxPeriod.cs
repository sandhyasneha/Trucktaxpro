using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TruckTaxPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessTaxPeriod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BusinessTaxPeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessId = table.Column<int>(type: "int", nullable: false),
                    TaxYearStart = table.Column<int>(type: "int", nullable: false),
                    TaxYearEnd = table.Column<int>(type: "int", nullable: false),
                    FirstUsedMonth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsFinalReturn = table.Column<bool>(type: "bit", nullable: false),
                    ConsentToDisclosure = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentStep = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessTaxPeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessTaxPeriods_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessTaxPeriods_BusinessId",
                table: "BusinessTaxPeriods",
                column: "BusinessId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BusinessTaxPeriods");
        }
    }
}
