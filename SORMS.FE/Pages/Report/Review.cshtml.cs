using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SORMS.API.DTOs;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SORMS.FE.Pages.Report
{
    public class ReviewModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ReviewModel> _logger;

        public ReviewModel(IHttpClientFactory httpClientFactory, ILogger<ReviewModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public ReportDto Report { get; set; } = new();

        [BindProperty]
        public ReviewReportDto Review { get; set; } = new();

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

                if (userRole != "Admin")
                {
                    TempData["ErrorMessage"] = "Chỉ Admin mới có thể duyệt báo cáo";
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
                            TempData["ErrorMessage"] = "Báo cáo này đã được xử lý";
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
                    TempData["ErrorMessage"] = "Không tìm thấy báo cáo";
                    return RedirectToPage("Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading report for review");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải báo cáo";
                return RedirectToPage("Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id, string action)
        {
            _logger.LogInformation($"=== OnPostAsync Review called === ID: {id}, Action: {action}");
            
            // Set status TRƯỚC KHI validate
            Review.Status = action == "approve" ? "Reviewed" : "Rejected";
            
            // Remove validation error for Status vì nó được set từ code
            ModelState.Remove("Review.Status");
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid");
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        _logger.LogError($"ModelState Error: {error.ErrorMessage}");
                    }
                }
                // Reload report data
                await OnGetAsync(id);
                return Page();
            }

            try
            {
                var token = HttpContext.Session.GetString("JWTToken");
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("No JWT token found");
                    return RedirectToPage("/Auth/Login");
                }

                _logger.LogInformation($"AdminFeedback: {Review.AdminFeedback}");
                _logger.LogInformation($"Review DTO - Status: {Review.Status}, Feedback: {Review.AdminFeedback}");

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var jsonContent = JsonSerializer.Serialize(Review);
                _logger.LogInformation($"JSON to send: {jsonContent}");
                
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"https://localhost:7001/api/Report/{id}/review", httpContent);

                _logger.LogInformation($"API Response: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = action == "approve" 
                        ? "Duyệt báo cáo thành công" 
                        : "Từ chối báo cáo thành công";
                    return RedirectToPage("Index");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Auth/Login");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error reviewing report: {errorContent}");
                    ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi xử lý báo cáo");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception when reviewing report");
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi xử lý báo cáo");
            }

            // Reload report data on error
            await OnGetAsync(id);
            return Page();
        }
    }
}
