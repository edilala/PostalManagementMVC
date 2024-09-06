using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostalManagementMVC.Data.Migrations
{
    public partial class AddFromToDifferenLocationCheckLocConns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_FromToLocationsNotEqual",
                table: "LocationConnection",
                sql: "ToLocationId <> FromLocationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_FromToLocationsNotEqual",
                table: "LocationConnection");

        }
    }
}
