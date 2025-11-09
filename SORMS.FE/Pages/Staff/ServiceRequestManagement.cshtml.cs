using Microsoft.AspNetCore.Mvc;
using SORMS.FE.Pages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SORMS.FE.Pages.Staff
{
    public class ServiceRequestManagementModel : BasePageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ServiceRequestManagementModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<ServiceRequestDto> AllRequests { get; set; } = new();
        public List<ServiceRequestDto> PendingRequests { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            try
            {
                var client = _httpClientFactory.CreateClient("API");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Lấy tất cả yêu cầu
                var allResponse = await client.GetAsync("api/ServiceRequest");
                if (allResponse.IsSuccessStatusCode)
                {
                    var json = await allResponse.Content.ReadAsStringAsync();
                    AllRequests = JsonSerializer.Deserialize<List<ServiceRequestDto>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<ServiceRequestDto>();
                }

                // Lấy yêu cầu chờ xử lý
                var pendingResponse = await client.GetAsync("api/ServiceRequest/pending");
                if (pendingResponse.IsSuccessStatusCode)
                {
                    var json = await pendingResponse.Content.ReadAsStringAsync();
                    PendingRequests = JsonSerializer.Deserialize<List<ServiceRequestDto>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<ServiceRequestDto>();
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải dữ liệu: " + ex.Message;
            }

            return Page();
        }
    }

    public class ServiceRequestDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ResidentId { get; set; }
        public string? ResidentName { get; set; }
        public string? StaffFeedback { get; set; }
        public string? ReviewedBy { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Priority { get; set; } = "Normal";
    }
}
