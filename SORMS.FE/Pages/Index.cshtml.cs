using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SORMS.FE.Pages
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Check if user is logged in
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                // Not logged in, redirect to login page
                return RedirectToPage("/Auth/Login");
            }

            // Get user role from session
            var userRole = HttpContext.Session.GetString("UserRole");

            // Redirect to appropriate dashboard based on role
            return userRole switch
            {
                "Admin" => RedirectToPage("/Admin/Dashboard"),
                "Staff" => RedirectToPage("/Staff/Dashboard"),
                "Resident" => RedirectToPage("/Resident/Dashboard"),
                _ => RedirectToPage("/Auth/Login") // Unknown role, redirect to login
            };
        }
    }
}



