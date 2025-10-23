namespace SORMS.API.DTOs
{
    public class BillingDto
    {
        public int Id { get; set; }
        public DateTime BillingDate { get; set; }
        public decimal Amount { get; set; }
        public bool IsPaid { get; set; }
        public int ResidentId { get; set; }
        
        // Additional properties for display purposes
        public string ResidentName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
