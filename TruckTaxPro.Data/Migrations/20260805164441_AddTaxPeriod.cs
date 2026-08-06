using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TruckTaxPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxPeriod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TaxYear",
                table: "TaxPeriods",
                newName: "TaxYearStart");

            migrationBuilder.RenameColumn(
                name: "Label",
                table: "TaxPeriods",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "TaxPeriods",
                newName: "IsFinalReturn");

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "TaxPeriods",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ConsentToDisclosure",
                table: "TaxPeriods",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TaxPeriods",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CurrentStep",
                table: "TaxPeriods",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FirstUsedMonth",
                table: "TaxPeriods",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "TaxYearEnd",
                table: "TaxPeriods",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TaxPeriods",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_TaxPeriods_BusinessId",
                table: "TaxPeriods",
                column: "BusinessId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaxPeriods_Businesses_BusinessId",
                table: "TaxPeriods",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaxPeriods_Businesses_BusinessId",
                table: "TaxPeriods");

            migrationBuilder.DropIndex(
                name: "IX_TaxPeriods_BusinessId",
                table: "TaxPeriods");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "TaxPeriods");

            migrationBuilder.DropColumn(
                name: "ConsentToDisclosure",
                table: "TaxPeriods");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TaxPeriods");

            migrationBuilder.DropColumn(
                name: "CurrentStep",
                table: "TaxPeriods");

            migrationBuilder.DropColumn(
                name: "FirstUsedMonth",
                table: "TaxPeriods");

            migrationBuilder.DropColumn(
                name: "TaxYearEnd",
                table: "TaxPeriods");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TaxPeriods");

            migrationBuilder.RenameColumn(
                name: "TaxYearStart",
                table: "TaxPeriods",
                newName: "TaxYear");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "TaxPeriods",
                newName: "Label");

            migrationBuilder.RenameColumn(
                name: "IsFinalReturn",
                table: "TaxPeriods",
                newName: "IsActive");
        }
    }
}
