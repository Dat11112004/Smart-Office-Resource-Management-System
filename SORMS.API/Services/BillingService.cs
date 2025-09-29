namespace SORMS.API.Services
{
    using Microsoft.EntityFrameworkCore;
    using SORMS.API.Data;
    using SORMS.API.DTOs;
    using SORMS.API.Interfaces;
    using SORMS.API.Models;

    public class BillingService : IBillingService
    {
        private readonly SormsDbContext _context;

        public BillingService(SormsDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BillingDto>> GetAllBillingsAsync()
        {
            var billings = await _context.Billings
                .Include(b => b.Resident)
                .OrderByDescending(b => b.BillingDate)
                .ToListAsync();

            return billings.Select(b => new BillingDto
            {
                Id = b.Id,
                BillingDate = b.BillingDate,
                Amount = b.Amount,
                IsPaid = b.IsPaid,
                ResidentId = b.ResidentId
            });
        }

        public async Task<BillingDto> GetBillingByIdAsync(int id)
        {
            var billing = await _context.Billings
                .Include(b => b.Resident)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (billing == null) return null;

            return new BillingDto
            {
                Id = billing.Id,
                BillingDate = billing.BillingDate,
                Amount = billing.Amount,
                IsPaid = billing.IsPaid,
                ResidentId = billing.ResidentId
            };
        }

        public async Task<BillingDto> CreateBillingAsync(BillingDto billingDto)
        {
            var billing = new Billing
            {
                BillingDate = billingDto.BillingDate,
                Amount = billingDto.Amount,
                IsPaid = billingDto.IsPaid,
                ResidentId = billingDto.ResidentId
            };

            _context.Billings.Add(billing);
            await _context.SaveChangesAsync();

            billingDto.Id = billing.Id;
            return billingDto;
        }

        public async Task<bool> MarkAsPaidAsync(int id)
        {
            var billing = await _context.Billings.FindAsync(id);
            if (billing == null || billing.IsPaid)
                return false;

            billing.IsPaid = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }

}

