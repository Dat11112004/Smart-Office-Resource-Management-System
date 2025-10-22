using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SORMS.API.DTOs;

namespace SORMS.FE.Pages.Auth
{
    public class ForgotPasswordModel : PageModel
    {
        [BindProperty]
        public ForgotPasswordDto ForgotPasswordDto { get; set; } = new ForgotPasswordDto();

        public string? Message { get; set; }

        public void OnGet()
        {
            // Initialize the ForgotPasswordDto
            ForgotPasswordDto = new ForgotPasswordDto();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // TODO: Implement actual forgot password logic here
            // For now, just show a message
            Message = "Forgot password functionality will be implemented here";
            
            return Page();
        }
    }
}
