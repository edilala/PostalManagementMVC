using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostalManagementMVC.Data.Migrations
{
    public partial class RefactorClientSubscription2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TrackedMailId",
                table: "ClientSubscription",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ClientSubscription_TrackedMailId",
                table: "ClientSubscription",
                column: "TrackedMailId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientSubscription_Mail_TrackedMailId",
                table: "ClientSubscription",
                column: "TrackedMailId",
                principalTable: "Mail",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientSubscription_Mail_TrackedMailId",
                table: "ClientSubscription");

            migrationBuilder.DropIndex(
                name: "IX_ClientSubscription_TrackedMailId",
                table: "ClientSubscription");

            migrationBuilder.DropColumn(
                name: "TrackedMailId",
                table: "ClientSubscription");
        }
    }
}
