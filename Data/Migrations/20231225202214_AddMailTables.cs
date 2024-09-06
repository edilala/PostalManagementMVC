using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostalManagementMVC.Data.Migrations
{
    public partial class AddMailTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Mail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecipientAddress = table.Column<string>(type: "nvarchar(510)", maxLength: 510, nullable: false),
                    SenderAddress = table.Column<string>(type: "nvarchar(510)", maxLength: 510, nullable: true),
                    StartLocationId = table.Column<int>(type: "int", nullable: false),
                    EndLocationId = table.Column<int>(type: "int", nullable: false),
                    TimeInserted = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeDelivered = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mail_Location_EndLocationId",
                        column: x => x.EndLocationId,
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Mail_Location_StartLocationId",
                        column: x => x.StartLocationId,
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "StatusCatalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusCatalog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssignedTo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MailId = table.Column<int>(type: "int", nullable: false),
                    MailCarrierId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TimeAssigned = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeCompleted = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignedTo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignedTo_AspNetUsers_MailCarrierId",
                        column: x => x.MailCarrierId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_AssignedTo_Mail_MailId",
                        column: x => x.MailId,
                        principalTable: "Mail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "MailStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MailId = table.Column<int>(type: "int", nullable: false),
                    StatusCatalogId = table.Column<int>(type: "int", nullable: false),
                    CarrierId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TimeAssigned = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MailStatus_AspNetUsers_CarrierId",
                        column: x => x.CarrierId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_MailStatus_Mail_MailId",
                        column: x => x.MailId,
                        principalTable: "Mail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MailStatus_StatusCatalog_StatusCatalogId",
                        column: x => x.StatusCatalogId,
                        principalTable: "StatusCatalog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssignedTo_MailCarrierId",
                table: "AssignedTo",
                column: "MailCarrierId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignedTo_MailId",
                table: "AssignedTo",
                column: "MailId");

            migrationBuilder.CreateIndex(
                name: "IX_Mail_EndLocationId",
                table: "Mail",
                column: "EndLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Mail_StartLocationId",
                table: "Mail",
                column: "StartLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_MailStatus_CarrierId",
                table: "MailStatus",
                column: "CarrierId");

            migrationBuilder.CreateIndex(
                name: "IX_MailStatus_MailId",
                table: "MailStatus",
                column: "MailId");

            migrationBuilder.CreateIndex(
                name: "IX_MailStatus_StatusCatalogId",
                table: "MailStatus",
                column: "StatusCatalogId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssignedTo");

            migrationBuilder.DropTable(
                name: "MailStatus");

            migrationBuilder.DropTable(
                name: "Mail");

            migrationBuilder.DropTable(
                name: "StatusCatalog");
        }
    }
}
