using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SORMS.FE.Pages.Resident
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

            // Check if user is Resident
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Resident")
            {
                return RedirectToPage("/AccessDenied");
            }

            return Page();
        }
    }
}
