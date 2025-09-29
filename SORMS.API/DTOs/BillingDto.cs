namespace SORMS.API.DTOs
{
    public class BillingDto
    {
        public int Id { get; set; }
        public DateTime BillingDate { get; set; }
        public decimal Amount { get; set; }
        public bool IsPaid { get; set; }

        public int ResidentId { get; set; }
    }

}
