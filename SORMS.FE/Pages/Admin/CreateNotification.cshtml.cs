using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace SORMS.FE.Pages.Admin
{
    public class CreateNotificationModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CreateNotificationModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public string Message { get; set; } = string.Empty;

        [BindProperty]
        public string TargetRole { get; set; } = "All";

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Message))
            {
                ErrorMessage = "Nội dung thông báo không được để trống.";
                return Page();
            }

            if (Message.Length > 500)
            {
                ErrorMessage = "Nội dung thông báo không được vượt quá 500 ký tự.";
                return Page();
            }

            try
            {
                var token = HttpContext.Session.GetString("JWTToken");
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToPage("/Account/Login");
                }

                var client = _httpClientFactory.CreateClient("API");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var requestDto = new
                {
                    Message = Message,
                    TargetRole = TargetRole
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(requestDto),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync("api/notification/broadcast", content);

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = $"Thông báo đã được gửi thành công đến {GetTargetRoleDisplay(TargetRole)}!";
                    // Reset form
                    Message = string.Empty;
                    TargetRole = "All";
                    return Page();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Không thể tạo thông báo: {errorContent}";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi kết nối: {ex.Message}";
                return Page();
            }
        }

        private string GetTargetRoleDisplay(string targetRole)
        {
            return targetRole switch
            {
                "All" => "tất cả người dùng",
                "Resident" => "tất cả cư dân",
                "Staff" => "tất cả nhân viên",
                _ => "người dùng"
            };
        }
    }
}
