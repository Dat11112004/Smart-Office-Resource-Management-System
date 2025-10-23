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
                // Tạo HttpClient từ factory với cấu hình đã setup
                var httpClient = _httpClientFactory.CreateClient("API");
                
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
                    // Fallback to mock data if API fails
                    Residents = GetMockResidents();
                }

                // Calculate statistics
                TotalResidents = Residents.Count;
                ActiveResidents = Residents.Count(r => r.IsActive);
                PendingResidents = Residents.Count(r => !r.IsActive);
                NewThisMonth = Residents.Count(r => r.CheckInDate >= DateTime.Now.AddDays(-30));
            }
            catch (Exception ex)
            {
                // Handle error - use mock data as fallback
                Console.WriteLine($"Error loading residents: {ex.Message}");
                Residents = GetMockResidents();
                
                // Calculate statistics
                TotalResidents = Residents.Count;
                ActiveResidents = Residents.Count(r => r.IsActive);
                PendingResidents = Residents.Count(r => !r.IsActive);
                NewThisMonth = Residents.Count(r => r.CheckInDate >= DateTime.Now.AddDays(-30));
            }
        }

        private List<ResidentDto> GetMockResidents()
        {
            return new List<ResidentDto>
            {
                new ResidentDto
                {
                    Id = 1,
                    FullName = "Nguyễn Văn An",
                    Email = "nguyenvanan@email.com",
                    PhoneNumber = "0123456789",
                    RoomNumber = "101",
                    CheckInDate = DateTime.Now.AddDays(-30),
                    IsActive = true
                },
                new ResidentDto
                {
                    Id = 2,
                    FullName = "Trần Thị Bình",
                    Email = "tranthibinh@email.com",
                    PhoneNumber = "0987654321",
                    RoomNumber = "102",
                    CheckInDate = DateTime.Now.AddDays(-15),
                    IsActive = true
                },
                new ResidentDto
                {
                    Id = 3,
                    FullName = "Lê Văn Cường",
                    Email = "levancuong@email.com",
                    PhoneNumber = "0369852147",
                    RoomNumber = "103",
                    CheckInDate = DateTime.Now.AddDays(-7),
                    IsActive = false
                },
                new ResidentDto
                {
                    Id = 4,
                    FullName = "Phạm Thị Dung",
                    Email = "phamthidung@email.com",
                    PhoneNumber = "0741258963",
                    RoomNumber = "201",
                    CheckInDate = DateTime.Now.AddDays(-45),
                    IsActive = true
                },
                new ResidentDto
                {
                    Id = 5,
                    FullName = "Hoàng Văn Em",
                    Email = "hoangvanem@email.com",
                    PhoneNumber = "0852147963",
                    RoomNumber = "202",
                    CheckInDate = DateTime.Now.AddDays(-20),
                    IsActive = true
                }
            };
        }
    }
}
