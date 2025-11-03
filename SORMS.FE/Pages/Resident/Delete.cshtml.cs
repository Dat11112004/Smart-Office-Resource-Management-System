using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SORMS.API.DTOs;
using System.Text.Json;

namespace SORMS.FE.Pages.Resident
{
    public class DeleteModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ResidentDto? Resident { get; set; }
        public string? ErrorMessage { get; set; }

        public DeleteModel(IHttpClientFactory httpClientFactory)
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

            try
            {
                var client = _httpClientFactory.CreateClient("API");
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.DeleteAsync($"/api/Resident/{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Xóa cư dân thành công!";
                    return RedirectToPage("/Resident/Index");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Xóa thất bại: {error}";
                    
                    // Reload resident data
                    var getResponse = await client.GetAsync($"/api/Resident/{id}");
                    if (getResponse.IsSuccessStatusCode)
                    {
                        var json = await getResponse.Content.ReadAsStringAsync();
                        Resident = JsonSerializer.Deserialize<ResidentDto>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                    }
                    
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
