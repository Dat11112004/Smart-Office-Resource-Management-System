using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SORMS.API.DTOs;
using SORMS.API.Interfaces;

namespace SORMS.API.Controllers
{
    [Authorize(Roles = "Admin,Staff,Resident")]
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceRequestController : ControllerBase
    {
        private readonly IServiceRequestService _serviceRequestService;

        public ServiceRequestController(IServiceRequestService serviceRequestService)
        {
            _serviceRequestService = serviceRequestService;
        }

        /// <summary>
        /// Lấy danh sách tất cả yêu cầu dịch vụ
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var requests = await _serviceRequestService.GetAllRequestsAsync();
            return Ok(requests);
        }

        /// <summary>
        /// Lấy thông tin yêu cầu dịch vụ theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var request = await _serviceRequestService.GetRequestByIdAsync(id);
            if (request == null)
                return NotFound("Không tìm thấy yêu cầu dịch vụ.");

            return Ok(request);
        }

        /// <summary>
        /// Tạo yêu cầu dịch vụ mới
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ServiceRequestDto requestDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _serviceRequestService.CreateRequestAsync(requestDto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Cập nhật trạng thái yêu cầu dịch vụ
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromQuery] string status)
        {
            var success = await _serviceRequestService.UpdateRequestStatusAsync(id, status);
            if (!success)
                return BadRequest("Không thể cập nhật trạng thái.");

            return Ok("Cập nhật trạng thái thành công.");
        }

        /// <summary>
        /// Xóa yêu cầu dịch vụ
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _serviceRequestService.DeleteRequestAsync(id);
            if (!success)
                return NotFound("Không thể xóa yêu cầu dịch vụ.");

            return NoContent();
        }
    }
}
