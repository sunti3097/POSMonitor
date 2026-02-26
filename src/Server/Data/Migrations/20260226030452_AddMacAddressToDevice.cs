using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POSMonitor.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMacAddressToDevice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MacAddress",
                table: "Devices",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MacAddress",
                table: "Devices");
        }
    }
}
