using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostalManagementMVC.Data.Migrations
{
    public partial class AddZipCodeTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PostalCodeId",
                table: "Mail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PostalCode",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ZipCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostalCode", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostalCode_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Mail_PostalCodeId",
                table: "Mail",
                column: "PostalCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_PostalCode_LocationId",
                table: "PostalCode",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Mail_PostalCode_PostalCodeId",
                table: "Mail",
                column: "PostalCodeId",
                principalTable: "PostalCode",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mail_PostalCode_PostalCodeId",
                table: "Mail");

            migrationBuilder.DropTable(
                name: "PostalCode");

            migrationBuilder.DropIndex(
                name: "IX_Mail_PostalCodeId",
                table: "Mail");

            migrationBuilder.DropColumn(
                name: "PostalCodeId",
                table: "Mail");
        }
    }
}
