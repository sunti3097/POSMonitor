using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POSMonitor.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyStoreFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyCode",
                table: "Devices",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StoreCode",
                table: "Devices",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyCode",
                table: "DeviceGroups",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Devices_CompanyCode",
                table: "Devices",
                column: "CompanyCode");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_StoreCode",
                table: "Devices",
                column: "StoreCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Devices_CompanyCode",
                table: "Devices");

            migrationBuilder.DropIndex(
                name: "IX_Devices_StoreCode",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "CompanyCode",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "StoreCode",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "CompanyCode",
                table: "DeviceGroups");
        }
    }
}
