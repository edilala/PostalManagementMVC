using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostalManagementMVC.Data.Migrations
{
    public partial class SetMailFieldsAsNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mail_Mail_MailBundleId",
                table: "Mail");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeDelivered",
                table: "Mail",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "SenderAddress",
                table: "Mail",
                type: "nvarchar(510)",
                maxLength: 510,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(510)",
                oldMaxLength: 510);

            migrationBuilder.AlterColumn<int>(
                name: "MailBundleId",
                table: "Mail",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Mail_Mail_MailBundleId",
                table: "Mail",
                column: "MailBundleId",
                principalTable: "Mail",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mail_Mail_MailBundleId",
                table: "Mail");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeDelivered",
                table: "Mail",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SenderAddress",
                table: "Mail",
                type: "nvarchar(510)",
                maxLength: 510,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(510)",
                oldMaxLength: 510,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MailBundleId",
                table: "Mail",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Mail_Mail_MailBundleId",
                table: "Mail",
                column: "MailBundleId",
                principalTable: "Mail",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
