using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SORMS.API.DTOs;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SORMS.FE.Pages.Report
{
    public class DeleteModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(IHttpClientFactory httpClientFactory, ILogger<DeleteModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public ReportDto Report { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                var token = HttpContext.Session.GetString("JWTToken");
                var userRole = HttpContext.Session.GetString("UserRole");

                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToPage("/Auth/Login");
                }

                if (userRole != "Staff")
                {
                    TempData["ErrorMessage"] = "Chỉ Staff mới có thể xóa báo cáo";
                    return RedirectToPage("Index");
                }

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

                    if (report != null)
                    {
                        if (report.Status != "Pending")
                        {
                            TempData["ErrorMessage"] = "Chỉ có thể xóa báo cáo đang chờ duyệt";
                            return RedirectToPage("Index");
                        }

                        Report = report;
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Auth/Login");
                }
                else
                {
                    TempData["ErrorMessage"] = "Không tìm thấy báo cáo hoặc bạn không có quyền";
                    return RedirectToPage("Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading report for delete");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải báo cáo";
                return RedirectToPage("Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            try
            {
                var token = HttpContext.Session.GetString("JWTToken");
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToPage("/Auth/Login");
                }

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.DeleteAsync($"https://localhost:7001/api/Report/{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Xóa báo cáo thành công";
                    return RedirectToPage("Index");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Auth/Login");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error deleting report: {errorContent}");
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa báo cáo";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception when deleting report");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa báo cáo";
            }

            return RedirectToPage("Index");
        }
    }
}
