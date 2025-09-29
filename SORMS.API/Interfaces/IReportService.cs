using SORMS.API.DTOs;

namespace SORMS.API.Interfaces
{
    public interface IReportService
    {
        Task<ReportDto> GenerateOccupancyReportAsync();
        Task<ReportDto> GenerateServiceUsageReportAsync();
        Task<ReportDto> GenerateRevenueReportAsync();
    }

}
