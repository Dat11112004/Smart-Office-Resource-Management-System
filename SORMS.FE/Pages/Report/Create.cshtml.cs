using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SORMS.API.DTOs;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SORMS.FE.Pages.Report
{
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(IHttpClientFactory httpClientFactory, ILogger<CreateModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [BindProperty]
        public CreateReportDto Report { get; set; } = new();

        public IActionResult OnGet()
        {
            var token = HttpContext.Session.GetString("JWTToken");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }

            if (userRole != "Staff")
            {
                TempData["ErrorMessage"] = "Chỉ Staff mới có thể tạo báo cáo";
                return RedirectToPage("Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var token = HttpContext.Session.GetString("JWTToken");
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToPage("/Auth/Login");
                }

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var jsonContent = JsonSerializer.Serialize(Report);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://localhost:7001/api/Report", httpContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Tạo báo cáo thành công! Chờ Admin phê duyệt.";
                    return RedirectToPage("Index");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Auth/Login");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error creating report: {errorContent}");
                    ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi tạo báo cáo");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception when creating report");
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi tạo báo cáo");
            }

            return Page();
        }
    }
}
