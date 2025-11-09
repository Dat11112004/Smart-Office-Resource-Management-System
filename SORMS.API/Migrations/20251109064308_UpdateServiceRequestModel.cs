using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SORMS.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateServiceRequestModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_Staffs_StaffId",
                table: "ServiceRequests");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ServiceRequests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<int>(
                name: "StaffId",
                table: "ServiceRequests",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedDate",
                table: "ServiceRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdated",
                table: "ServiceRequests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "ServiceRequests",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReviewedBy",
                table: "ServiceRequests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedDate",
                table: "ServiceRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StaffFeedback",
                table: "ServiceRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "ServiceRequests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_Staffs_StaffId",
                table: "ServiceRequests",
                column: "StaffId",
                principalTable: "Staffs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_Staffs_StaffId",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "CompletedDate",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "ReviewedBy",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "ReviewedDate",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "StaffFeedback",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "ServiceRequests");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ServiceRequests",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "StaffId",
                table: "ServiceRequests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_Staffs_StaffId",
                table: "ServiceRequests",
                column: "StaffId",
                principalTable: "Staffs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
