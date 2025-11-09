using Microsoft.AspNetCore.Mvc;
using SORMS.FE.Pages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SORMS.FE.Pages.Resident
{
    public class ServiceRequestsModel : BasePageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ServiceRequestsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<ServiceRequestDto> MyRequests { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            try
            {
                var client = _httpClientFactory.CreateClient("API");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync("api/ServiceRequest/my-requests");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    MyRequests = JsonSerializer.Deserialize<List<ServiceRequestDto>>(json, new JsonSerializerOptions
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

        public async Task<IActionResult> OnPostCreateRequestAsync(string title, string serviceType, string description, string priority)
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            try
            {
                var client = _httpClientFactory.CreateClient("API");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var requestDto = new
                {
                    Title = title,
                    ServiceType = serviceType,
                    Description = description,
                    Priority = priority
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(requestDto),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync("api/ServiceRequest", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Tạo yêu cầu dịch vụ thành công!";
                }
                else
                {
                    var error = JsonSerializer.Deserialize<ApiErrorResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    TempData["ErrorMessage"] = error?.Message ?? "Không thể tạo yêu cầu";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteRequestAsync(int id)
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            try
            {
                var client = _httpClientFactory.CreateClient("API");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.DeleteAsync($"api/ServiceRequest/{id}");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Xóa yêu cầu thành công!";
                }
                else
                {
                    var error = JsonSerializer.Deserialize<ApiErrorResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    TempData["ErrorMessage"] = error?.Message ?? "Không thể xóa yêu cầu";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }

            return RedirectToPage();
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
