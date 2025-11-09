using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SORMS.FE.Pages.Resident
{
    public class NotificationsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public NotificationsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<NotificationItem> Notifications { get; set; } = new List<NotificationItem>();
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var token = HttpContext.Session.GetString("JWTToken");
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToPage("/Account/Login");
                }

                var client = _httpClientFactory.CreateClient("API");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync("api/notification/my-notifications");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Notifications = JsonSerializer.Deserialize<List<NotificationItem>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<NotificationItem>();

                    // Sort by date descending (newest first)
                    Notifications = Notifications.OrderByDescending(n => n.CreatedAt).ToList();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Account/Login");
                }
                else
                {
                    ErrorMessage = "Không thể tải danh sách thông báo. Vui lòng thử lại sau.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Đã xảy ra lỗi: {ex.Message}";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostMarkAsReadAsync(int notificationId)
        {
            try
            {
                var token = HttpContext.Session.GetString("JWTToken");
                if (string.IsNullOrEmpty(token))
                {
                    return new JsonResult(new { success = false, message = "Phiên đăng nhập hết hạn" });
                }

                var client = _httpClientFactory.CreateClient("API");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.PutAsync($"api/notification/{notificationId}/read", null);

                if (response.IsSuccessStatusCode)
                {
                    return new JsonResult(new { success = true });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new JsonResult(new { 
                        success = false, 
                        message = $"Không thể đánh dấu đã đọc. Status: {response.StatusCode}, Error: {errorContent}" 
                    });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public class NotificationItem
        {
            public int Id { get; set; }
            public string Message { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
            public bool IsRead { get; set; }
            public string Type { get; set; } = "Individual";
            public string? TargetRole { get; set; }
        }
    }
}
