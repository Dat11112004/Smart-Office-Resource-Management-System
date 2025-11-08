using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SORMS.API.Migrations
{
    /// <inheritdoc />
    public partial class FixRoomIsAvailableData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fix data cũ: Đồng bộ IsAvailable với IsOccupied
            // Nếu IsOccupied = true → IsAvailable phải = false
            // Nếu IsOccupied = false → IsAvailable phải = true
            migrationBuilder.Sql(@"
                UPDATE Rooms 
                SET IsAvailable = CASE 
                    WHEN IsOccupied = 1 THEN 0 
                    ELSE 1 
                END
                WHERE IsAvailable != CASE 
                    WHEN IsOccupied = 1 THEN 0 
                    ELSE 1 
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
