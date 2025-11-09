using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace SORMS.FE.Pages.ManagerAccount
{
    public class ResetPasswordModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ResetPasswordModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Otp { get; set; } = string.Empty;

        [BindProperty]
        public string NewPassword { get; set; } = string.Empty;

        [BindProperty]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }

        public IActionResult OnGet(string? email, string? otp)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otp))
            {
                return RedirectToPage("/ManagerAccount/ForgotPassword");
            }

            Email = email;
            Otp = otp;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Vui lòng điền đầy đủ thông tin.";
                return Page();
            }

            if (NewPassword != ConfirmPassword)
            {
                ErrorMessage = "Mật khẩu xác nhận không khớp!";
                return Page();
            }

            if (NewPassword.Length < 6)
            {
                ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.";
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient("API");

                var requestDto = new
                {
                    Email = Email,
                    Otp = Otp,
                    NewPassword = NewPassword
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(requestDto),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync("api/auth/reset-password", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Đặt lại mật khẩu thành công! Vui lòng đăng nhập.";
                    return RedirectToPage("/Auth/Login");
                }
                else
                {
                    ErrorMessage = responseContent.Trim('"') ?? "Không thể đặt lại mật khẩu. OTP sai hoặc đã hết hạn.";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Lỗi kết nối. Vui lòng thử lại sau.";
                return Page();
            }
        }
    }
}
