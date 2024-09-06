using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostalManagementMVC.Data.Migrations
{
    public partial class RemoveUnusedTableField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostOffice_AspNetUsers_ManagerId",
                table: "PostOffice");

            migrationBuilder.DropTable(
                name: "AssignedTo");

            migrationBuilder.DropIndex(
                name: "IX_PostOffice_ManagerId",
                table: "PostOffice");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "PostOffice");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ManagerId",
                table: "PostOffice",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AssignedTo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MailCarrierId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MailId = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimeAssigned = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeCompleted = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignedTo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignedTo_AspNetUsers_MailCarrierId",
                        column: x => x.MailCarrierId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssignedTo_Mail_MailId",
                        column: x => x.MailId,
                        principalTable: "Mail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PostOffice_ManagerId",
                table: "PostOffice",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignedTo_MailCarrierId",
                table: "AssignedTo",
                column: "MailCarrierId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignedTo_MailId",
                table: "AssignedTo",
                column: "MailId");

            migrationBuilder.AddForeignKey(
                name: "FK_PostOffice_AspNetUsers_ManagerId",
                table: "PostOffice",
                column: "ManagerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
