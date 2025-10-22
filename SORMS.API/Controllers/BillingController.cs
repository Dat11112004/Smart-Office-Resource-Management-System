using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SORMS.API.DTOs;
using SORMS.API.Interfaces;

namespace SORMS.API.Controllers
{
    [Authorize (Roles = "Admin,Staff,Resident")]
    [ApiController]
    [Route("api/[controller]")]
    public class BillingController : ControllerBase
    {
        private readonly IBillingService _billingService;

        public BillingController(IBillingService billingService)
        {
            _billingService = billingService;
        }

        /// <summary>
        /// Lấy danh sách tất cả hóa đơn
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var billings = await _billingService.GetAllBillingsAsync();
            return Ok(billings);
        }

        /// <summary>
        /// Lấy chi tiết hóa đơn theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var billing = await _billingService.GetBillingByIdAsync(id);
            if (billing == null)
                return NotFound("Không tìm thấy hóa đơn.");

            return Ok(billing);
        }

        /// <summary>
        /// Tạo mới một hóa đơn
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BillingDto billingDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _billingService.CreateBillingAsync(billingDto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Đánh dấu hóa đơn đã thanh toán
        /// </summary>
        [HttpPut("{id}/pay")]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            var success = await _billingService.MarkAsPaidAsync(id);
            if (!success)
                return BadRequest("Không thể đánh dấu đã thanh toán.");

            return NoContent();
        }
    }
}
