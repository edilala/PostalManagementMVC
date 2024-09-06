using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostalManagementMVC.Data.Migrations
{
    public partial class AddLocationConnectionDataCheck : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_CostValid",
                table: "LocationConnection",
                sql: "Cost >= 0 AND Cost < 10000");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ValidWeekDay",
                table: "LocationConnection",
                sql: "OnDay IN ('MONDAY','TUESDAY','WEDNESDAY','THURSDAY','FRIDAY','SATURDAY','SUNDAY')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_CostValid",
                table: "LocationConnection");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ValidWeekDay",
                table: "LocationConnection");
        }
    }
}
