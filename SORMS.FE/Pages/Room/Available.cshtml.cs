using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SORMS.API.DTOs;
using System.Text.Json;

namespace SORMS.FE.Pages.Room
{
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
    }
}
