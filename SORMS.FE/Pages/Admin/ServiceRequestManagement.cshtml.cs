using Microsoft.AspNetCore.Mvc;
using SORMS.FE.Pages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SORMS.FE.Pages.Admin
{
    public class ServiceRequestManagementModel : BasePageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ServiceRequestManagementModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<Staff.ServiceRequestDto> AllRequests { get; set; } = new();
        public List<Staff.ServiceRequestDto> PendingRequests { get; set; } = new();

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
                    AllRequests = JsonSerializer.Deserialize<List<Staff.ServiceRequestDto>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<Staff.ServiceRequestDto>();
                }

                // Lấy yêu cầu chờ xử lý
                var pendingResponse = await client.GetAsync("api/ServiceRequest/pending");
                if (pendingResponse.IsSuccessStatusCode)
                {
                    var json = await pendingResponse.Content.ReadAsStringAsync();
                    PendingRequests = JsonSerializer.Deserialize<List<Staff.ServiceRequestDto>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<Staff.ServiceRequestDto>();
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải dữ liệu: " + ex.Message;
            }

            return Page();
        }
    }
}
