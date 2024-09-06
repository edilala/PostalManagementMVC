using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostalManagementMVC.Data.Migrations
{
    public partial class ChangePostOfficeIdWithFlagInLocation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Location_PostOffice_PostOfficeId",
                table: "Location");

            migrationBuilder.DropIndex(
                name: "IX_Location_PostOfficeId",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "PostOfficeId",
                table: "Location");

            migrationBuilder.AddColumn<bool>(
                name: "IsPostOffice",
                table: "Location",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPostOffice",
                table: "Location");

            migrationBuilder.AddColumn<int>(
                name: "PostOfficeId",
                table: "Location",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Location_PostOfficeId",
                table: "Location",
                column: "PostOfficeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Location_PostOffice_PostOfficeId",
                table: "Location",
                column: "PostOfficeId",
                principalTable: "PostOffice",
                principalColumn: "Id");
        }
    }
}
