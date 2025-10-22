using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SORMS.API.DTOs;
using SORMS.API.Interfaces;

namespace SORMS.API.Controllers
{
    [Authorize(Roles = "Admin,Resident")]
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
        /// Lấy danh sách tất cả cư dân
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var residents = await _residentService.GetAllResidentsAsync();
            return Ok(residents);
        }

        /// <summary>
        /// Lấy thông tin cư dân theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var resident = await _residentService.GetResidentByIdAsync(id);
            if (resident == null)
                return NotFound("Không tìm thấy cư dân.");

            return Ok(resident);
        }

        /// <summary>
        /// Tạo mới cư dân
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ResidentDto residentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _residentService.CreateResidentAsync(residentDto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Cập nhật thông tin cư dân
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ResidentDto residentDto)
        {
            var success = await _residentService.UpdateResidentAsync(id, residentDto);
            if (!success)
                return NotFound("Không thể cập nhật cư dân.");

            return NoContent();
        }

        /// <summary>
        /// Xóa cư dân
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _residentService.DeleteResidentAsync(id);
            if (!success)
                return NotFound("Không thể xóa cư dân.");

            return NoContent();
        }

        /// <summary>
        /// Lấy danh sách cư dân theo phòng
        /// </summary>
        [HttpGet("room/{roomId}")]
        public async Task<IActionResult> GetByRoom(int roomId)
        {
            var residents = await _residentService.GetResidentsByRoomIdAsync(roomId);
            return Ok(residents);
        }

        /// <summary>
        /// Check-in cư dân theo ngày
        /// </summary>
        [HttpPut("{residentId}/checkin")]
        public async Task<IActionResult> CheckIn(int residentId, [FromQuery] DateTime date)
        {
            var success = await _residentService.CheckInAsync(residentId, date);
            if (!success)
                return BadRequest("Không thể check-in.");

            return Ok("Check-in thành công.");
        }

        /// <summary>
        /// Check-out cư dân theo ngày
        /// </summary>
        [HttpPut("{residentId}/checkout")]
        public async Task<IActionResult> CheckOut(int residentId, [FromQuery] DateTime date)
        {
            var success = await _residentService.CheckOutAsync(residentId, date);
            if (!success)
                return BadRequest("Không thể check-out.");

            return Ok("Check-out thành công.");
        }
    }
}
