using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SORMS.API.DTOs;
using System.Text.Json;

namespace SORMS.FE.Pages.Staff
{
    public class DetailsModel : PageModel
    {
        public StaffDto? Staff { get; set; }
        public string? ErrorMessage { get; set; }

        private readonly IHttpClientFactory _httpClientFactory;

        public DetailsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Check if user is Admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                return RedirectToPage("/AccessDenied");
            }

            try
            {
                var token = HttpContext.Session.GetString("JWTToken");
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToPage("/Login");
                }

                var httpClient = _httpClientFactory.CreateClient("API");
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await httpClient.GetAsync($"/api/staff/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    Staff = JsonSerializer.Deserialize<StaffDto>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (Staff == null)
                    {
                        ErrorMessage = "Không tìm thấy thông tin nhân viên.";
                    }
                }
                else
                {
                    ErrorMessage = $"Không thể tải thông tin nhân viên. Mã lỗi: {response.StatusCode}";
                }

                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi tải thông tin nhân viên: {ex.Message}";
                return Page();
            }
        }
    }
}
