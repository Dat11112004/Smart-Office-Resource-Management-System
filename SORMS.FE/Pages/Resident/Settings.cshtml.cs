using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;

namespace SORMS.FE.Pages.Resident
{
    public class SettingsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SettingsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Account Settings (Tab 1)
        [BindProperty]
        [Required(ErrorMessage = "Mật khẩu hiện tại là bắt buộc")]
        public string? CurrentPassword { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string? NewPassword { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string? ConfirmPassword { get; set; }

        [BindProperty]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        [BindProperty]
        public string? Phone { get; set; }

        // Profile Settings (Tab 2 - Resident only)
        [BindProperty]
        public string? Address { get; set; }

        [BindProperty]
        public string? EmergencyContact { get; set; }

        [BindProperty]
        public string? Notes { get; set; }

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }

            var userRole = HttpContext.Session.GetString("UserRole");
            Console.WriteLine($"[Settings OnGet] UserRole: {userRole}");
            
            if (userRole != "Resident")
            {
                ErrorMessage = "Bạn không có quyền truy cập trang này.";
                return Page();
            }

            // Load current resident info
            Console.WriteLine("[Settings OnGet] Loading resident info...");
            await LoadResidentInfoAsync(token);

            Console.WriteLine($"[Settings OnGet] Loaded - Email: {Email}, Phone: {Phone}, Address: {Address}");

            // CRITICAL: Set ModelState values sau khi load để asp-for hiển thị đúng
            ModelState.SetModelValue(nameof(Email), new Microsoft.AspNetCore.Mvc.ModelBinding.ValueProviderResult(Email ?? ""));
            ModelState.SetModelValue(nameof(Phone), new Microsoft.AspNetCore.Mvc.ModelBinding.ValueProviderResult(Phone ?? ""));
            ModelState.SetModelValue(nameof(Address), new Microsoft.AspNetCore.Mvc.ModelBinding.ValueProviderResult(Address ?? ""));
            ModelState.SetModelValue(nameof(EmergencyContact), new Microsoft.AspNetCore.Mvc.ModelBinding.ValueProviderResult(EmergencyContact ?? ""));
            ModelState.SetModelValue(nameof(Notes), new Microsoft.AspNetCore.Mvc.ModelBinding.ValueProviderResult(Notes ?? ""));

            return Page();
        }

        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }

            // Manual validation since we're using name attributes instead of asp-for
            if (string.IsNullOrWhiteSpace(CurrentPassword))
            {
                ErrorMessage = "Vui lòng nhập mật khẩu hiện tại.";
                await LoadResidentInfoAsync(token);
                return Page();
            }

            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                ErrorMessage = "Vui lòng nhập mật khẩu mới.";
                await LoadResidentInfoAsync(token);
                return Page();
            }

            if (NewPassword.Length < 6)
            {
                ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự.";
                await LoadResidentInfoAsync(token);
                return Page();
            }

            if (NewPassword != ConfirmPassword)
            {
                ErrorMessage = "Mật khẩu mới và xác nhận mật khẩu không khớp.";
                await LoadResidentInfoAsync(token);
                return Page();
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient("API");
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var requestData = new
                {
                    CurrentPassword,
                    NewPassword
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(requestData),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await httpClient.PostAsync("/api/auth/change-password", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "Đổi mật khẩu thành công!";
                    CurrentPassword = NewPassword = ConfirmPassword = null;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Đổi mật khẩu thất bại: {error}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Đã xảy ra lỗi: {ex.Message}";
            }

            await LoadResidentInfoAsync(token);
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateAccountAsync()
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }

            // Manual validation
            if (string.IsNullOrWhiteSpace(Email) && string.IsNullOrWhiteSpace(Phone))
            {
                ErrorMessage = "Vui lòng nhập ít nhất một thông tin (Email hoặc Số điện thoại).";
                await LoadResidentInfoAsync(token);
                return Page();
            }

            Console.WriteLine($"[UpdateAccount] Email: {Email}, Phone: {Phone}");

            try
            {
                var httpClient = _httpClientFactory.CreateClient("API");
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var requestData = new
                {
                    Email = Email ?? "",
                    Phone = Phone ?? ""
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(requestData),
                    Encoding.UTF8,
                    "application/json"
                );

                Console.WriteLine($"[UpdateAccount] Sending request: {JsonSerializer.Serialize(requestData)}");

                var response = await httpClient.PutAsync("/api/resident/update-account", jsonContent);

                Console.WriteLine($"[UpdateAccount] Response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "Cập nhật thông tin tài khoản thành công!";
                    // Update session
                    if (!string.IsNullOrEmpty(Email))
                    {
                        HttpContext.Session.SetString("UserEmail", Email);
                    }
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[UpdateAccount] Error: {error}");
                    ErrorMessage = $"Cập nhật thất bại: {error}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UpdateAccount] Exception: {ex}");
                ErrorMessage = $"Đã xảy ra lỗi: {ex.Message}";
            }

            // Load lại dữ liệu
            await LoadResidentInfoAsync(token);
            
            // Set ModelState để hiển thị dữ liệu
            ModelState.SetModelValue(nameof(Email), new Microsoft.AspNetCore.Mvc.ModelBinding.ValueProviderResult(Email ?? ""));
            ModelState.SetModelValue(nameof(Phone), new Microsoft.AspNetCore.Mvc.ModelBinding.ValueProviderResult(Phone ?? ""));
            ModelState.SetModelValue(nameof(Address), new Microsoft.AspNetCore.Mvc.ModelBinding.ValueProviderResult(Address ?? ""));
            ModelState.SetModelValue(nameof(EmergencyContact), new Microsoft.AspNetCore.Mvc.ModelBinding.ValueProviderResult(EmergencyContact ?? ""));
            ModelState.SetModelValue(nameof(Notes), new Microsoft.AspNetCore.Mvc.ModelBinding.ValueProviderResult(Notes ?? ""));
            
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateProfileAsync()
        {
            Console.WriteLine("=== OnPostUpdateProfileAsync CALLED ===");
            
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("[UpdateProfile] No token - redirecting to login");
                return RedirectToPage("/Auth/Login");
            }

            Console.WriteLine($"[UpdateProfile] Address: {Address}");
            Console.WriteLine($"[UpdateProfile] EmergencyContact: {EmergencyContact}");
            Console.WriteLine($"[UpdateProfile] Notes: {Notes}");

            try
            {
                var httpClient = _httpClientFactory.CreateClient("API");
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var requestData = new
                {
                    Address,
                    EmergencyContact,
                    Notes
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(requestData),
                    Encoding.UTF8,
                    "application/json"
                );

                Console.WriteLine($"[UpdateProfile] Request JSON: {JsonSerializer.Serialize(requestData)}");

                var response = await httpClient.PutAsync("/api/resident/update-profile", jsonContent);

                Console.WriteLine($"[UpdateProfile] Response Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "Cập nhật hồ sơ thành công!";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[UpdateProfile] Error Response: {error}");
                    ErrorMessage = $"Cập nhật thất bại: {error}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UpdateProfile] Exception: {ex}");
                ErrorMessage = $"Đã xảy ra lỗi: {ex.Message}";
            }

            // Load lại dữ liệu
            await LoadResidentInfoAsync(token);
            
            // Set ModelState để hiển thị dữ liệu
            ModelState.SetModelValue(nameof(Email), new Microsoft.AspNetCore.Mvc.ModelBinding.ValueProviderResult(Email ?? ""));
            ModelState.SetModelValue(nameof(Phone), new Microsoft.AspNetCore.Mvc.ModelBinding.ValueProviderResult(Phone ?? ""));
            ModelState.SetModelValue(nameof(Address), new Microsoft.AspNetCore.Mvc.ModelBinding.ValueProviderResult(Address ?? ""));
            ModelState.SetModelValue(nameof(EmergencyContact), new Microsoft.AspNetCore.Mvc.ModelBinding.ValueProviderResult(EmergencyContact ?? ""));
            ModelState.SetModelValue(nameof(Notes), new Microsoft.AspNetCore.Mvc.ModelBinding.ValueProviderResult(Notes ?? ""));
            
            return Page();
        }

        private async Task LoadResidentInfoAsync(string token)
        {
            try
            {
                Console.WriteLine("[LoadResidentInfo] Starting...");
                var httpClient = _httpClientFactory.CreateClient("API");
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                Console.WriteLine("[LoadResidentInfo] Calling /api/resident/my-profile...");
                var response = await httpClient.GetAsync("/api/resident/my-profile");

                Console.WriteLine($"[LoadResidentInfo] Response Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[LoadResidentInfo] JSON Response: {jsonContent}");
                    
                    var resident = JsonSerializer.Deserialize<ResidentProfileDto>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (resident != null)
                    {
                        Email = resident.Email;
                        Phone = resident.Phone;
                        Address = resident.Address;
                        EmergencyContact = resident.EmergencyContact;
                        Notes = resident.Notes;
                        
                        Console.WriteLine($"[LoadResidentInfo] Deserialized - Email: {Email}, Phone: {Phone}");
                    }
                    else
                    {
                        Console.WriteLine("[LoadResidentInfo] Deserialization returned null");
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[LoadResidentInfo] Error Response: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoadResidentInfo] Exception: {ex.Message}");
                Console.WriteLine($"[LoadResidentInfo] Stack Trace: {ex.StackTrace}");
            }
        }

        private class ResidentProfileDto
        {
            public string? Email { get; set; }
            public string? Phone { get; set; }
            public string? Address { get; set; }
            public string? EmergencyContact { get; set; }
            public string? Notes { get; set; }
        }
    }
}
