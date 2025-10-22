using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SORMS.API.DTOs;
using SORMS.API.Interfaces;

namespace SORMS.API.Controllers
{
    [Authorize(Roles = "Admin,Staff,Resident")]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Tạo thông báo mới cho cư dân
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NotificationDto notificationDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _notificationService.CreateNotificationAsync(notificationDto);
            return CreatedAtAction(nameof(GetByResident), new { residentId = created.ResidentId }, created);
        }

        /// <summary>
        /// Lấy danh sách thông báo của cư dân
        /// </summary>
        [HttpGet("resident/{residentId}")]
        public async Task<IActionResult> GetByResident(int residentId)
        {
            var notifications = await _notificationService.GetNotificationsForResidentAsync(residentId);
            return Ok(notifications);
        }

        /// <summary>
        /// Đánh dấu thông báo là đã đọc
        /// </summary>
        [HttpPut("{notificationId}/read")]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            var success = await _notificationService.MarkAsReadAsync(notificationId);
            if (!success)
                return BadRequest("Không thể đánh dấu đã đọc.");

            return NoContent();
        }
    }
}
