using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SORMS.API.DTOs;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SORMS.FE.Pages.Report
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IHttpClientFactory httpClientFactory, ILogger<IndexModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public List<ReportDto> Reports { get; set; } = new();
        public string? CurrentFilter { get; set; }
        public string? CurrentStatus { get; set; }
        public string? UserRole { get; set; }

        public async Task<IActionResult> OnGetAsync(string? searchString, string? status)
        {
            try
            {
                var token = HttpContext.Session.GetString("JWTToken");
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToPage("/Auth/Login");
                }

                UserRole = HttpContext.Session.GetString("UserRole");
                CurrentFilter = searchString;
                CurrentStatus = status;

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync("https://localhost:7001/api/Report");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var reports = JsonSerializer.Deserialize<List<ReportDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    Reports = reports ?? new List<ReportDto>();

                    // Filter by search string
                    if (!string.IsNullOrEmpty(searchString))
                    {
                        Reports = Reports.Where(r =>
                            r.Title.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                            r.Content.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                            r.CreatedBy.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                        ).ToList();
                    }

                    // Filter by status
                    if (!string.IsNullOrEmpty(status))
                    {
                        Reports = Reports.Where(r => r.Status == status).ToList();
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Auth/Login");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching reports");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách báo cáo";
            }

            return Page();
        }
    }
}
