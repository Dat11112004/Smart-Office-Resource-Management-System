using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using SORMS.API.DTOs;

namespace SORMS.FE.Pages.Admin
{
    public class DashboardModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DashboardModel> _logger;

        public DashboardModel(IHttpClientFactory httpClientFactory, ILogger<DashboardModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // Statistics
        public int TotalResidents { get; set; }
        public int TotalStaff { get; set; }
        public int TotalRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int PendingCheckIns { get; set; }
        public int PendingServiceRequests { get; set; }
        public int PendingReports { get; set; }

        // Lists
        public List<ResidentDto> RecentResidents { get; set; } = new List<ResidentDto>();
        public List<ServiceRequestDto> RecentServiceRequests { get; set; } = new List<ServiceRequestDto>();
        public List<ReportDto> RecentReports { get; set; } = new List<ReportDto>();

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if user is logged in
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }

            // Check if user is Admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                return RedirectToPage("/AccessDenied");
            }

            await LoadDashboardDataAsync(token);

            return Page();
        }

        private async Task LoadDashboardDataAsync(string token)
        {
            var httpClient = _httpClientFactory.CreateClient("API");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                // Get Residents
                    var residentsResponse = await httpClient.GetAsync("/api/resident");
                if (residentsResponse.IsSuccessStatusCode)
                {
                    var residentsJson = await residentsResponse.Content.ReadAsStringAsync();
                    var residents = JsonSerializer.Deserialize<List<ResidentDto>>(residentsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    TotalResidents = residents?.Count ?? 0;
                    RecentResidents = residents?.OrderByDescending(r => r.CheckInDate).Take(5).ToList() ?? new List<ResidentDto>();
                }                // Get Staff
                var staffResponse = await httpClient.GetAsync("/api/staff");
                if (staffResponse.IsSuccessStatusCode)
                {
                    var staffJson = await staffResponse.Content.ReadAsStringAsync();
                    var staff = JsonSerializer.Deserialize<List<StaffDto>>(staffJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    TotalStaff = staff?.Count ?? 0;
                }

                // Get Rooms
                var roomsResponse = await httpClient.GetAsync("/api/room");
                if (roomsResponse.IsSuccessStatusCode)
                {
                    var roomsJson = await roomsResponse.Content.ReadAsStringAsync();
                    var rooms = JsonSerializer.Deserialize<List<RoomDto>>(roomsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    TotalRooms = rooms?.Count ?? 0;
                    OccupiedRooms = rooms?.Count(r => r.IsOccupied) ?? 0;
                    AvailableRooms = TotalRooms - OccupiedRooms;
                }

                // Get Check-in Records
                var checkInsResponse = await httpClient.GetAsync("/api/checkin");
                if (checkInsResponse.IsSuccessStatusCode)
                {
                    var checkInsJson = await checkInsResponse.Content.ReadAsStringAsync();
                    var checkIns = JsonSerializer.Deserialize<List<CheckInRecordDto>>(checkInsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    PendingCheckIns = checkIns?.Count(c => c.Status == "Pending") ?? 0;
                }

                // Get Service Requests
                var serviceRequestsResponse = await httpClient.GetAsync("/api/servicerequest");
                if (serviceRequestsResponse.IsSuccessStatusCode)
                {
                    var serviceRequestsJson = await serviceRequestsResponse.Content.ReadAsStringAsync();
                    var serviceRequests = JsonSerializer.Deserialize<List<ServiceRequestDto>>(serviceRequestsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    PendingServiceRequests = serviceRequests?.Count(s => s.Status == "Pending") ?? 0;
                    RecentServiceRequests = serviceRequests?.OrderByDescending(s => s.RequestDate).Take(5).ToList() ?? new List<ServiceRequestDto>();
                }

                // Get Reports
                var reportsResponse = await httpClient.GetAsync("/api/report");
                if (reportsResponse.IsSuccessStatusCode)
                {
                    var reportsJson = await reportsResponse.Content.ReadAsStringAsync();
                    var reports = JsonSerializer.Deserialize<List<ReportDto>>(reportsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    PendingReports = reports?.Count(r => r.Status == "Pending") ?? 0;
                    RecentReports = reports?.OrderByDescending(r => r.GeneratedDate).Take(5).ToList() ?? new List<ReportDto>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading dashboard data: {ex.Message}");
            }
        }
    }
}
