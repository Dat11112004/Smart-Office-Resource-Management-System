using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using SORMS.API.DTOs;

namespace SORMS.FE.Pages.Staff
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
        public int TotalServiceRequests { get; set; }
        public int PendingServiceRequests { get; set; }
        public int CompletedServiceRequests { get; set; }
        public int InProgressServiceRequests { get; set; }
        public int TotalCheckIns { get; set; }
        public int PendingCheckIns { get; set; }
        public int TotalReports { get; set; }
        public int PendingReports { get; set; }

        // Lists
        public List<ServiceRequestDto> RecentServiceRequests { get; set; } = new List<ServiceRequestDto>();
        public List<CheckInRecordDto> RecentCheckIns { get; set; } = new List<CheckInRecordDto>();
        public List<ReportDto> RecentReports { get; set; } = new List<ReportDto>();

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if user is logged in
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }

            // Check if user is Staff
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Staff")
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
                // Get Service Requests
                var serviceRequestsResponse = await httpClient.GetAsync("/api/servicerequest");
                if (serviceRequestsResponse.IsSuccessStatusCode)
                {
                    var serviceRequestsJson = await serviceRequestsResponse.Content.ReadAsStringAsync();
                    var serviceRequests = JsonSerializer.Deserialize<List<ServiceRequestDto>>(serviceRequestsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    TotalServiceRequests = serviceRequests?.Count ?? 0;
                    PendingServiceRequests = serviceRequests?.Count(s => s.Status == "Pending") ?? 0;
                    CompletedServiceRequests = serviceRequests?.Count(s => s.Status == "Completed") ?? 0;
                    InProgressServiceRequests = serviceRequests?.Count(s => s.Status == "In Progress") ?? 0;
                    RecentServiceRequests = serviceRequests?.OrderByDescending(s => s.RequestDate).Take(10).ToList() ?? new List<ServiceRequestDto>();
                }

                // Get Check-in Records
                var checkInsResponse = await httpClient.GetAsync("/api/checkin");
                if (checkInsResponse.IsSuccessStatusCode)
                {
                    var checkInsJson = await checkInsResponse.Content.ReadAsStringAsync();
                    var checkIns = JsonSerializer.Deserialize<List<CheckInRecordDto>>(checkInsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    TotalCheckIns = checkIns?.Count ?? 0;
                    PendingCheckIns = checkIns?.Count(c => c.Status == "Pending") ?? 0;
                    RecentCheckIns = checkIns?.OrderByDescending(c => c.RequestTime).Take(5).ToList() ?? new List<CheckInRecordDto>();
                }

                // Get Reports
                var reportsResponse = await httpClient.GetAsync("/api/report");
                if (reportsResponse.IsSuccessStatusCode)
                {
                    var reportsJson = await reportsResponse.Content.ReadAsStringAsync();
                    var reports = JsonSerializer.Deserialize<List<ReportDto>>(reportsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    TotalReports = reports?.Count ?? 0;
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
