using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SORMS.API.DTOs;

namespace SORMS.FE.Pages.Room
{
    public class CreateModel : PageModel
    {
        // Bind từng property riêng lẻ thay vì nested object
        [BindProperty]
        public int Floor { get; set; } 
        [BindProperty]
        public string RoomNumber { get; set; } = string.Empty;
        
        [BindProperty]
        public string Type { get; set; } = string.Empty;
        
        [BindProperty]
        public decimal MonthlyRent { get; set; }
        
        [BindProperty]
        public decimal Area { get; set; }
        
        [BindProperty]
        public bool IsAvailable { get; set; }
        
        [BindProperty]
        public string? Description { get; set; }
        
        private readonly IHttpClientFactory _httpClientFactory;
        
        public CreateModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public void OnGet()
        {
            Console.WriteLine("=== Create Room OnGet được gọi ===");
        }
        
        public async Task<IActionResult> OnPostAsync()
        {
            Console.WriteLine("=== Create Room OnPostAsync được gọi ===");
            Console.WriteLine($"Floor: {Floor}");
            Console.WriteLine($"RoomNumber: '{RoomNumber}'");
            Console.WriteLine($"Type: '{Type}'");
            Console.WriteLine($"MonthlyRent: {MonthlyRent}");
            Console.WriteLine($"Area: {Area}");
            Console.WriteLine($"IsAvailable: {IsAvailable}");
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"ModelState Error: {error.ErrorMessage}");
                }
                return Page();
            }
            
            // Tạo RoomDto từ các properties đã bind
            var roomDto = new RoomDto
            {
                Floor = Floor,
                RoomNumber = RoomNumber,
                Type = Type,
                MonthlyRent = MonthlyRent,
                Area = Area,
                IsAvailable = IsAvailable,
                IsOccupied = !IsAvailable,
                IsActive = true,
                Description = Description
            };
            
            var client = _httpClientFactory.CreateClient("API");
            var token = HttpContext.Session.GetString("JWTToken");
            if (!string.IsNullOrEmpty(token))
            { 
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            var json = System.Text.Json.JsonSerializer.Serialize(roomDto);
            Console.WriteLine($"JSON Body: {json}");
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/api/Room", content);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("Index");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi tạo phòng." + await response.Content.ReadAsStringAsync());
                return Page();
            }

        }
    }
}
