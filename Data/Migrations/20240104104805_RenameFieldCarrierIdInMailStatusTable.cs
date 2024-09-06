using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostalManagementMVC.Data.Migrations
{
    public partial class RenameFieldCarrierIdInMailStatusTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MailStatus_AspNetUsers_CarrierId",
                table: "MailStatus");

            migrationBuilder.RenameColumn(
                name: "CarrierId",
                table: "MailStatus",
                newName: "OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_MailStatus_CarrierId",
                table: "MailStatus",
                newName: "IX_MailStatus_OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_MailStatus_AspNetUsers_OwnerId",
                table: "MailStatus",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MailStatus_AspNetUsers_OwnerId",
                table: "MailStatus");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "MailStatus",
                newName: "CarrierId");

            migrationBuilder.RenameIndex(
                name: "IX_MailStatus_OwnerId",
                table: "MailStatus",
                newName: "IX_MailStatus_CarrierId");

            migrationBuilder.AddForeignKey(
                name: "FK_MailStatus_AspNetUsers_CarrierId",
                table: "MailStatus",
                column: "CarrierId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
