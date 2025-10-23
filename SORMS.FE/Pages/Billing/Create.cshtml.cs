using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SORMS.API.DTOs;

namespace SORMS.FE.Pages.Billing
{
    public class CreateModel : PageModel
    {
        [BindProperty]
        public BillingDto BillingDto { get; set; } = new BillingDto();

        public string? Message { get; set; }

        public void OnGet()
        {
            // Initialize the BillingDto
            BillingDto = new BillingDto
            {
                BillingDate = DateTime.Now
            };
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // TODO: Implement actual billing creation logic here
            // For now, just show a message
            Message = "Billing creation functionality will be implemented here";
            
            return Page();
        }
    }
}



