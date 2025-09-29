using Microsoft.AspNetCore.Mvc;
using SORMS.API.DTOs;
using SORMS.API.Interfaces;

namespace SORMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckInController : ControllerBase
    {
        private readonly ICheckInService _checkInService;

        public CheckInController(ICheckInService checkInService)
        {
            _checkInService = checkInService;
        }

        /// <summary>
        /// Cư dân thực hiện check-in bằng mã QR
        /// </summary>
        [HttpPost("checkin")]
        public async Task<IActionResult> CheckIn([FromQuery] int residentId, [FromQuery] string qrCodeData)
        {
            var success = await _checkInService.CheckInAsync(residentId, qrCodeData);
            if (!success)
                return BadRequest("Không thể check-in. Kiểm tra thông tin cư dân hoặc phòng.");

            return Ok("Check-in thành công.");
        }

        /// <summary>
        /// Cư dân thực hiện check-out bằng mã QR
        /// </summary>
        [HttpPost("checkout")]
        public async Task<IActionResult> CheckOut([FromQuery] int residentId, [FromQuery] string qrCodeData)
        {
            var success = await _checkInService.CheckOutAsync(residentId, qrCodeData);
            if (!success)
                return BadRequest("Không thể check-out. Kiểm tra trạng thái cư dân hoặc phòng.");

            return Ok("Check-out thành công.");
        }

        /// <summary>
        /// Lấy trạng thái check-in mới nhất của cư dân
        /// </summary>
        [HttpGet("latest/{residentId}")]
        public async Task<IActionResult> GetLatestStatus(int residentId)
        {
            var status = await _checkInService.GetLatestCheckInStatusAsync(residentId);
            if (status == null)
                return NotFound("Không có bản ghi check-in nào.");

            return Ok(status);
        }

        /// <summary>
        /// Lấy lịch sử check-in của cư dân
        /// </summary>
        [HttpGet("history/{residentId}")]
        public async Task<IActionResult> GetHistory(int residentId)
        {
            var history = await _checkInService.GetCheckInHistoryAsync(residentId);
            return Ok(history);
        }
    }
}
