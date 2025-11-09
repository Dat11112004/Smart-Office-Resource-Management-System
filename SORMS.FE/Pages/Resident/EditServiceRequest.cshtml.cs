using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SORMS.FE.Pages.Resident
{
    public class EditServiceRequestModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public EditServiceRequestModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public new ServiceRequestDto? Request { get; set; }

        public async Task<IActionResult> OnGetAsync(int requestId)
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            try
            {
                var client = _httpClientFactory.CreateClient("API");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync($"api/ServiceRequest/{requestId}");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    Request = JsonSerializer.Deserialize<ServiceRequestDto>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (Request == null)
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy yêu cầu";
                        return RedirectToPage("/Resident/ServiceRequests");
                    }

                    return Page();
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể tải thông tin yêu cầu";
                    return RedirectToPage("/Resident/ServiceRequests");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return RedirectToPage("/Resident/ServiceRequests");
            }
        }

        public async Task<IActionResult> OnPostAsync(int requestId, string title, string serviceType, string description, string priority)
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            try
            {
                var client = _httpClientFactory.CreateClient("API");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var requestDto = new
                {
                    Title = title,
                    ServiceType = serviceType,
                    Description = description,
                    Priority = priority
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(requestDto),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PutAsync($"api/ServiceRequest/{requestId}", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Cập nhật yêu cầu thành công!";
                }
                else
                {
                    var error = JsonSerializer.Deserialize<ApiErrorResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    TempData["ErrorMessage"] = error?.Message ?? "Không thể cập nhật yêu cầu";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }

            return RedirectToPage("/Resident/ServiceRequests");
        }
    }
}
