using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostalManagementMVC.Data.Migrations
{
    public partial class AddBundleMailField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MailBundleId",
                table: "Mail",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mail_MailBundleId",
                table: "Mail",
                column: "MailBundleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Mail_Mail_MailBundleId",
                table: "Mail",
                column: "MailBundleId",
                principalTable: "Mail",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mail_Mail_MailBundleId",
                table: "Mail");

            migrationBuilder.DropIndex(
                name: "IX_Mail_MailBundleId",
                table: "Mail");

            migrationBuilder.DropColumn(
                name: "MailBundleId",
                table: "Mail");
        }
    }
}
