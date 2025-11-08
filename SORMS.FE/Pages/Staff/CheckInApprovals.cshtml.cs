using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SORMS.FE.Pages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SORMS.FE.Pages.Staff
{
    public class CheckInApprovalsModel : BasePageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CheckInApprovalsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<CheckInRecordDto> PendingCheckIns { get; set; } = new();
        public List<CheckInRecordDto> PendingCheckOuts { get; set; } = new();
        public List<CheckInRecordDto> AllRecords { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            try
            {
                var client = _httpClientFactory.CreateClient("API");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Lấy yêu cầu check-in chờ phê duyệt
                var checkInResponse = await client.GetAsync("api/CheckIn/pending-checkin");
                if (checkInResponse.IsSuccessStatusCode)
                {
                    var json = await checkInResponse.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<List<CheckInRecordDto>>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    PendingCheckIns = result?.Data ?? new List<CheckInRecordDto>();
                }

                // Lấy yêu cầu check-out chờ phê duyệt
                var checkOutResponse = await client.GetAsync("api/CheckIn/pending-checkout");
                if (checkOutResponse.IsSuccessStatusCode)
                {
                    var json = await checkOutResponse.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<List<CheckInRecordDto>>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    PendingCheckOuts = result?.Data ?? new List<CheckInRecordDto>();
                }

                // Lấy tất cả records
                var allResponse = await client.GetAsync("api/CheckIn/all");
                if (allResponse.IsSuccessStatusCode)
                {
                    var json = await allResponse.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<List<CheckInRecordDto>>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    AllRecords = result?.Data ?? new List<CheckInRecordDto>();
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải dữ liệu: " + ex.Message;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostApproveCheckInAsync(int requestId)
        {
            return await ProcessApprovalAsync(requestId, true, null, "approve-checkin");
        }

        public async Task<IActionResult> OnPostRejectCheckInAsync(int requestId, string rejectReason)
        {
            return await ProcessApprovalAsync(requestId, false, rejectReason, "approve-checkin");
        }

        public async Task<IActionResult> OnPostApproveCheckOutAsync(int requestId)
        {
            return await ProcessApprovalAsync(requestId, true, null, "approve-checkout");
        }

        public async Task<IActionResult> OnPostRejectCheckOutAsync(int requestId, string rejectReason)
        {
            return await ProcessApprovalAsync(requestId, false, rejectReason, "approve-checkout");
        }

        private async Task<IActionResult> ProcessApprovalAsync(int requestId, bool isApproved, string? rejectReason, string endpoint)
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            try
            {
                var client = _httpClientFactory.CreateClient("API");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var requestDto = new
                {
                    RequestId = requestId,
                    IsApproved = isApproved,
                    RejectReason = rejectReason
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(requestDto),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync($"api/CheckIn/{endpoint}", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var action = isApproved ? "phê duyệt" : "từ chối";
                    TempData["SuccessMessage"] = $"Đã {action} yêu cầu thành công!";
                }
                else
                {
                    var error = JsonSerializer.Deserialize<ApiErrorResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    TempData["ErrorMessage"] = error?.Message ?? "Không thể xử lý yêu cầu";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }

            return RedirectToPage();
        }
    }

    public class CheckInRecordDto
    {
        public int Id { get; set; }
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = string.Empty;
        public int RoomId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public DateTime? RequestTime { get; set; }
        public DateTime? ApprovedTime { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutRequestTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? RejectReason { get; set; }
        public int? ApprovedBy { get; set; }
        public string? ApprovedByName { get; set; }
        public string? RequestType { get; set; }
    }
}
