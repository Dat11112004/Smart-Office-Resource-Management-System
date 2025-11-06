using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SORMS.API.DTOs;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SORMS.FE.Pages.Report
{
    public class DetailsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(IHttpClientFactory httpClientFactory, ILogger<DetailsModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public ReportDto Report { get; set; } = new();
        public string? UserRole { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                var token = HttpContext.Session.GetString("JWTToken");
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToPage("/Auth/Login");
                }

                UserRole = HttpContext.Session.GetString("UserRole");

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync($"https://localhost:7001/api/Report/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var report = JsonSerializer.Deserialize<ReportDto>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    Report = report ?? new ReportDto();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Auth/Login");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy báo cáo";
                    return RedirectToPage("Index");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền xem báo cáo này";
                    return RedirectToPage("Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching report details");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải chi tiết báo cáo";
                return RedirectToPage("Index");
            }

            return Page();
        }
    }
}
