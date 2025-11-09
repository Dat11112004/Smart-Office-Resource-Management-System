using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace SORMS.FE.Pages.ManagerAccount
{
    public class VerifyOtpModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public VerifyOtpModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Otp { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }

        public IActionResult OnGet(string? email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToPage("/ManagerAccount/ForgotPassword");
            }

            Email = email;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(Otp))
            {
                ErrorMessage = "Vui lòng nhập mã OTP.";
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient("API");

                var requestDto = new { Email = Email, Otp = Otp };
                var content = new StringContent(
                    JsonSerializer.Serialize(requestDto),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync("api/auth/verify-otp", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // OTP verified, redirect to reset password
                    return RedirectToPage("/ManagerAccount/ResetPassword", new { email = Email, otp = Otp });
                }
                else
                {
                    ErrorMessage = responseContent.Trim('"') ?? "OTP không hợp lệ hoặc đã hết hạn.";
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
