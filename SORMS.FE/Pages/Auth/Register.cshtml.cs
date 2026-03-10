using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SORMS.API.DTOs;
using System.Text;
using System.Text.Json;

namespace SORMS.FE.Pages.Auth
{
    public class RegisterModel : PageModel
    {
        // Bind từng property riêng lẻ thay vì bind cả object
        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public int RoleId { get; set; } = 3; // Default là Resident
        
        // 🔥 NEW: Thông tin bổ sung cho Resident profile
        [BindProperty]
        public string? FullName { get; set; }
        
        [BindProperty]
        public string? Phone { get; set; }
        
        [BindProperty]
        public string? IdentityNumber { get; set; }
        
        [BindProperty]
        public string? Address { get; set; }
        
        [BindProperty]
        public string? EmergencyContact { get; set; }

        public string? Message { get; set; }
        public string? Token { get; set; }
        public bool IsSuccess { get; set; } = false;

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public RegisterModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public void OnGet()
        {
            // Không cần khởi tạo RegisterDto nữa
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Console.WriteLine("=== OnPostAsync được gọi ===");
            
            // Debug: Log tất cả form data
            Console.WriteLine("=== Request.Form Data ===");
            foreach (var key in Request.Form.Keys)
            {
                Console.WriteLine($"{key}: '{Request.Form[key]}'");
            }
            
            Console.WriteLine($"Email: '{Email}'");
            Console.WriteLine($"Username: '{Username}'");
            Console.WriteLine($"Password length: {Password?.Length ?? 0}");
            Console.WriteLine($"RoleId: {RoleId}");
            
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                Message = "Dữ liệu không hợp lệ: " + string.Join(", ", errors);
                Console.WriteLine($"ModelState Errors: {Message}");
                return Page();
            }

            // Validate thủ công
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                Message = "Vui lòng điền đầy đủ thông tin!";
                return Page();
            }

            if (RoleId < 1 || RoleId > 3)
            {
                Message = "Role ID không hợp lệ!";
                return Page();
            }

            try
            {
                Console.WriteLine("=== Bắt đầu gọi API ===");
                
                // Tạo RegisterDto từ các properties đã bind
                var registerDto = new RegisterDto
                {
                    Email = Email,
                    UserName = Username,
                    Password = Password,
                    RoleId = RoleId,
                    FullName = FullName,
                    Phone = Phone,
                    IdentityNumber = IdentityNumber,
                    Address = Address,
                    EmergencyContact = EmergencyContact
                };
                
                var httpClient = _httpClientFactory.CreateClient("API");
                
                var json = JsonSerializer.Serialize(registerDto);
                Console.WriteLine($"JSON Body: {json}");
                
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("/api/auth/register", content);
                
                Console.WriteLine($"API Response Status: {response.StatusCode}");

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

                        // Lưu token vào session
                        HttpContext.Session.SetString("JWTToken", result.Token);
                        
                        // Lưu thông tin user vào session (giống Login)
                        if (!string.IsNullOrEmpty(result.UserRole))
                        {
                            HttpContext.Session.SetString("UserRole", result.UserRole);
                            Console.WriteLine($"UserRole saved to session: {result.UserRole}");
                        }
                        
                        if (result.UserId.HasValue)
                        {
                            HttpContext.Session.SetInt32("UserId", result.UserId.Value);
                            Console.WriteLine($"UserId saved to session: {result.UserId.Value}");
                        }
                        
                        if (!string.IsNullOrEmpty(result.Username))
                        {
                            HttpContext.Session.SetString("Username", result.Username);
                        }
                        
                        Console.WriteLine($"Register successful - redirecting to Index with role: {result.UserRole}");
                        
                        return RedirectToPage("/Index");
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Message = $"Lỗi đăng ký: {errorContent}";
                    Console.WriteLine($"API Error: {Message}");
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

    public class RegisterResponse
    {
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public string? UserRole { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
    }
}
