using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostalManagementMVC.Data.Migrations
{
    public partial class AddMailCategoryModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Mail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "MailCategory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailCategory", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Mail_CategoryId",
                table: "Mail",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Mail_MailCategory_CategoryId",
                table: "Mail",
                column: "CategoryId",
                principalTable: "MailCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mail_MailCategory_CategoryId",
                table: "Mail");

            migrationBuilder.DropTable(
                name: "MailCategory");

            migrationBuilder.DropIndex(
                name: "IX_Mail_CategoryId",
                table: "Mail");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Mail");
        }
    }
}
