using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostalManagementMVC.Data.Migrations
{
    public partial class RefactorConnectionCatalogColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConnectionOffsetHours",
                table: "ConnectionCatalog");

            migrationBuilder.AddColumn<string>(
                name: "ConnectionDays",
                table: "ConnectionCatalog",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConnectionDays",
                table: "ConnectionCatalog");

            migrationBuilder.AddColumn<int>(
                name: "ConnectionOffsetHours",
                table: "ConnectionCatalog",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
