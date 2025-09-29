using Microsoft.AspNetCore.Mvc;
using SORMS.API.DTOs;
using SORMS.API.Interfaces;

namespace SORMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        /// <summary>
        /// Tạo báo cáo tỷ lệ phòng đã sử dụng
        /// </summary>
        [HttpPost("occupancy")]
        public async Task<IActionResult> GenerateOccupancyReport()
        {
            var report = await _reportService.GenerateOccupancyReportAsync();
            return Ok(report);
        }

        /// <summary>
        /// Tạo báo cáo sử dụng dịch vụ
        /// </summary>
        [HttpPost("service-usage")]
        public async Task<IActionResult> GenerateServiceUsageReport()
        {
            var report = await _reportService.GenerateServiceUsageReportAsync();
            return Ok(report);
        }

        /// <summary>
        /// Tạo báo cáo doanh thu đã thu
        /// </summary>
        [HttpPost("revenue")]
        public async Task<IActionResult> GenerateRevenueReport()
        {
            var report = await _reportService.GenerateRevenueReportAsync();
            return Ok(report);
        }
    }
}
