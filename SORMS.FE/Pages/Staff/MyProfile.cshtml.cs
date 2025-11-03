using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SORMS.FE.Models;

namespace SORMS.FE.Pages.Staff
{
    public class MyProfileModel : PageModel
    {
        public UserInfo? StaffInfo { get; set; }
        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            // Kiểm tra authentication
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }

            // Kiểm tra role phải là Staff
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Staff")
            {
                ErrorMessage = "Bạn không có quyền truy cập trang này.";
                return Page();
            }

            // Lấy thông tin từ session
            StaffInfo = new UserInfo
            {
                Id = HttpContext.Session.GetInt32("UserId") ?? 0,
                Username = HttpContext.Session.GetString("Username") ?? "N/A",
                Email = HttpContext.Session.GetString("UserEmail") ?? "N/A",
                Role = userRole,
                RoleId = 2 // Staff = 2
            };

            return Page();
        }
    }
}
