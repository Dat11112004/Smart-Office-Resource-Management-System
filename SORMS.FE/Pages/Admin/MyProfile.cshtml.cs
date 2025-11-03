using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SORMS.FE.Models;

namespace SORMS.FE.Pages.Admin
{
    public class MyProfileModel : PageModel
    {
        public UserInfo? AdminInfo { get; set; }
        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            // Kiểm tra authentication
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }

            // Kiểm tra role phải là Admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                ErrorMessage = "Bạn không có quyền truy cập trang này.";
                return Page();
            }

            // Lấy thông tin từ session
            AdminInfo = new UserInfo
            {
                Id = HttpContext.Session.GetInt32("UserId") ?? 0,
                Username = HttpContext.Session.GetString("Username") ?? "N/A",
                Email = HttpContext.Session.GetString("UserEmail") ?? "N/A",
                Role = userRole,
                RoleId = 1 // Admin = 1
            };

            return Page();
        }
    }
}
