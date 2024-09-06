using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostalManagementMVC.Data.Migrations
{
    public partial class AddCreationModificationUserToMail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "Mail",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ModifiedById",
                table: "Mail",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mail_CreatedById",
                table: "Mail",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Mail_ModifiedById",
                table: "Mail",
                column: "ModifiedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Mail_AspNetUsers_CreatedById",
                table: "Mail",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Mail_AspNetUsers_ModifiedById",
                table: "Mail",
                column: "ModifiedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mail_AspNetUsers_CreatedById",
                table: "Mail");

            migrationBuilder.DropForeignKey(
                name: "FK_Mail_AspNetUsers_ModifiedById",
                table: "Mail");

            migrationBuilder.DropIndex(
                name: "IX_Mail_CreatedById",
                table: "Mail");

            migrationBuilder.DropIndex(
                name: "IX_Mail_ModifiedById",
                table: "Mail");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Mail");

            migrationBuilder.DropColumn(
                name: "ModifiedById",
                table: "Mail");
        }
    }
}
