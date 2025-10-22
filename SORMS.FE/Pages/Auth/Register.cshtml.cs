using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SORMS.API.DTOs;
using System.Text;
using System.Text.Json;

namespace SORMS.FE.Pages.Auth
{
    public class RegisterModel : PageModel
    {
        [BindProperty]
        public RegisterDto RegisterDto { get; set; } = new RegisterDto();

        public string? Message { get; set; }
        public string? Token { get; set; }
        public bool IsSuccess { get; set; } = false;

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public RegisterModel(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public void OnGet()
        {
            // Initialize the RegisterDto
            RegisterDto = new RegisterDto();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Gọi API đăng ký
                var apiUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001";
                var json = JsonSerializer.Serialize(RegisterDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{apiUrl}/api/auth/register", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<RegisterResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result != null && !string.IsNullOrEmpty(result.Token))
                    {
                        IsSuccess = true;
                        Message = result.Message ?? "Đăng ký thành công!";
                        Token = result.Token;

                        // Lưu token vào session hoặc cookie
                        HttpContext.Session.SetString("JWTToken", result.Token);
                        
                        // Redirect đến trang chính hoặc dashboard
                        return RedirectToPage("/Index");
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Message = $"Lỗi đăng ký: {errorContent}";
                }
            }
            catch (Exception ex)
            {
                Message = $"Có lỗi xảy ra: {ex.Message}";
            }

            return Page();
        }
    }

    public class RegisterResponse
    {
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
