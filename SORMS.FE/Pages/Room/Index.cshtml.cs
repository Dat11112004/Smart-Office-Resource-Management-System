using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SORMS.API.DTOs;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SORMS.FE.Pages.Room
{
    public class IndexModel : PageModel
    {
        public List<RoomDto> Rooms { get; set; } = new List<RoomDto>();
        public int TotalRooms { get; set; } = 0;
        public int AvailableRooms { get; set; } = 0;
        public int OccupiedRooms { get; set; } = 0;
        public int OccupancyRate { get; set; } = 0;

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public IndexModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task OnGetAsync()
        {
            try
            {
                Console.WriteLine("=== Room Index OnGetAsync được gọi ===");
                
                var client = _httpClientFactory.CreateClient("API");
                var token = HttpContext.Session.GetString("JWTToken");
                
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                
                var response = await client.GetAsync("/api/Room");
                
                Console.WriteLine($"API Response Status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Response: {jsonString}");
                    
                    Rooms = JsonSerializer.Deserialize<List<RoomDto>>(jsonString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<RoomDto>();
                    
                    Console.WriteLine($"Loaded {Rooms.Count} rooms from API");
                }
                else
                {
                    Console.WriteLine($"Failed to load rooms: {response.StatusCode}");
                    Rooms = new List<RoomDto>();
                }

                // Calculate statistics
                TotalRooms = Rooms.Count;
                AvailableRooms = Rooms.Count(r => r.IsAvailable);
                OccupiedRooms = Rooms.Count(r => !r.IsAvailable);
                OccupancyRate = TotalRooms > 0 ? (int)((double)OccupiedRooms / TotalRooms * 100) : 0;
                
                Console.WriteLine($"Stats - Total: {TotalRooms}, Available: {AvailableRooms}, Occupied: {OccupiedRooms}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading rooms: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Rooms = new List<RoomDto>();
            }
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
                    return RedirectToPage();
                }
                else
                {
                    // Parse error message
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                        var errorMsg = errorResponse.GetProperty("message").GetString();
                        TempData["ErrorMessage"] = errorMsg ?? "Không thể gửi yêu cầu check-in";
                    }
                    catch
                    {
                        TempData["ErrorMessage"] = "Không thể gửi yêu cầu check-in";
                    }
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



