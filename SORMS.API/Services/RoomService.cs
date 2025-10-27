using Microsoft.EntityFrameworkCore;
using SORMS.API.Data;
using SORMS.API.DTOs;
using SORMS.API.Interfaces;
using SORMS.API.Models;

namespace SORMS.API.Services
{
    public class RoomService : IRoomService
    {
        private readonly SormsDbContext _context;

        public RoomService(SormsDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RoomDto>> GetAllRoomsAsync()
        {
            var rooms = await _context.Rooms.ToListAsync();

            return rooms.Select(r => new RoomDto
            {
                Id = r.Id,
                RoomNumber = r.RoomNumber,
                Type = r.Type,
                RoomType = r.Type, // Alias
                Floor = r.Floor,
                MonthlyRent = r.MonthlyRent,
                Area = r.Area,
                IsOccupied = r.IsOccupied,
                IsAvailable = r.IsAvailable,
                CurrentResident = r.CurrentResident,
                Description = r.Description,
                IsActive = r.IsActive
            });
        }

        public async Task<RoomDto> GetRoomByIdAsync(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return null;

            return new RoomDto
            {
                Id = room.Id,
                RoomNumber = room.RoomNumber,
                Type = room.Type,
                RoomType = room.Type, // Alias
                Floor = room.Floor,
                MonthlyRent = room.MonthlyRent,
                Area = room.Area,
                IsOccupied = room.IsOccupied,
                IsAvailable = room.IsAvailable,
                CurrentResident = room.CurrentResident,
                Description = room.Description,
                IsActive = room.IsActive
            };
        }

        public async Task<RoomDto> CreateRoomAsync(RoomDto roomDto)
        {
            var room = new Room
            {
                RoomNumber = roomDto.RoomNumber,
                Type = roomDto.Type,
                Floor = roomDto.Floor,
                MonthlyRent = roomDto.MonthlyRent,
                Area = roomDto.Area,
                IsOccupied = roomDto.IsOccupied,
                IsAvailable = roomDto.IsAvailable,
                Description = roomDto.Description,
                CurrentResident = roomDto.CurrentResident,
                IsActive = roomDto.IsActive
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            roomDto.Id = room.Id;
            return roomDto;
        }

        public async Task<bool> UpdateRoomAsync(int id, RoomDto roomDto)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return false;

            room.RoomNumber = roomDto.RoomNumber;
            room.Type = roomDto.Type;
            room.IsOccupied = roomDto.IsOccupied;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteRoomAsync(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return false;

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<RoomDto>> GetAvailableRoomsAsync()
        {
            var rooms = await _context.Rooms
                .Where(r => !r.IsOccupied)
                .ToListAsync();

            return rooms.Select(r => new RoomDto
            {
                Id = r.Id,
                RoomNumber = r.RoomNumber,
                Type = r.Type,
                RoomType = r.Type, // Alias
                Floor = r.Floor,
                MonthlyRent = r.MonthlyRent,
                Area = r.Area,
                IsOccupied = r.IsOccupied,
                IsAvailable = r.IsAvailable,
                CurrentResident = r.CurrentResident,
                Description = r.Description,
                IsActive = r.IsActive
            });
        }
    }
}
