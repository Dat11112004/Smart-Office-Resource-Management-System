using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SORMS.API.DTOs;
using System.Text;
using System.Text.Json;

namespace SORMS.FE.Pages.Resident
{
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        [BindProperty]
        public ResidentDto? Resident { get; set; }

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public EditModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }

            try
            {
                var client = _httpClientFactory.CreateClient("API");
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync($"/api/Resident/{id}");
                
                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = "Không tìm thấy cư dân.";
                    return RedirectToPage("/Resident/Index");
                }

                var json = await response.Content.ReadAsStringAsync();
                Resident = JsonSerializer.Deserialize<ResidentDto>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Đã xảy ra lỗi: {ex.Message}";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }

            if (Resident == null)
            {
                ErrorMessage = "Dữ liệu không hợp lệ.";
                return Page();
            }

            // Manual validation
            if (string.IsNullOrWhiteSpace(Resident.FullName))
            {
                ErrorMessage = "Họ tên không được để trống.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Resident.Email))
            {
                ErrorMessage = "Email không được để trống.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Resident.Phone))
            {
                ErrorMessage = "Số điện thoại không được để trống.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Resident.IdentityNumber))
            {
                ErrorMessage = "Số CMND/CCCD không được để trống.";
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient("API");
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(Resident),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PutAsync($"/api/Resident/{id}", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Cập nhật thông tin cư dân thành công!";
                    return RedirectToPage("/Resident/Details", new { id = id });
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Cập nhật thất bại: {error}";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Đã xảy ra lỗi: {ex.Message}";
                return Page();
            }
        }
    }
}
