namespace SORMS.API.Services
{
    using Microsoft.EntityFrameworkCore;
    using SORMS.API.Data;
    using SORMS.API.DTOs;
    using SORMS.API.Interfaces;
    using SORMS.API.Models;

    public class ReportService : IReportService
    {
        private readonly SormsDbContext _context;

        public ReportService(SormsDbContext context)
        {
            _context = context;
        }

        public async Task<ReportDto> GenerateOccupancyReportAsync()
        {
            var totalRooms = await _context.Rooms.CountAsync();
            var occupiedRooms = await _context.Rooms.CountAsync(r => r.IsOccupied);
            var occupancyRate = totalRooms == 0 ? 0 : (double)occupiedRooms / totalRooms * 100;

            var report = new Report
            {
                Title = "Occupancy Report",
                GeneratedDate = DateTime.UtcNow,
                CreatedBy = "System",
                Content = $"Total Rooms: {totalRooms}, Occupied: {occupiedRooms}, Occupancy Rate: {occupancyRate:F2}%"
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return new ReportDto
            {
                Id = report.Id,
                Title = report.Title,
                GeneratedDate = report.GeneratedDate,
                CreatedBy = report.CreatedBy
            };
        }

        public async Task<ReportDto> GenerateServiceUsageReportAsync()
        {
            var totalRequests = await _context.ServiceRequests.CountAsync();
            var completedRequests = await _context.ServiceRequests.CountAsync(r => r.Status == "Completed");

            var report = new Report
            {
                Title = "Service Usage Report",
                GeneratedDate = DateTime.UtcNow,
                CreatedBy = "System",
                Content = $"Total Requests: {totalRequests}, Completed: {completedRequests}"
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return new ReportDto
            {
                Id = report.Id,
                Title = report.Title,
                GeneratedDate = report.GeneratedDate,
                CreatedBy = report.CreatedBy
            };
        }

        public async Task<ReportDto> GenerateRevenueReportAsync()
        {
            var totalRevenue = await _context.Billings
                .Where(b => b.IsPaid)
                .SumAsync(b => b.Amount);

            var report = new Report
            {
                Title = "Revenue Report",
                GeneratedDate = DateTime.UtcNow,
                CreatedBy = "System",
                Content = $"Total Revenue Collected: {totalRevenue:C}"
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return new ReportDto
            {
                Id = report.Id,
                Title = report.Title,
                GeneratedDate = report.GeneratedDate,
                CreatedBy = report.CreatedBy
            };
        }
    }

}
