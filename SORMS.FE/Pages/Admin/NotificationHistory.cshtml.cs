using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SORMS.FE.Pages.Admin
{
    public class NotificationHistoryModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public NotificationHistoryModel(IHttpClientFactory httpClientFactory)
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

                var response = await client.GetAsync("api/notification/sent-history");

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
                    ErrorMessage = "Không thể tải lịch sử thông báo. Vui lòng thử lại sau.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Đã xảy ra lỗi: {ex.Message}";
            }

            return Page();
        }

        public class NotificationItem
        {
            public int Id { get; set; }
            public string Message { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
            public bool IsRead { get; set; }
            public string Type { get; set; } = "Individual";
            public string? TargetRole { get; set; }
            public int? ResidentId { get; set; }
            public int? StaffId { get; set; }
        }
    }
}
