using Microsoft.EntityFrameworkCore.Migrations;
using PostalManagementMVC.Entities;

#nullable disable

namespace PostalManagementMVC.Data.Migrations
{
    public partial class AddUniquePairLocationConnection : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "IX_OnlyOneConnectionForDayForLocations",
                table: "LocationConnection",
                columns: new string[] { "FromLocationId", "ToLocationId", "OnDay" }
                );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "IX_OnlyOneConnectionForDayForLocations",
                table: "LocationConnection"
                );
        }
    }
}
