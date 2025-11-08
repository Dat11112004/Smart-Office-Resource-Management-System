using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SORMS.API.DTOs;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;

namespace SORMS.FE.Pages.Room
{
    public class ApiErrorResponse
    {
        public string Message { get; set; }
        public string Error { get; set; }
    }

    public class AvailableModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        
        public List<RoomDto> AvailableRooms { get; set; } = new List<RoomDto>();
        public int TotalAvailableRooms { get; set; }
        public decimal AverageRent { get; set; }
        public decimal LowestRent { get; set; }
        public decimal HighestRent { get; set; }
        
        public AvailableModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        
        public async Task<IActionResult> OnGetAsync()
        {
            Console.WriteLine("=== Available Rooms OnGetAsync được gọi ===");
            
            var client = _httpClientFactory.CreateClient("API");
            var token = HttpContext.Session.GetString("JWTToken");
            
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }
            
            client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            var response = await client.GetAsync("/api/Room");
            
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"API Error: {response.StatusCode}");
                return Page();
            }
            
            var json = await response.Content.ReadAsStringAsync();
            var allRooms = JsonSerializer.Deserialize<List<RoomDto>>(json, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
            
            // Lọc chỉ phòng trống
            AvailableRooms = allRooms?.Where(r => r.IsAvailable).ToList() ?? new List<RoomDto>();
            
            // Tính toán thống kê
            TotalAvailableRooms = AvailableRooms.Count;
            
            if (TotalAvailableRooms > 0)
            {
                AverageRent = AvailableRooms.Average(r => r.MonthlyRent);
                LowestRent = AvailableRooms.Min(r => r.MonthlyRent);
                HighestRent = AvailableRooms.Max(r => r.MonthlyRent);
            }
            
            Console.WriteLine($"Found {TotalAvailableRooms} available rooms");
            Console.WriteLine($"Average Rent: {AverageRent:N0} VNĐ");
            
            return Page();
        }

        public async Task<IActionResult> OnPostRequestCheckInAsync(int roomId)
        {
            Console.WriteLine($"=== OnPostRequestCheckInAsync được gọi với roomId: {roomId} ===");

            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("No token found, redirecting to login");
                return RedirectToPage("/Auth/Login");
            }

            // Kiểm tra role
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Resident")
            {
                TempData["ErrorMessage"] = "Chỉ Resident mới có thể yêu cầu thuê phòng!";
                return RedirectToPage();
            }

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
                Console.WriteLine($"Response content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Yêu cầu check-in đã được gửi! Vui lòng chờ Staff/Admin phê duyệt.";
                    return RedirectToPage("/Resident/CheckInStatus");
                }
                else
                {
                    var error = JsonSerializer.Deserialize<ApiErrorResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    TempData["ErrorMessage"] = error?.Message ?? "Không thể gửi yêu cầu check-in";
                    return RedirectToPage();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                TempData["ErrorMessage"] = "Lỗi khi gửi yêu cầu: " + ex.Message;
                return RedirectToPage();
            }
        }
    }
}
