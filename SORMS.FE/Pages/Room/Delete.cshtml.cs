using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SORMS.API.DTOs;
using System.Text.Json;

namespace SORMS.FE.Pages.Room
{
    public class DeleteModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DeleteModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public RoomDto Room { get; set; } = new RoomDto();
        public string DeleteType { get; set; } = ""; // "soft" hoặc "hard"
        public string ErrorMessage { get; set; } = "";
        public bool CanDelete { get; set; } = false;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Kiểm tra phân quyền Admin
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xóa phòng. Chỉ Admin mới có quyền này.";
                return RedirectToPage("/Room/Index");
            }

            // Lấy JWT token
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }

            var client = _httpClientFactory.CreateClient("API");
            client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                // Lấy thông tin phòng
                var response = await client.GetAsync($"/api/Room/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    Room = JsonSerializer.Deserialize<RoomDto>(json, options) ?? new RoomDto();

                    // Kiểm tra điều kiện xóa
                    if (Room.IsOccupied)
                    {
                        DeleteType = "soft";
                        ErrorMessage = "Phòng đang có người thuê. Hệ thống sẽ VÔ HIỆU HÓA phòng này (Soft Delete).";
                        CanDelete = true;
                    }
                    else
                    {
                        // Kiểm tra lịch sử (gọi thêm API hoặc giả định dựa trên CurrentResident)
                        if (!string.IsNullOrEmpty(Room.CurrentResident))
                        {
                            DeleteType = "soft";
                            ErrorMessage = "Phòng có lịch sử check-in. Hệ thống sẽ VÔ HIỆU HÓA phòng này (Soft Delete) để giữ lại dữ liệu.";
                            CanDelete = true;
                        }
                        else
                        {
                            DeleteType = "hard";
                            ErrorMessage = "Phòng không có lịch sử. Hệ thống sẽ XÓA HOÀN TOÀN phòng này khỏi database (Hard Delete).";
                            CanDelete = true;
                        }
                    }

                    Console.WriteLine($"Delete Check - Room {Room.RoomNumber}: Type={DeleteType}, CanDelete={CanDelete}");
                }
                else
                {
                    ErrorMessage = "Không tìm thấy phòng.";
                    return RedirectToPage("/Room/Index");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading room for delete: {ex.Message}");
                ErrorMessage = "Đã xảy ra lỗi khi tải thông tin phòng.";
                return RedirectToPage("/Room/Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            // Kiểm tra phân quyền Admin
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xóa phòng. Chỉ Admin mới có quyền này.";
                return RedirectToPage("/Room/Index");
            }

            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }

            var client = _httpClientFactory.CreateClient("API");
            client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await client.DeleteAsync($"/api/Room/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Xóa phòng thành công!";
                    Console.WriteLine($"Room {id} deleted successfully");
                    return RedirectToPage("/Room/Index");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Không thể xóa phòng: {errorContent}";
                    Console.WriteLine($"Delete failed: {errorContent}");
                    return RedirectToPage("/Room/Details", new { id = id });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting room: {ex.Message}");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa phòng.";
                return RedirectToPage("/Room/Details", new { id = id });
            }
        }
    }
}
