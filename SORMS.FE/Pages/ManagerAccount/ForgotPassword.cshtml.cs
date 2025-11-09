using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace SORMS.FE.Pages.ManagerAccount
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ForgotPasswordModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
            // Clear any session data
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Vui lòng nhập email hợp lệ.";
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient("API");
                
                var requestDto = new { Email = Email };
                var content = new StringContent(
                    JsonSerializer.Serialize(requestDto),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync("api/auth/forgot-password", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Redirect to Verify OTP page with email
                    return RedirectToPage("/ManagerAccount/VerifyOtp", new { email = Email });
                }
                else
                {
                    ErrorMessage = responseContent.Trim('"') ?? "Email không tồn tại trong hệ thống.";
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
