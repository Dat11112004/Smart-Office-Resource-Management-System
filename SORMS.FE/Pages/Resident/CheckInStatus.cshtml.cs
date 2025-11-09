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
                var statusContent = await statusResponse.Content.ReadAsStringAsync();
                
                Console.WriteLine($"[CheckInStatus] API Response Status: {statusResponse.StatusCode}");
                Console.WriteLine($"[CheckInStatus] API Response Content: {statusContent}");
                
                if (statusResponse.IsSuccessStatusCode)
                {
                    var statusResult = JsonSerializer.Deserialize<ApiResponse<CheckInRecordDto>>(statusContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    Console.WriteLine($"[CheckInStatus] Deserialized Success: {statusResult?.Success}");
                    Console.WriteLine($"[CheckInStatus] Data is NULL: {statusResult?.Data == null}");
                    
                    CurrentStatus = statusResult?.Data;
                    
                    if (CurrentStatus != null)
                    {
                        Console.WriteLine($"[CheckInStatus] CurrentStatus: ID={CurrentStatus.Id}, Status={CurrentStatus.Status}");
                    }
                    else
                    {
                        Console.WriteLine($"[CheckInStatus] CurrentStatus is NULL - No active check-in");
                    }
                }
                else
                {
                    Console.WriteLine($"[CheckInStatus] API returned error status");
                    // Parse error message từ API
                    try
                    {
                        var errorResult = JsonSerializer.Deserialize<ApiErrorResponse>(statusContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        TempData["ErrorMessage"] = errorResult?.Message ?? $"Không thể tải trạng thái check-in. Mã lỗi: {statusResponse.StatusCode}";
                    }
                    catch
                    {
                        TempData["ErrorMessage"] = $"Không thể tải trạng thái check-in. Mã lỗi: {statusResponse.StatusCode}";
                    }
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
