using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SORMS.FE.Pages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SORMS.FE.Pages.Resident
{
    public class RequestCheckOutModel : BasePageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RequestCheckOutModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public CheckInRecordDto? CurrentCheckIn { get; set; }

        public async Task<IActionResult> OnGetAsync(int checkInRecordId)
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            try
            {
                var client = _httpClientFactory.CreateClient("API");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Lấy thông tin check-in hiện tại của resident này
                var response = await client.GetAsync("api/CheckIn/my-status");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<CheckInRecordDto>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    CurrentCheckIn = result?.Data;

                    // Kiểm tra trạng thái và ID khớp
                    if (CurrentCheckIn == null)
                    {
                        TempData["ErrorMessage"] = "Bạn chưa check-in vào phòng nào.";
                        return RedirectToPage("/Resident/CheckInStatus");
                    }
                    
                    if (CurrentCheckIn.Id != checkInRecordId)
                    {
                        TempData["ErrorMessage"] = "ID check-in không hợp lệ.";
                        return RedirectToPage("/Resident/CheckInStatus");
                    }
                    
                    if (CurrentCheckIn.Status != "CheckedIn")
                    {
                        TempData["ErrorMessage"] = "Bạn không thể yêu cầu check-out khi chưa check-in.";
                        return RedirectToPage("/Resident/CheckInStatus");
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể tải thông tin check-in.";
                    return RedirectToPage("/Resident/CheckInStatus");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải thông tin: " + ex.Message;
                return RedirectToPage("/Resident/CheckInStatus");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int checkInRecordId)
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            try
            {
                var client = _httpClientFactory.CreateClient("API");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var requestDto = new { CheckInRecordId = checkInRecordId };
                var content = new StringContent(
                    JsonSerializer.Serialize(requestDto),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync("api/CheckIn/request-checkout", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Yêu cầu check-out đã được gửi! Vui lòng chờ Staff/Admin phê duyệt.";
                    return RedirectToPage("/Resident/CheckInStatus");
                }
                else
                {
                    var error = JsonSerializer.Deserialize<ApiErrorResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    TempData["ErrorMessage"] = error?.Message ?? "Không thể gửi yêu cầu check-out";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }

            return RedirectToPage("/Resident/CheckInStatus");
        }
    }
}
