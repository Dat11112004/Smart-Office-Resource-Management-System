using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SORMS.API.DTOs;
using System.Text.Json;

namespace SORMS.FE.Pages.Staff
{
    public class IndexModel : PageModel
    {
        public List<StaffDto> StaffList { get; set; } = new List<StaffDto>();
        public int TotalStaff { get; set; } = 0;
        public string? ErrorMessage { get; set; }

        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Check if user is Admin
                var userRole = HttpContext.Session.GetString("UserRole");
                if (userRole != "Admin")
                {
                    return RedirectToPage("/AccessDenied");
                }

                // Get JWT token from session
                var token = HttpContext.Session.GetString("JWTToken");
                
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToPage("/Login");
                }
                
                // Create HttpClient with Authorization header
                var httpClient = _httpClientFactory.CreateClient("API");
                httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
                // Call API to get staff list
                var response = await httpClient.GetAsync("/api/staff");
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var staffList = JsonSerializer.Deserialize<List<StaffDto>>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    if (staffList != null)
                    {
                        StaffList = staffList;
                        TotalStaff = staffList.Count;
                    }
                }
                else
                {
                    ErrorMessage = $"Không thể tải danh sách nhân viên. Mã lỗi: {response.StatusCode}";
                    StaffList = new List<StaffDto>();
                }

                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi tải danh sách nhân viên: {ex.Message}";
                StaffList = new List<StaffDto>();
                return Page();
            }
        }
    }
}
