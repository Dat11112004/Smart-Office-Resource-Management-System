using SORMS.API.DTOs;

namespace SORMS.API.Interfaces
{
    public interface IBillingService
    {
        Task<IEnumerable<BillingDto>> GetAllBillingsAsync();
        Task<BillingDto> GetBillingByIdAsync(int id);
        Task<BillingDto> CreateBillingAsync(BillingDto billingDto);
        Task<bool> MarkAsPaidAsync(int id);
    }

}
