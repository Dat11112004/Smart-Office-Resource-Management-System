using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using SORMS.API.DTOs;
using System.IdentityModel.Tokens.Jwt;

namespace SORMS.FE.Pages.Resident
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

        // Resident Info
        public ResidentDto ResidentInfo { get; set; }
        public RoomDto RoomInfo { get; set; }

        // Statistics
        public int TotalServiceRequests { get; set; }
        public int PendingServiceRequests { get; set; }
        public int CompletedServiceRequests { get; set; }
        public int TotalReports { get; set; }
        public int PendingReports { get; set; }
        public int TotalNotifications { get; set; }
        public int UnreadNotifications { get; set; }

        // Lists
        public List<ServiceRequestDto> RecentServiceRequests { get; set; } = new List<ServiceRequestDto>();
        public List<ReportDto> RecentReports { get; set; } = new List<ReportDto>();
        public List<NotificationDto> RecentNotifications { get; set; } = new List<NotificationDto>();

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if user is logged in
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }

            // Check if user is Resident
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Resident")
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
                // Get ResidentId from token
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var residentIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "ResidentId");
                
                if (residentIdClaim != null && int.TryParse(residentIdClaim.Value, out int residentId))
                {
                    // Get Resident Info - Use my-profile endpoint instead of /{id} to avoid 403 Forbidden
                    var residentResponse = await httpClient.GetAsync("/api/resident/my-profile");
                    if (residentResponse.IsSuccessStatusCode)
                    {
                        var residentJson = await residentResponse.Content.ReadAsStringAsync();
                        ResidentInfo = JsonSerializer.Deserialize<ResidentDto>(residentJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        Console.WriteLine($"DEBUG: ResidentInfo loaded - ID: {ResidentInfo?.Id}, RoomId: {ResidentInfo?.RoomId}, RoomNumber: {ResidentInfo?.RoomNumber}");

                        // Get Room Info if resident has a room
                        if (ResidentInfo?.RoomId != null && ResidentInfo.RoomId > 0)
                        {
                            var roomResponse = await httpClient.GetAsync($"/api/room/{ResidentInfo.RoomId}");
                            if (roomResponse.IsSuccessStatusCode)
                            {
                                var roomJson = await roomResponse.Content.ReadAsStringAsync();
                                RoomInfo = JsonSerializer.Deserialize<RoomDto>(roomJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                                Console.WriteLine($"DEBUG: RoomInfo loaded - RoomNumber: {RoomInfo?.RoomNumber}");
                            }
                            else
                            {
                                Console.WriteLine($"DEBUG: Failed to load room info - Status: {roomResponse.StatusCode}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"DEBUG: Resident has no room assigned (RoomId is null or 0)");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"DEBUG: Failed to load resident info - Status: {residentResponse.StatusCode}");
                    }

                    // Get Service Requests for this resident
                    var serviceRequestsResponse = await httpClient.GetAsync($"/api/servicerequest/resident/{residentId}");
                    if (serviceRequestsResponse.IsSuccessStatusCode)
                    {
                        var serviceRequestsJson = await serviceRequestsResponse.Content.ReadAsStringAsync();
                        var serviceRequests = JsonSerializer.Deserialize<List<ServiceRequestDto>>(serviceRequestsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        TotalServiceRequests = serviceRequests?.Count ?? 0;
                        PendingServiceRequests = serviceRequests?.Count(s => s.Status == "Pending") ?? 0;
                        CompletedServiceRequests = serviceRequests?.Count(s => s.Status == "Completed") ?? 0;
                        RecentServiceRequests = serviceRequests?.OrderByDescending(s => s.RequestDate).Take(5).ToList() ?? new List<ServiceRequestDto>();
                    }

                    // Get Reports for this resident
                    var reportsResponse = await httpClient.GetAsync($"/api/report/resident/{residentId}");
                    if (reportsResponse.IsSuccessStatusCode)
                    {
                        var reportsJson = await reportsResponse.Content.ReadAsStringAsync();
                        var reports = JsonSerializer.Deserialize<List<ReportDto>>(reportsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        TotalReports = reports?.Count ?? 0;
                        PendingReports = reports?.Count(r => r.Status == "Pending") ?? 0;
                        RecentReports = reports?.OrderByDescending(r => r.GeneratedDate).Take(5).ToList() ?? new List<ReportDto>();
                    }

                    // Get Notifications for this resident
                    var notificationsResponse = await httpClient.GetAsync($"/api/notification/resident/{residentId}");
                    if (notificationsResponse.IsSuccessStatusCode)
                    {
                        var notificationsJson = await notificationsResponse.Content.ReadAsStringAsync();
                        var notifications = JsonSerializer.Deserialize<List<NotificationDto>>(notificationsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        TotalNotifications = notifications?.Count ?? 0;
                        UnreadNotifications = notifications?.Count(n => !n.IsRead) ?? 0;
                        RecentNotifications = notifications?.OrderByDescending(n => n.CreatedAt).Take(5).ToList() ?? new List<NotificationDto>();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading dashboard data: {ex.Message}");
            }
        }
    }
}
