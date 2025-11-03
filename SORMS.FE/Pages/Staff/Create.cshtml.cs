using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SORMS.API.DTOs;
using System.Text;
using System.Text.Json;

namespace SORMS.FE.Pages.Staff
{
    public class CreateModel : PageModel
    {
        [BindProperty]
        public StaffDto Staff { get; set; } = new StaffDto();
        public string? ErrorMessage { get; set; }

        private readonly IHttpClientFactory _httpClientFactory;

        public CreateModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult OnGet()
        {
            // Check if user is Admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                return RedirectToPage("/AccessDenied");
            }

            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Check if user is Admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                return RedirectToPage("/AccessDenied");
            }

            try
            {
                // Manual validation
                if (string.IsNullOrWhiteSpace(Staff.FullName))
                {
                    ErrorMessage = "Vui lòng nhập họ tên nhân viên.";
                    return Page();
                }

                if (string.IsNullOrWhiteSpace(Staff.Email))
                {
                    ErrorMessage = "Vui lòng nhập email.";
                    return Page();
                }

                if (!Staff.Email.Contains("@"))
                {
                    ErrorMessage = "Email không hợp lệ.";
                    return Page();
                }

                if (string.IsNullOrWhiteSpace(Staff.Phone))
                {
                    ErrorMessage = "Vui lòng nhập số điện thoại.";
                    return Page();
                }

                var token = HttpContext.Session.GetString("JWTToken");
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToPage("/Login");
                }

                var httpClient = _httpClientFactory.CreateClient("API");
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var json = JsonSerializer.Serialize(Staff);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("/api/staff", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Thêm nhân viên thành công!";
                    return RedirectToPage("/Staff/Index");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Không thể thêm nhân viên. Lỗi: {errorContent}";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi thêm nhân viên: {ex.Message}";
                return Page();
            }
        }
    }
}
