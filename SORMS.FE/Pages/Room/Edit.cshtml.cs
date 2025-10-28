using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SORMS.API.DTOs;
using System.Text.Json;

namespace SORMS.FE.Pages.Room
{
    public class EditModel : PageModel
    {
        [BindProperty]
        public int Id { get; set; }
        
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

        public EditModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        
        public async Task<IActionResult> OnGetAsync(int id)
        {
            Console.WriteLine($"=== Edit Room OnGetAsync được gọi với ID: {id} ===");
            
            var client = _httpClientFactory.CreateClient("API");
            var token = HttpContext.Session.GetString("JWTToken");
            
            Console.WriteLine($"Token: {(string.IsNullOrEmpty(token) ? "NULL" : "OK")}");
            
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }
            
            client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            var response = await client.GetAsync($"/api/Room/{id}");
            
            Console.WriteLine($"API Response: {response.StatusCode}");
            
            if (!response.IsSuccessStatusCode)
            {
                return RedirectToPage("/Room/Index");
            }
            
            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"JSON Response: {json}");
            
            var room = JsonSerializer.Deserialize<RoomDto>(json, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
            
            if (room != null)
            {
                Id = room.Id;
                Floor = room.Floor;
                RoomNumber = room.RoomNumber;
                Type = room.Type;
                MonthlyRent = room.MonthlyRent;
                Area = room.Area;
                IsAvailable = room.IsAvailable;
                Description = room.Description;
                
                Console.WriteLine($"Loaded - Floor: {Floor}, RoomNumber: {RoomNumber}, MonthlyRent: {MonthlyRent}, Area: {Area}");
            }
            
            return Page();
        }
        
        public async Task<IActionResult> OnPostAsync()
        {
            Console.WriteLine("=== Edit Room OnPostAsync được gọi ===");
            Console.WriteLine($"Id: {Id}");
            Console.WriteLine($"Floor: {Floor}");
            Console.WriteLine($"RoomNumber: '{RoomNumber}'");
            Console.WriteLine($"Type: '{Type}'");
            Console.WriteLine($"MonthlyRent: {MonthlyRent}");
            Console.WriteLine($"Area: {Area}");
            Console.WriteLine($"IsAvailable: {IsAvailable}");
            Console.WriteLine($"Description: '{Description}'");
            
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"ModelState Error: {error.ErrorMessage}");
                }
                return Page();
            }
            
            // Tạo RoomDto để gửi lên API
            var roomDto = new RoomDto
            {
                Id = Id,
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
            
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }
            
            client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            var json = JsonSerializer.Serialize(roomDto);
            Console.WriteLine($"JSON Body: {json}");
            
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"/api/Room/{Id}", content);
            
            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("Index");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error: {errorContent}");
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi cập nhật phòng. " + errorContent);
                return Page();
            }
        }
    }
}
