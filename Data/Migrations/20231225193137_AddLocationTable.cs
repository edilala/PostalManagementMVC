using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostalManagementMVC.Data.Migrations
{
    public partial class AddLocationTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConnectionCatalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ConnectionOffsetHours = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectionCatalog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Location",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PostOfficeInChargeId = table.Column<int>(type: "int", nullable: false),
                    PostOfficeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Location", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Location_PostOffice_PostOfficeId",
                        column: x => x.PostOfficeId,
                        principalTable: "PostOffice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Location_PostOffice_PostOfficeInChargeId",
                        column: x => x.PostOfficeInChargeId,
                        principalTable: "PostOffice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LocationConnection",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromLocationId = table.Column<int>(type: "int", nullable: false),
                    ToLocationId = table.Column<int>(type: "int", nullable: false),
                    ConnectionTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationConnection", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationConnection_ConnectionCatalog_ConnectionTypeId",
                        column: x => x.ConnectionTypeId,
                        principalTable: "ConnectionCatalog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LocationConnection_Location_FromLocationId",
                        column: x => x.FromLocationId,
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LocationConnection_Location_ToLocationId",
                        column: x => x.ToLocationId,
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Location_PostOfficeId",
                table: "Location",
                column: "PostOfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_Location_PostOfficeInChargeId",
                table: "Location",
                column: "PostOfficeInChargeId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationConnection_ConnectionTypeId",
                table: "LocationConnection",
                column: "ConnectionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationConnection_FromLocationId",
                table: "LocationConnection",
                column: "FromLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationConnection_ToLocationId",
                table: "LocationConnection",
                column: "ToLocationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocationConnection");

            migrationBuilder.DropTable(
                name: "ConnectionCatalog");

            migrationBuilder.DropTable(
                name: "Location");
        }
    }
}
