using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SORMS.API.DTOs;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SORMS.FE.Pages.Report
{
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<EditModel> _logger;

        public EditModel(IHttpClientFactory httpClientFactory, ILogger<EditModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [BindProperty]
        public UpdateReportDto Report { get; set; } = new();

        public int ReportId { get; set; }
        public string? ReportStatus { get; set; }

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
                    TempData["ErrorMessage"] = "Chỉ Staff mới có thể chỉnh sửa báo cáo";
                    return RedirectToPage("Index");
                }

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync($"https://localhost:7001/api/Report/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var reportDto = JsonSerializer.Deserialize<ReportDto>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (reportDto != null)
                    {
                        ReportId = reportDto.Id;
                        ReportStatus = reportDto.Status;

                        if (reportDto.Status != "Pending")
                        {
                            TempData["ErrorMessage"] = "Chỉ có thể chỉnh sửa báo cáo đang chờ duyệt";
                            return RedirectToPage("Index");
                        }

                        Report = new UpdateReportDto
                        {
                            Title = reportDto.Title,
                            Content = reportDto.Content
                        };
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
                _logger.LogError(ex, "Error loading report for edit");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải báo cáo";
                return RedirectToPage("Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                ReportId = id;
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

                var response = await client.PutAsync($"https://localhost:7001/api/Report/{id}", httpContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Cập nhật báo cáo thành công";
                    return RedirectToPage("Index");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Auth/Login");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error updating report: {errorContent}");
                    ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi cập nhật báo cáo");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception when updating report");
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi cập nhật báo cáo");
            }

            ReportId = id;
            return Page();
        }
    }
}
