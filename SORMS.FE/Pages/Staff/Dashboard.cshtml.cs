using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SORMS.FE.Pages.Staff
{
    public class DashboardModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Check if user is logged in
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }

            // Check if user is Staff
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Staff")
            {
                return RedirectToPage("/AccessDenied");
            }

            return Page();
        }
    }
}
