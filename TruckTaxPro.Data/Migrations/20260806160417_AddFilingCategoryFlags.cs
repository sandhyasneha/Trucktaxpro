using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TruckTaxPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFilingCategoryFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IncludeCredit",
                table: "BusinessTaxPeriods",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IncludePriorYearSoldSuspended",
                table: "BusinessTaxPeriods",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IncludeSuspended",
                table: "BusinessTaxPeriods",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IncludeTaxable",
                table: "BusinessTaxPeriods",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IncludeCredit",
                table: "BusinessTaxPeriods");

            migrationBuilder.DropColumn(
                name: "IncludePriorYearSoldSuspended",
                table: "BusinessTaxPeriods");

            migrationBuilder.DropColumn(
                name: "IncludeSuspended",
                table: "BusinessTaxPeriods");

            migrationBuilder.DropColumn(
                name: "IncludeTaxable",
                table: "BusinessTaxPeriods");
        }
    }
}
