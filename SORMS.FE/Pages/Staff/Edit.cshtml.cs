using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SORMS.API.DTOs;
using System.Text;
using System.Text.Json;

namespace SORMS.FE.Pages.Staff
{
    public class EditModel : PageModel
    {
        [BindProperty]
        public StaffDto? Staff { get; set; }
        public string? ErrorMessage { get; set; }

        private readonly IHttpClientFactory _httpClientFactory;

        public EditModel(IHttpClientFactory httpClientFactory)
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

        public async Task<IActionResult> OnPostAsync(int id)
        {
            // Check if user is Admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                return RedirectToPage("/AccessDenied");
            }

            try
            {
                // Manual validation
                if (Staff == null)
                {
                    ErrorMessage = "Dữ liệu không hợp lệ.";
                    return Page();
                }

                if (string.IsNullOrWhiteSpace(Staff.FullName))
                {
                    ErrorMessage = "Vui lòng nhập họ tên nhân viên.";
                    return Page();
                }

                if (string.IsNullOrWhiteSpace(Staff.Email))
                {
                    ErrorMessage = "Vui lòng nhập email.";
                    return Page();
                }

                if (!Staff.Email.Contains("@"))
                {
                    ErrorMessage = "Email không hợp lệ.";
                    return Page();
                }

                if (string.IsNullOrWhiteSpace(Staff.Phone))
                {
                    ErrorMessage = "Vui lòng nhập số điện thoại.";
                    return Page();
                }

                var token = HttpContext.Session.GetString("JWTToken");
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToPage("/Login");
                }

                var httpClient = _httpClientFactory.CreateClient("API");
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var json = JsonSerializer.Serialize(Staff);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PutAsync($"/api/staff/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Cập nhật thông tin nhân viên thành công!";
                    return RedirectToPage("/Staff/Details", new { id = id });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Không thể cập nhật thông tin. Lỗi: {errorContent}";
                    
                    // Reload staff data to display form again
                    var getResponse = await httpClient.GetAsync($"/api/staff/{id}");
                    if (getResponse.IsSuccessStatusCode)
                    {
                        var jsonContent = await getResponse.Content.ReadAsStringAsync();
                        Staff = JsonSerializer.Deserialize<StaffDto>(jsonContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                    }
                    
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi cập nhật thông tin: {ex.Message}";
                return Page();
            }
        }
    }
}
