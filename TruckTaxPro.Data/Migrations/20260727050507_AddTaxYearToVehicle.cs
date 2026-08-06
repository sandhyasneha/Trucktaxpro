using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TruckTaxPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxYearToVehicle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TaxYear",
                table: "Vehicles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TaxYear",
                table: "Vehicles");
        }
    }
}
