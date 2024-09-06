using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostalManagementMVC.Data.Migrations
{
    public partial class RefactorLocationConnectionTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocationConnection_ConnectionCatalog_ConnectionTypeId",
                table: "LocationConnection");

            migrationBuilder.DropTable(
                name: "ConnectionCatalog");

            migrationBuilder.DropIndex(
                name: "IX_LocationConnection_ConnectionTypeId",
                table: "LocationConnection");

            migrationBuilder.DropColumn(
                name: "ConnectionTypeId",
                table: "LocationConnection");

            migrationBuilder.AddColumn<double>(
                name: "Cost",
                table: "LocationConnection",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "OnDay",
                table: "LocationConnection",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cost",
                table: "LocationConnection");

            migrationBuilder.DropColumn(
                name: "OnDay",
                table: "LocationConnection");

            migrationBuilder.AddColumn<int>(
                name: "ConnectionTypeId",
                table: "LocationConnection",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ConnectionCatalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConnectionDays = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectionCatalog", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocationConnection_ConnectionTypeId",
                table: "LocationConnection",
                column: "ConnectionTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_LocationConnection_ConnectionCatalog_ConnectionTypeId",
                table: "LocationConnection",
                column: "ConnectionTypeId",
                principalTable: "ConnectionCatalog",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
