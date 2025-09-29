namespace SORMS.API.Services
{
    using Microsoft.EntityFrameworkCore;
    using SORMS.API.Data;
    using SORMS.API.DTOs;
    using SORMS.API.Models;

    public class ResidentService : IResidentService
    {
        private readonly SormsDbContext _context;

        public ResidentService(SormsDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ResidentDto>> GetAllResidentsAsync()
        {
            var residents = await _context.Residents
                .Include(r => r.Room)
                .ToListAsync();

            return residents.Select(r => new ResidentDto
            {
                Id = r.Id,
                FullName = r.FullName,
                Email = r.Email,
                Phone = r.Phone,
                IdentityNumber = r.IdentityNumber,
                Role = r.Role,
                RoomId = r.RoomId
            });
        }

        public async Task<ResidentDto> GetResidentByIdAsync(int id)
        {
            var resident = await _context.Residents
                .Include(r => r.Room)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (resident == null) return null;

            return new ResidentDto
            {
                Id = resident.Id,
                FullName = resident.FullName,
                Email = resident.Email,
                Phone = resident.Phone,
                IdentityNumber = resident.IdentityNumber,
                Role = resident.Role,
                RoomId = resident.RoomId
            };
        }

        public async Task<ResidentDto> CreateResidentAsync(ResidentDto residentDto)
        {
            var resident = new Resident
            {
                FullName = residentDto.FullName,
                Email = residentDto.Email,
                Phone = residentDto.Phone,
                IdentityNumber = residentDto.IdentityNumber,
                Role = residentDto.Role,
                RoomId = residentDto.RoomId
            };

            _context.Residents.Add(resident);
            await _context.SaveChangesAsync();

            residentDto.Id = resident.Id;
            return residentDto;
        }

        public async Task<bool> UpdateResidentAsync(int id, ResidentDto residentDto)
        {
            var resident = await _context.Residents.FindAsync(id);
            if (resident == null) return false;

            resident.FullName = residentDto.FullName;
            resident.Email = residentDto.Email;
            resident.Phone = residentDto.Phone;
            resident.IdentityNumber = residentDto.IdentityNumber;
            resident.Role = residentDto.Role;
            resident.RoomId = residentDto.RoomId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteResidentAsync(int id)
        {
            var resident = await _context.Residents.FindAsync(id);
            if (resident == null) return false;

            _context.Residents.Remove(resident);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ResidentDto>> GetResidentsByRoomIdAsync(int roomId)
        {
            var residents = await _context.Residents
                .Where(r => r.RoomId == roomId)
                .ToListAsync();

            return residents.Select(r => new ResidentDto
            {
                Id = r.Id,
                FullName = r.FullName,
                Email = r.Email,
                Phone = r.Phone,
                IdentityNumber = r.IdentityNumber,
                Role = r.Role,
                RoomId = r.RoomId
            });
        }

        public async Task<bool> CheckInAsync(int residentId, DateTime checkInDate)
        {
            var resident = await _context.Residents.FindAsync(residentId);
            if (resident == null || resident.CheckInDate != default)
                return false;

            resident.CheckInDate = checkInDate;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CheckOutAsync(int residentId, DateTime checkOutDate)
        {
            var resident = await _context.Residents.FindAsync(residentId);
            if (resident == null || resident.CheckOutDate != null)
                return false;

            resident.CheckOutDate = checkOutDate;
            await _context.SaveChangesAsync();
            return true;
        }
    }

}
