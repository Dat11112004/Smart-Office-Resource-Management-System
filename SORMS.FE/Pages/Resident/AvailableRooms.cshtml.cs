using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SORMS.FE.Pages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SORMS.FE.Pages.Resident
{
    public class AvailableRoomsModel : BasePageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AvailableRoomsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<RoomDto> AvailableRooms { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            try
            {
                var client = _httpClientFactory.CreateClient("API");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Lấy danh sách phòng trống
                var response = await client.GetAsync("api/Room/available");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    AvailableRooms = JsonSerializer.Deserialize<List<RoomDto>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<RoomDto>();
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải danh sách phòng: " + ex.Message;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int roomId)
        {
            Console.WriteLine($"=== OnPostAsync Check-in được gọi với RoomId: {roomId} ===");
            
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Token không tồn tại, redirect về Login");
                return RedirectToPage("/Auth/Login");
            }

            Console.WriteLine($"Token: {token.Substring(0, 50)}...");

            try
            {
                var client = _httpClientFactory.CreateClient("API");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Gửi yêu cầu check-in
                var requestDto = new { RoomId = roomId };
                var jsonBody = JsonSerializer.Serialize(requestDto);
                Console.WriteLine($"Sending POST to api/CheckIn/request-checkin with body: {jsonBody}");
                
                var content = new StringContent(
                    jsonBody,
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync("api/CheckIn/request-checkin", content);
                Console.WriteLine($"Response status: {response.StatusCode}");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Yêu cầu check-in đã được gửi! Vui lòng chờ Staff/Admin phê duyệt.";
                    return RedirectToPage("/Resident/CheckInStatus");
                }
                else
                {
                    // Log chi tiết lỗi
                    Console.WriteLine($"API Error Status: {response.StatusCode}");
                    Console.WriteLine($"API Error Content: {responseContent}");
                    
                    try
                    {
                        var error = JsonSerializer.Deserialize<ApiErrorResponse>(responseContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        TempData["ErrorMessage"] = error?.Message ?? $"Không thể gửi yêu cầu check-in. Status: {response.StatusCode}. Response: {responseContent}";
                    }
                    catch
                    {
                        TempData["ErrorMessage"] = $"Lỗi API: {response.StatusCode}. Chi tiết: {responseContent}";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }

            return RedirectToPage();
        }
    }

    public class RoomDto
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Floor { get; set; }
        public decimal MonthlyRent { get; set; }
        public decimal Area { get; set; }
        public bool IsOccupied { get; set; }
        public bool IsAvailable { get; set; }
        public string? CurrentResident { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
