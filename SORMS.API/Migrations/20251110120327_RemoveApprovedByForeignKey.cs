using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SORMS.API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveApprovedByForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CheckInRecords_Users_ApprovedBy",
                table: "CheckInRecords");

            migrationBuilder.DropIndex(
                name: "IX_CheckInRecords_ApprovedBy",
                table: "CheckInRecords");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}
