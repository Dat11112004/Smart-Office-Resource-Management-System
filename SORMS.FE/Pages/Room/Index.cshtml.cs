using Microsoft.AspNetCore.Mvc.RazorPages;
using SORMS.API.DTOs;
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
    }
}



