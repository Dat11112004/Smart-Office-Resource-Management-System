using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SORMS.API.DTOs;

namespace SORMS.FE.Pages.Billing
{
    public class IndexModel : PageModel
    {
        public IEnumerable<BillingDto>? Billings { get; set; }

        public void OnGet()
        {
            // TODO: Load billing data from API
            // For now, create sample data
            Billings = new List<BillingDto>
            {
                new BillingDto
                {
                    Id = 1,
                    ResidentId = 1,
                    ResidentName = "Nguyễn Văn A",
                    Amount = 1000000,
                    IsPaid = true,
                    BillingDate = DateTime.Now.AddDays(-10),
                    CreatedAt = DateTime.Now.AddDays(-10)
                },
                new BillingDto
                {
                    Id = 2,
                    ResidentId = 2,
                    ResidentName = "Trần Thị B",
                    Amount = 1500000,
                    IsPaid = false,
                    BillingDate = DateTime.Now.AddDays(-5),
                    CreatedAt = DateTime.Now.AddDays(-5)
                },
                new BillingDto
                {
                    Id = 3,
                    ResidentId = 3,
                    ResidentName = "Lê Văn C",
                    Amount = 2000000,
                    IsPaid = true,
                    BillingDate = DateTime.Now.AddDays(-3),
                    CreatedAt = DateTime.Now.AddDays(-3)
                }
            };
        }

        public IActionResult OnPostMarkAsPaid(int id)
        {
            // TODO: Implement mark as paid logic
            return RedirectToPage();
        }
    }
}
