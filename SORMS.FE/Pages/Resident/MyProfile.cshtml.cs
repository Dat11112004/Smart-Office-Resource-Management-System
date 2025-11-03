using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using SORMS.API.DTOs;

namespace SORMS.FE.Pages.Resident
{
    public class MyProfileModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public MyProfileModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public ResidentDto? Profile { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Kiểm tra role
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Resident")
            {
                return RedirectToPage("/AccessDenied");
            }

            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient("API");
                httpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", token);

                // Gọi API my-profile
                var response = await httpClient.GetAsync("/api/Resident/my-profile");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Profile = JsonSerializer.Deserialize<ResidentDto>(json, new JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true 
                    });

                    return Page();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    ErrorMessage = "Chưa có hồ sơ cư dân. Vui lòng liên hệ Admin để được hỗ trợ.";
                    return Page();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Không thể tải hồ sơ: {errorContent}";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception loading profile: {ex.Message}");
                ErrorMessage = $"Lỗi: {ex.Message}";
                return Page();
            }
        }
    }
}
