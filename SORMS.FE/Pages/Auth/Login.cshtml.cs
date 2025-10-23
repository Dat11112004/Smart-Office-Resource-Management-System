using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SORMS.API.DTOs;
using System.Text;
using System.Text.Json;

namespace SORMS.FE.Pages.Auth
{
    public class LoginModel : PageModel
    {
        // Bind từng property riêng lẻ để tránh lỗi model binding
        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public string? Message { get; set; }
        public bool IsSuccess { get; set; } = false;

        private readonly IHttpClientFactory _httpClientFactory;

        public LoginModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult OnGet()
        {
            Console.WriteLine("=== Login Page OnGet được gọi ===");
            try
            {
                return Page();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in OnGet: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Console.WriteLine("=== OnPostAsync Login được gọi ===");
            Console.WriteLine($"Email: '{Email}'");
            Console.WriteLine($"Password length: {Password?.Length ?? 0}");
            
            // Validate thủ công
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                Message = "Vui lòng điền đầy đủ Email và Mật khẩu!";
                return Page();
            }

            try
            {
                Console.WriteLine("=== Bắt đầu gọi API Login ===");
                
                // Tạo LoginDto từ các properties đã bind
                var loginDto = new LoginDto
                {
                    Email = Email,
                    Password = Password
                };
                
                var httpClient = _httpClientFactory.CreateClient("API");
                
                var json = JsonSerializer.Serialize(loginDto);
                Console.WriteLine($"JSON Body: {json}");
                
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("/api/auth/login", content);
                
                Console.WriteLine($"API Response Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result != null && !string.IsNullOrEmpty(result.Token))
                    {
                        IsSuccess = true;
                        Message = result.Message ?? "Đăng nhập thành công!";

                        // Lưu token vào session
                        HttpContext.Session.SetString("JWTToken", result.Token);
                        
                        Console.WriteLine($"Token saved to session: {result.Token.Substring(0, 20)}...");
                        
                        // Redirect đến trang chính
                        return RedirectToPage("/Index");
                    }
                    else
                    {
                        Message = "Đăng nhập thất bại: Không nhận được token từ server.";
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Message = $"Đăng nhập thất bại: Email hoặc mật khẩu không đúng.";
                    Console.WriteLine($"API Error: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Message = $"Có lỗi xảy ra: {ex.Message}";
                Console.WriteLine($"Exception: {ex}");
            }

            return Page();
        }
    }

    public class LoginResponse
    {
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
