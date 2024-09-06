using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostalManagementMVC.Data.Migrations
{
    public partial class SetPostOfficeIdAsNullableInLocationEnt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Location_PostOffice_PostOfficeId",
                table: "Location");

            migrationBuilder.AlterColumn<int>(
                name: "PostOfficeId",
                table: "Location",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Location_PostOffice_PostOfficeId",
                table: "Location",
                column: "PostOfficeId",
                principalTable: "PostOffice",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Location_PostOffice_PostOfficeId",
                table: "Location");

            migrationBuilder.AlterColumn<int>(
                name: "PostOfficeId",
                table: "Location",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Location_PostOffice_PostOfficeId",
                table: "Location",
                column: "PostOfficeId",
                principalTable: "PostOffice",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
