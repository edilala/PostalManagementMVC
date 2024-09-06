using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostalManagementMVC.Data.Migrations
{
    public partial class RenameFieldSubscriptionMailColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TrackedItemCode",
                table: "ClientSubscription",
                newName: "TrackedMailId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TrackedMailId",
                table: "ClientSubscription",
                newName: "TrackedItemCode");
        }
    }
}
