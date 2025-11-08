using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SORMS.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckInApprovalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Billings");

            migrationBuilder.RenameColumn(
                name: "Method",
                table: "CheckInRecords",
                newName: "RequestType");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "CheckInRecords",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CheckInTime",
                table: "CheckInRecords",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "ApprovedBy",
                table: "CheckInRecords",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedTime",
                table: "CheckInRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckOutRequestTime",
                table: "CheckInRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectReason",
                table: "CheckInRecords",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RequestTime",
                table: "CheckInRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_CheckInRecords_ApprovedBy",
                table: "CheckInRecords",
                column: "ApprovedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_CheckInRecords_Users_ApprovedBy",
                table: "CheckInRecords",
                column: "ApprovedBy",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CheckInRecords_Users_ApprovedBy",
                table: "CheckInRecords");

            migrationBuilder.DropIndex(
                name: "IX_CheckInRecords_ApprovedBy",
                table: "CheckInRecords");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "CheckInRecords");

            migrationBuilder.DropColumn(
                name: "ApprovedTime",
                table: "CheckInRecords");

            migrationBuilder.DropColumn(
                name: "CheckOutRequestTime",
                table: "CheckInRecords");

            migrationBuilder.DropColumn(
                name: "RejectReason",
                table: "CheckInRecords");

            migrationBuilder.DropColumn(
                name: "RequestTime",
                table: "CheckInRecords");

            migrationBuilder.RenameColumn(
                name: "RequestType",
                table: "CheckInRecords",
                newName: "Method");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "CheckInRecords",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CheckInTime",
                table: "CheckInRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Billings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResidentId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BillingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Billings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Billings_Residents_ResidentId",
                        column: x => x.ResidentId,
                        principalTable: "Residents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Billings_ResidentId",
                table: "Billings",
                column: "ResidentId");
        }
    }
}
