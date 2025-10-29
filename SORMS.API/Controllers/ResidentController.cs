using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SORMS.API.DTOs;
using SORMS.API.Interfaces;
using System.Security.Claims;

namespace SORMS.API.Controllers
{
    [Authorize] // Tất cả endpoints đều cần đăng nhập
    [ApiController]
    [Route("api/[controller]")]
    public class ResidentController : ControllerBase
    {
        private readonly IResidentService _residentService;

        public ResidentController(IResidentService residentService)
        {
            _residentService = residentService;
        }

        /// <summary>
        /// Lấy danh sách tất cả cư dân - Chỉ Admin và Staff
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAll()
        {
            var residents = await _residentService.GetAllResidentsAsync();
            return Ok(residents);
        }

        /// <summary>
        /// Lấy thông tin cư dân theo ID - Chỉ Admin và Staff
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetById(int id)
        {
            var resident = await _residentService.GetResidentByIdAsync(id);
            if (resident == null)
                return NotFound("Không tìm thấy cư dân.");

            return Ok(resident);
        }

        /// <summary>
        /// Lấy thông tin profile của Resident hiện tại (dựa vào UserId từ JWT)
        /// </summary>
        [HttpGet("my-profile")]
        [Authorize(Roles = "Resident")]
        public async Task<IActionResult> GetMyProfile()
        {
            // Lấy UserId từ JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Không thể xác định người dùng.");
            }

            // Tìm resident theo UserId
            var residents = await _residentService.GetAllResidentsAsync();
            var myResident = residents.FirstOrDefault(r => r.UserId == userId);

            if (myResident == null)
            {
                return NotFound("Không tìm thấy hồ sơ cư dân. Vui lòng liên hệ Admin để được hỗ trợ.");
            }

            return Ok(myResident);
        }

        /// <summary>
        /// Tạo mới cư dân - Chỉ Admin và Staff
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create([FromBody] ResidentDto residentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _residentService.CreateResidentAsync(residentDto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Cập nhật thông tin cư dân - Chỉ Admin và Staff
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Update(int id, [FromBody] ResidentDto residentDto)
        {
            var success = await _residentService.UpdateResidentAsync(id, residentDto);
            if (!success)
                return NotFound("Không thể cập nhật cư dân.");

            return NoContent();
        }

        /// <summary>
        /// Xóa cư dân - Chỉ Admin
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _residentService.DeleteResidentAsync(id);
            if (!success)
                return NotFound("Không thể xóa cư dân.");

            return NoContent();
        }

        /// <summary>
        /// Lấy danh sách cư dân theo phòng - Chỉ Admin và Staff
        /// </summary>
        [HttpGet("room/{roomId}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetByRoom(int roomId)
        {
            var residents = await _residentService.GetResidentsByRoomIdAsync(roomId);
            return Ok(residents);
        }

        /// <summary>
        /// Check-in cư dân theo ngày - Chỉ Admin và Staff
        /// </summary>
        [HttpPut("{residentId}/checkin")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CheckIn(int residentId, [FromQuery] DateTime date)
        {
            var success = await _residentService.CheckInAsync(residentId, date);
            if (!success)
                return BadRequest("Không thể check-in.");

            return Ok("Check-in thành công.");
        }

        /// <summary>
        /// Check-out cư dân theo ngày - Chỉ Admin và Staff
        /// </summary>
        [HttpPut("{residentId}/checkout")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CheckOut(int residentId, [FromQuery] DateTime date)
        {
            var success = await _residentService.CheckOutAsync(residentId, date);
            if (!success)
                return BadRequest("Không thể check-out.");

            return Ok("Check-out thành công.");
        }
    }
}
