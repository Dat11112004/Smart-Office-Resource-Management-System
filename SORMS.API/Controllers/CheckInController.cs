using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SORMS.API.DTOs;
using SORMS.API.Interfaces;
using System.Security.Claims;

namespace SORMS.API.Controllers
{
    [Authorize]
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
        /// [RESIDENT] Tạo yêu cầu check-in vào phòng
        /// </summary>
        [Authorize(Roles = "Resident")]
        [HttpPost("request-checkin")]
        public async Task<IActionResult> RequestCheckIn([FromBody] CreateCheckInRequestDto request)
        {
            try
            {
                // Log tất cả claims để debug
                var allClaims = string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}"));
                Console.WriteLine($"[CheckIn API] All Claims: {allClaims}");
                
                var residentIdClaim = User.FindFirst("ResidentId");
                Console.WriteLine($"[CheckIn API] ResidentId Claim: {residentIdClaim?.Value ?? "NULL"}");
                
                var residentId = int.Parse(residentIdClaim?.Value ?? "0");
                if (residentId == 0)
                {
                    Console.WriteLine("[CheckIn API] ERROR: ResidentId = 0, không tìm thấy claim ResidentId");
                    return BadRequest(new { success = false, message = "Không tìm thấy thông tin resident. Vui lòng logout và login lại." });
                }

                Console.WriteLine($"[CheckIn API] Creating check-in request for ResidentId={residentId}, RoomId={request.RoomId}");
                var result = await _checkInService.CreateCheckInRequestAsync(residentId, request.RoomId);
                return Ok(new { 
                    success = true, 
                    message = "Yêu cầu check-in đã được gửi. Vui lòng chờ Staff/Admin phê duyệt.", 
                    data = result 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// [RESIDENT] Tạo yêu cầu check-out khỏi phòng
        /// </summary>
        [Authorize(Roles = "Resident")]
        [HttpPost("request-checkout")]
        public async Task<IActionResult> RequestCheckOut([FromBody] CreateCheckOutRequestDto request)
        {
            try
            {
                var residentId = int.Parse(User.FindFirst("ResidentId")?.Value ?? "0");
                if (residentId == 0)
                    return BadRequest("Không tìm thấy thông tin resident");

                var result = await _checkInService.CreateCheckOutRequestAsync(residentId, request.CheckInRecordId);
                return Ok(new { 
                    success = true, 
                    message = "Yêu cầu check-out đã được gửi. Vui lòng chờ Staff/Admin phê duyệt.", 
                    data = result 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// [STAFF/ADMIN] Phê duyệt hoặc từ chối yêu cầu check-in
        /// </summary>
        [Authorize(Roles = "Admin,Staff")]
        [HttpPost("approve-checkin")]
        public async Task<IActionResult> ApproveCheckIn([FromBody] ApproveCheckInRequestDto request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                    return BadRequest("Không tìm thấy thông tin user");

                var result = await _checkInService.ApproveCheckInRequestAsync(
                    request.RequestId, 
                    userId, 
                    request.IsApproved, 
                    request.RejectReason
                );

                var message = request.IsApproved ? "Đã phê duyệt yêu cầu check-in" : "Đã từ chối yêu cầu check-in";
                return Ok(new { success = true, message = message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// [STAFF/ADMIN] Phê duyệt hoặc từ chối yêu cầu check-out
        /// </summary>
        [Authorize(Roles = "Admin,Staff")]
        [HttpPost("approve-checkout")]
        public async Task<IActionResult> ApproveCheckOut([FromBody] ApproveCheckInRequestDto request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                    return BadRequest("Không tìm thấy thông tin user");

                var result = await _checkInService.ApproveCheckOutRequestAsync(
                    request.RequestId, 
                    userId, 
                    request.IsApproved, 
                    request.RejectReason
                );

                var message = request.IsApproved ? "Đã phê duyệt yêu cầu check-out" : "Đã từ chối yêu cầu check-out";
                return Ok(new { success = true, message = message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// [STAFF/ADMIN] Lấy danh sách yêu cầu check-in chờ phê duyệt
        /// </summary>
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("pending-checkin")]
        public async Task<IActionResult> GetPendingCheckInRequests()
        {
            try
            {
                var requests = await _checkInService.GetPendingCheckInRequestsAsync();
                return Ok(new { success = true, data = requests });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// [STAFF/ADMIN] Lấy danh sách yêu cầu check-out chờ phê duyệt
        /// </summary>
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("pending-checkout")]
        public async Task<IActionResult> GetPendingCheckOutRequests()
        {
            try
            {
                var requests = await _checkInService.GetPendingCheckOutRequestsAsync();
                return Ok(new { success = true, data = requests });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// [RESIDENT] Lấy trạng thái check-in hiện tại của mình
        /// </summary>
        [Authorize(Roles = "Resident")]
        [HttpGet("my-status")]
        public async Task<IActionResult> GetMyCurrentStatus()
        {
            try
            {
                var residentId = int.Parse(User.FindFirst("ResidentId")?.Value ?? "0");
                if (residentId == 0)
                    return BadRequest("Không tìm thấy thông tin resident");

                var status = await _checkInService.GetCurrentCheckInStatusAsync(residentId);
                return Ok(new { success = true, data = status });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// [RESIDENT] Lấy lịch sử check-in của mình
        /// </summary>
        [Authorize(Roles = "Resident")]
        [HttpGet("my-history")]
        public async Task<IActionResult> GetMyHistory()
        {
            try
            {
                var residentId = int.Parse(User.FindFirst("ResidentId")?.Value ?? "0");
                if (residentId == 0)
                    return BadRequest("Không tìm thấy thông tin resident");

                var history = await _checkInService.GetCheckInHistoryAsync(residentId);
                return Ok(new { success = true, data = history });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// [STAFF/ADMIN] Lấy tất cả records check-in
        /// </summary>
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllCheckInRecords()
        {
            try
            {
                var records = await _checkInService.GetAllCheckInRecordsAsync();
                return Ok(new { success = true, data = records });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
