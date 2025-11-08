using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SORMS.FE.Pages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SORMS.FE.Pages.Resident
{
    public class CheckInStatusModel : BasePageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CheckInStatusModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public CheckInRecordDto? CurrentStatus { get; set; }
        public List<CheckInRecordDto> History { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            try
            {
                var client = _httpClientFactory.CreateClient("API");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Lấy trạng thái hiện tại
                var statusResponse = await client.GetAsync("api/CheckIn/my-status");
                if (statusResponse.IsSuccessStatusCode)
                {
                    var statusJson = await statusResponse.Content.ReadAsStringAsync();
                    var statusResult = JsonSerializer.Deserialize<ApiResponse<CheckInRecordDto>>(statusJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    CurrentStatus = statusResult?.Data;
                }

                // Lấy lịch sử
                var historyResponse = await client.GetAsync("api/CheckIn/my-history");
                if (historyResponse.IsSuccessStatusCode)
                {
                    var historyJson = await historyResponse.Content.ReadAsStringAsync();
                    var historyResult = JsonSerializer.Deserialize<ApiResponse<List<CheckInRecordDto>>>(historyJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    History = historyResult?.Data ?? new List<CheckInRecordDto>();
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải thông tin: " + ex.Message;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCheckOutAsync(int checkInRecordId)
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            try
            {
                var client = _httpClientFactory.CreateClient("API");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var requestDto = new { CheckInRecordId = checkInRecordId };
                var content = new StringContent(
                    JsonSerializer.Serialize(requestDto),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync("api/CheckIn/request-checkout", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Yêu cầu check-out đã được gửi! Vui lòng chờ Staff/Admin phê duyệt.";
                }
                else
                {
                    var error = JsonSerializer.Deserialize<ApiErrorResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    TempData["ErrorMessage"] = error?.Message ?? "Không thể gửi yêu cầu check-out";
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
