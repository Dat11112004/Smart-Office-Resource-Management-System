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
                // Mock data for demonstration
                Rooms = new List<RoomDto>
                {
                    new RoomDto
                    {
                        Id = 1,
                        RoomNumber = "101",
                        Floor = 1,
                        RoomType = "Tiêu chuẩn",
                        MonthlyRent = 3500000,
                        Area = 25,
                        IsAvailable = false,
                        CurrentResident = "Nguyễn Văn An"
                    },
                    new RoomDto
                    {
                        Id = 2,
                        RoomNumber = "102",
                        Floor = 1,
                        RoomType = "Tiêu chuẩn",
                        MonthlyRent = 3500000,
                        Area = 25,
                        IsAvailable = true,
                        CurrentResident = null
                    },
                    new RoomDto
                    {
                        Id = 3,
                        RoomNumber = "103",
                        Floor = 1,
                        RoomType = "Deluxe",
                        MonthlyRent = 5000000,
                        Area = 35,
                        IsAvailable = false,
                        CurrentResident = "Trần Thị Bình"
                    },
                    new RoomDto
                    {
                        Id = 4,
                        RoomNumber = "201",
                        Floor = 2,
                        RoomType = "Suite",
                        MonthlyRent = 8000000,
                        Area = 50,
                        IsAvailable = true,
                        CurrentResident = null
                    },
                    new RoomDto
                    {
                        Id = 5,
                        RoomNumber = "202",
                        Floor = 2,
                        RoomType = "Deluxe",
                        MonthlyRent = 5000000,
                        Area = 35,
                        IsAvailable = false,
                        CurrentResident = "Lê Văn Cường"
                    },
                    new RoomDto
                    {
                        Id = 6,
                        RoomNumber = "203",
                        Floor = 2,
                        RoomType = "Tiêu chuẩn",
                        MonthlyRent = 3500000,
                        Area = 25,
                        IsAvailable = true,
                        CurrentResident = null
                    },
                    new RoomDto
                    {
                        Id = 7,
                        RoomNumber = "301",
                        Floor = 3,
                        RoomType = "Suite",
                        MonthlyRent = 8000000,
                        Area = 50,
                        IsAvailable = false,
                        CurrentResident = "Phạm Thị Dung"
                    },
                    new RoomDto
                    {
                        Id = 8,
                        RoomNumber = "302",
                        Floor = 3,
                        RoomType = "Deluxe",
                        MonthlyRent = 5000000,
                        Area = 35,
                        IsAvailable = true,
                        CurrentResident = null
                    }
                };

                // Calculate statistics
                TotalRooms = Rooms.Count;
                AvailableRooms = Rooms.Count(r => r.IsAvailable);
                OccupiedRooms = Rooms.Count(r => !r.IsAvailable);
                OccupancyRate = TotalRooms > 0 ? (int)((double)OccupiedRooms / TotalRooms * 100) : 0;
            }
            catch (Exception ex)
            {
                // Handle error
                Console.WriteLine($"Error loading rooms: {ex.Message}");
            }
        }
    }
}



