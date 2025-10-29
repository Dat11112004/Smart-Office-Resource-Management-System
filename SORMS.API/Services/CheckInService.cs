namespace SORMS.API.Services
{
    using Microsoft.EntityFrameworkCore;
    using SORMS.API.Data;
    using SORMS.API.DTOs;
    using SORMS.API.Interfaces;
    using SORMS.API.Models;



    public class CheckInService : ICheckInService
    {
        private readonly SormsDbContext _context;

        public CheckInService(SormsDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CheckInAsync(int residentId, string qrCodeData)
        {
            var resident = await _context.Residents.FindAsync(residentId);
            if (resident == null || resident.CheckInDate != default)
                return false;

            // Kiểm tra resident có RoomId không
            if (!resident.RoomId.HasValue)
                return false;

            var room = await _context.Rooms.FindAsync(resident.RoomId.Value);
            if (room == null || room.IsOccupied)
                return false;

            var record = new CheckInRecord
            {
                ResidentId = residentId,
                RoomId = resident.RoomId.Value,
                CheckInTime = DateTime.UtcNow,
                Status = "CheckedIn",
                Method = "QR",
            };

            room.IsOccupied = true;
            resident.CheckInDate = DateTime.UtcNow;

            _context.CheckInRecords.Add(record);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CheckOutAsync(int residentId, string qrCodeData)
        {
            var resident = await _context.Residents.FindAsync(residentId);
            if (resident == null || resident.CheckOutDate != null)
                return false;

            var room = await _context.Rooms.FindAsync(resident.RoomId);
            if (room == null)
                return false;

            var record = await _context.CheckInRecords
                .Where(r => r.ResidentId == residentId && r.Status == "CheckedIn")
                .OrderByDescending(r => r.CheckInTime)
                .FirstOrDefaultAsync();

            if (record == null)
                return false;

            record.CheckOutTime = DateTime.UtcNow;
            record.Status = "CheckedOut";
            record.Method = "QR";

            room.IsOccupied = false;
            resident.CheckOutDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CheckInRecordDto> GetLatestCheckInStatusAsync(int residentId)
        {
            var record = await _context.CheckInRecords
                .Include(r => r.Room)
                .Where(r => r.ResidentId == residentId)
                .OrderByDescending(r => r.CheckInTime)
                .FirstOrDefaultAsync();

            if (record == null) return null;

            return new CheckInRecordDto
            {
                Id = record.Id,
                ResidentId = record.ResidentId,
                RoomId = record.RoomId,
                RoomNumber = record.Room.RoomNumber,
                CheckInTime = record.CheckInTime,
                CheckOutTime = record.CheckOutTime,
                Status = record.Status,
                Method = record.Method
            };
        }

        public async Task<IEnumerable<CheckInRecordDto>> GetCheckInHistoryAsync(int residentId)
        {
            var records = await _context.CheckInRecords
                .Include(r => r.Room)
                .Where(r => r.ResidentId == residentId)
                .OrderByDescending(r => r.CheckInTime)
                .ToListAsync();

            return records.Select(record => new CheckInRecordDto
            {
                Id = record.Id,
                ResidentId = record.ResidentId,
                RoomId = record.RoomId,
                RoomNumber = record.Room.RoomNumber,
                CheckInTime = record.CheckInTime,
                CheckOutTime = record.CheckOutTime,
                Status = record.Status,
                Method = record.Method
            });
        }
    }

}
