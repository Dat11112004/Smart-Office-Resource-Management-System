using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SORMS.API.DTOs;

namespace SORMS.FE.Pages.Auth
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public LoginDto LoginDto { get; set; } = new LoginDto();

        public string? Message { get; set; }

        public void OnGet()
        {
            // Initialize the LoginDto
            LoginDto = new LoginDto();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // TODO: Implement actual login logic here
            // For now, just show a message
            Message = "Login functionality will be implemented here";
            
            return Page();
        }
    }
}
