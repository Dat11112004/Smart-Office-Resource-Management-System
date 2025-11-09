using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SORMS.FE.Pages.Staff
{
    public class SendNotificationModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SendNotificationModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public string Message { get; set; } = string.Empty;

        [BindProperty]
        public int SelectedResidentId { get; set; }

        public List<ResidentItem> Residents { get; set; } = new List<ResidentItem>();
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadResidentsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(Message))
            {
                ErrorMessage = "Nội dung thông báo không được để trống.";
                await LoadResidentsAsync();
                return Page();
            }

            if (Message.Length > 500)
            {
                ErrorMessage = "Nội dung thông báo không được vượt quá 500 ký tự.";
                await LoadResidentsAsync();
                return Page();
            }

            if (SelectedResidentId <= 0)
            {
                ErrorMessage = "Vui lòng chọn cư dân để gửi thông báo.";
                await LoadResidentsAsync();
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
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var requestDto = new
                {
                    Message = Message,
                    ResidentId = SelectedResidentId
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(requestDto),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync("api/notification/individual", content);

                if (response.IsSuccessStatusCode)
                {
                    // Get resident name for success message
                    var residentName = Residents.FirstOrDefault(r => r.Id == SelectedResidentId)?.Name ?? "cư dân";
                    SuccessMessage = $"Thông báo đã được gửi thành công đến {residentName}!";
                    
                    // Reset form
                    Message = string.Empty;
                    SelectedResidentId = 0;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Account/Login");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Không thể gửi thông báo. Vui lòng thử lại sau.";
                }
            }
            catch (HttpRequestException)
            {
                ErrorMessage = "Không thể kết nối đến server. Vui lòng kiểm tra kết nối mạng.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Đã xảy ra lỗi: {ex.Message}";
            }

            await LoadResidentsAsync();
            return Page();
        }

        private async Task LoadResidentsAsync()
        {
            try
            {
                var token = HttpContext.Session.GetString("JWTToken");
                if (string.IsNullOrEmpty(token))
                {
                    return;
                }

                var client = _httpClientFactory.CreateClient("API");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync("api/resident");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var residents = JsonSerializer.Deserialize<List<ResidentItem>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<ResidentItem>();

                    // Sort by name
                    Residents = residents.OrderBy(r => r.Name).ToList();
                }
            }
            catch (Exception)
            {
                // If loading residents fails, just continue with empty list
                Residents = new List<ResidentItem>();
            }
        }

        public class ResidentItem
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string? PhoneNumber { get; set; }
            public string? RoomNumber { get; set; }
        }
    }
}
