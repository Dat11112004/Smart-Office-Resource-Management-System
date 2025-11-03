using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;

namespace SORMS.FE.Pages.Admin
{
    public class SettingsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SettingsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Account Settings
        [BindProperty]
        [Required(ErrorMessage = "Mật khẩu hiện tại là bắt buộc")]
        public string? CurrentPassword { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string? NewPassword { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string? ConfirmPassword { get; set; }

        [BindProperty]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }

            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                ErrorMessage = "Bạn không có quyền truy cập trang này.";
                return Page();
            }

            // Load current email
            Email = HttpContext.Session.GetString("UserEmail");

            return Page();
        }

        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }

            // Manual validation since we're using name attributes instead of asp-for
            if (string.IsNullOrWhiteSpace(CurrentPassword))
            {
                ErrorMessage = "Vui lòng nhập mật khẩu hiện tại.";
                Email = HttpContext.Session.GetString("UserEmail");
                return Page();
            }

            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                ErrorMessage = "Vui lòng nhập mật khẩu mới.";
                Email = HttpContext.Session.GetString("UserEmail");
                return Page();
            }

            if (NewPassword.Length < 6)
            {
                ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự.";
                Email = HttpContext.Session.GetString("UserEmail");
                return Page();
            }

            if (NewPassword != ConfirmPassword)
            {
                ErrorMessage = "Mật khẩu mới và xác nhận mật khẩu không khớp.";
                Email = HttpContext.Session.GetString("UserEmail");
                return Page();
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient("API");
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var requestData = new
                {
                    CurrentPassword,
                    NewPassword
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(requestData),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await httpClient.PostAsync("/api/auth/change-password", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "Đổi mật khẩu thành công!";
                    CurrentPassword = NewPassword = ConfirmPassword = null;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Đổi mật khẩu thất bại: {error}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Đã xảy ra lỗi: {ex.Message}";
            }

            Email = HttpContext.Session.GetString("UserEmail");
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateEmailAsync()
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }

            if (string.IsNullOrEmpty(Email))
            {
                ErrorMessage = "Email không được để trống.";
                return Page();
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient("API");
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var requestData = new { Email };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(requestData),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await httpClient.PutAsync("/api/auth/update-email", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "Cập nhật email thành công!";
                    HttpContext.Session.SetString("UserEmail", Email);
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Cập nhật email thất bại: {error}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Đã xảy ra lỗi: {ex.Message}";
            }

            return Page();
        }
    }
}
