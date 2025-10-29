using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SORMS.API.DTOs;

namespace SORMS.FE.Pages.Resident
{
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        
        public CreateModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        public string? FullName { get; set; }
        
        [BindProperty]
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }
        
        [BindProperty]
        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        public string? Phone { get; set; }
        
        [BindProperty]
        [Required(ErrorMessage = "Số CMND/CCCD là bắt buộc")]
        public string? IdentityNumber { get; set; }
        
        [BindProperty]
        [Required(ErrorMessage = "Vai trò là bắt buộc")]
        public string? Role { get; set; }
        
        [BindProperty]
        public DateTime CheckInDate { get; set; } = DateTime.Now;
        
        [BindProperty]
        public DateTime? CheckOutDate { get; set; }
        
        [BindProperty]
        public int? RoomId { get; set; } // Nullable - có thể tạo resident chưa có phòng
        
        [BindProperty]
        public string? Address { get; set; }
        
        [BindProperty]
        public string? EmergencyContact { get; set; }
        
        [BindProperty]
        public string? Notes { get; set; }

        public List<SelectListItem> AvailableRooms { get; set; } = new List<SelectListItem>();
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? roomId)
        {
            // Kiểm tra authentication
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }

            // 🔥 CRITICAL: Chỉ Admin và Staff mới được tạo resident thủ công
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Staff")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này. Resident tự đăng ký qua trang Register.";
                return RedirectToPage("/Index");
            }

            // Pre-fill RoomId nếu có từ query string (khi click "Thuê phòng")
            if (roomId.HasValue)
            {
                RoomId = roomId.Value;
            }

            // Load danh sách phòng trống
            await LoadAvailableRoomsAsync(token);
            
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Console.WriteLine("=== OnPostAsync START ===");
            
            // DEBUG: Log raw form data
            Console.WriteLine("=== RAW FORM DATA ===");
            foreach (var key in Request.Form.Keys)
            {
                Console.WriteLine($"  {key} = '{Request.Form[key]}'");
            }
            Console.WriteLine("===================");
            
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("ERROR: No JWT token - redirecting to login");
                return RedirectToPage("/Auth/Login");
            }

            var userRole = HttpContext.Session.GetString("UserRole");
            Console.WriteLine($"User Role: {userRole}");
            Console.WriteLine($"Token exists: Yes");
            
            // DEBUG: Log received values
            Console.WriteLine($"RECEIVED VALUES:");
            Console.WriteLine($"  FullName: '{FullName}'");
            Console.WriteLine($"  Email: '{Email}'");
            Console.WriteLine($"  Phone: '{Phone}'");
            Console.WriteLine($"  IdentityNumber: '{IdentityNumber}'");
            Console.WriteLine($"  Role: '{Role}'");
            Console.WriteLine($"  RoomId: {RoomId}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ERROR: ModelState is invalid");
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    if (errors.Any())
                    {
                        foreach (var error in errors)
                        {
                            Console.WriteLine($"  - {key}: {error.ErrorMessage}");
                        }
                    }
                }
                await LoadAvailableRoomsAsync(token);
                return Page();
            }

            try
            {
                var newResident = new
                {
                    FullName,
                    Email,
                    Phone,
                    IdentityNumber,
                    Role,
                    CheckInDate,
                    CheckOutDate,
                    RoomId,
                    Address,
                    EmergencyContact,
                    Notes,
                    IsActive = true
                };

                Console.WriteLine($"Creating resident: {FullName}, Email: {Email}, Role: {Role}");
                Console.WriteLine($"  - RoomId: {RoomId?.ToString() ?? "NULL"}");

                var httpClient = _httpClientFactory.CreateClient("API");
                httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var jsonPayload = JsonSerializer.Serialize(newResident);
                Console.WriteLine($"JSON Payload: {jsonPayload}");
                
                var jsonContent = new StringContent(
                    jsonPayload, 
                    Encoding.UTF8, 
                    "application/json"
                );

                Console.WriteLine("Sending POST to /api/Resident...");
                var response = await httpClient.PostAsync("/api/Resident", jsonContent);

                Console.WriteLine($"Response Status: {response.StatusCode}");
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response Body: {responseBody}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Thêm cư dân mới thành công!";
                    Console.WriteLine("SUCCESS: Redirecting to /Resident/Index");
                    return RedirectToPage("/Resident/Index");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Không thể tạo cư dân: {errorContent}";
                    Console.WriteLine($"Create failed: {errorContent}");
                    await LoadAvailableRoomsAsync(token);
                    return Page();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                ErrorMessage = $"Đã xảy ra lỗi: {ex.Message}";
                await LoadAvailableRoomsAsync(token);
                return Page();
            }
        }

        private async Task LoadAvailableRoomsAsync(string token)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("API");
                httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await httpClient.GetAsync("/api/Room");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var rooms = JsonSerializer.Deserialize<List<RoomDto>>(json, new JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true 
                    });

                    AvailableRooms = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "", Text = "-- Chưa chọn phòng --", Selected = !RoomId.HasValue }
                    };

                    if (rooms != null)
                    {
                        var availableRooms = rooms
                            .Where(r => r.IsAvailable && r.IsActive)
                            .OrderBy(r => r.RoomNumber)
                            .Select(r => new SelectListItem
                            {
                                Value = r.Id.ToString(),
                                Text = $"Phòng {r.RoomNumber} - Tầng {r.Floor} ({r.Type}) - {r.MonthlyRent:N0} VNĐ/tháng",
                                Selected = RoomId.HasValue && r.Id == RoomId.Value
                            });

                        AvailableRooms.AddRange(availableRooms);
                    }

                    Console.WriteLine($"Loaded {AvailableRooms.Count - 1} available rooms");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading rooms: {ex.Message}");
            }
        }
    }
}
