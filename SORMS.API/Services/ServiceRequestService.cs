namespace SORMS.API.Services
{
    using Microsoft.EntityFrameworkCore;
    using SORMS.API.Data;
    using SORMS.API.DTOs;
    using SORMS.API.Interfaces;
    using SORMS.API.Models;

    public class ServiceRequestService : IServiceRequestService
    {
        private readonly SormsDbContext _context;

        public ServiceRequestService(SormsDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ServiceRequestDto>> GetAllRequestsAsync()
        {
            var requests = await _context.ServiceRequests
                .Include(r => r.Resident)
                .Include(r => r.Staff)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            return requests.Select(r => new ServiceRequestDto
            {
                Id = r.Id,
                ServiceType = r.ServiceType,
                Description = r.Description,
                Status = r.Status,
                ResidentId = r.ResidentId,
                StaffId = r.StaffId
            });
        }

        public async Task<ServiceRequestDto> GetRequestByIdAsync(int id)
        {
            var request = await _context.ServiceRequests
                .Include(r => r.Resident)
                .Include(r => r.Staff)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return null;

            return new ServiceRequestDto
            {
                Id = request.Id,
                ServiceType = request.ServiceType,
                Description = request.Description,
                Status = request.Status,
                ResidentId = request.ResidentId,
                StaffId = request.StaffId
            };
        }

        public async Task<ServiceRequestDto> CreateRequestAsync(ServiceRequestDto requestDto)
        {
            var request = new ServiceRequest
            {
                ServiceType = requestDto.ServiceType,
                Description = requestDto.Description,
                Status = "Pending",
                RequestDate = DateTime.UtcNow,
                ResidentId = requestDto.ResidentId,
                StaffId = requestDto.StaffId
            };

            _context.ServiceRequests.Add(request);
            await _context.SaveChangesAsync();

            requestDto.Id = request.Id;
            requestDto.Status = request.Status;
            return requestDto;
        }

        public async Task<bool> UpdateRequestStatusAsync(int id, string status)
        {
            var request = await _context.ServiceRequests.FindAsync(id);
            if (request == null) return false;

            request.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteRequestAsync(int id)
        {
            var request = await _context.ServiceRequests.FindAsync(id);
            if (request == null) return false;

            _context.ServiceRequests.Remove(request);
            await _context.SaveChangesAsync();
            return true;
        }
    }

}
