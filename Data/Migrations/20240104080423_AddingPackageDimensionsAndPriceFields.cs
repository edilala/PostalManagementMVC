using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostalManagementMVC.Data.Migrations
{
    public partial class AddingPackageDimensionsAndPriceFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "MailStatus",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Fee",
                table: "MailCategory",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AlterColumn<Guid>(
                name: "Code",
                table: "Mail",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<double>(
                name: "Height",
                table: "Mail",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Hight",
                table: "Mail",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "ReceiverContactNr",
                table: "Mail",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Weight",
                table: "Mail",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Width",
                table: "Mail",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "LocationAssignedId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_LocationAssignedId",
                table: "AspNetUsers",
                column: "LocationAssignedId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Location_LocationAssignedId",
                table: "AspNetUsers",
                column: "LocationAssignedId",
                principalTable: "Location",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Location_LocationAssignedId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_LocationAssignedId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "MailStatus");

            migrationBuilder.DropColumn(
                name: "Fee",
                table: "MailCategory");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "Mail");

            migrationBuilder.DropColumn(
                name: "Hight",
                table: "Mail");

            migrationBuilder.DropColumn(
                name: "ReceiverContactNr",
                table: "Mail");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "Mail");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "Mail");

            migrationBuilder.DropColumn(
                name: "LocationAssignedId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<Guid>(
                name: "Code",
                table: "Mail",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
