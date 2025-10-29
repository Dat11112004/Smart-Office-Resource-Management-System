using Microsoft.AspNetCore.Mvc.RazorPages;
using SORMS.API.DTOs;
using System.Text;
using System.Text.Json;

namespace SORMS.FE.Pages.Resident
{
    public class IndexModel : PageModel
    {
        public List<ResidentDto> Residents { get; set; } = new List<ResidentDto>();
        public int TotalResidents { get; set; } = 0;
        public int ActiveResidents { get; set; } = 0;
        public int PendingResidents { get; set; } = 0;
        public int NewThisMonth { get; set; } = 0;

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
                // Lấy JWT token từ session
                var token = HttpContext.Session.GetString("JWTToken");
                
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("No JWT token found - user not authenticated");
                    Residents = new List<ResidentDto>();
                    return;
                }
                
                // Tạo HttpClient với Authorization header
                var httpClient = _httpClientFactory.CreateClient("API");
                httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
                // Gọi API để lấy danh sách residents
                var response = await httpClient.GetAsync("/api/resident");
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var residents = JsonSerializer.Deserialize<List<ResidentDto>>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    if (residents != null)
                    {
                        Residents = residents;
                    }
                }
                else
                {
                    Console.WriteLine($"API call failed with status: {response.StatusCode}");
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error: {errorContent}");
                    Residents = new List<ResidentDto>();
                }

                // Calculate statistics
                TotalResidents = Residents.Count;
                ActiveResidents = Residents.Count(r => r.IsActive == true);
                PendingResidents = Residents.Count(r => r.IsActive != true);
                NewThisMonth = Residents.Count(r => r.CheckInDate.HasValue && r.CheckInDate.Value >= DateTime.Now.AddDays(-30));
            }
            catch (Exception ex)
            {
                // Handle error - log and show empty list
                Console.WriteLine($"Error loading residents: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Residents = new List<ResidentDto>();
                
                // Calculate statistics (all zeros)
                TotalResidents = 0;
                ActiveResidents = 0;
                PendingResidents = 0;
                NewThisMonth = 0;
            }
        }
    }
}
