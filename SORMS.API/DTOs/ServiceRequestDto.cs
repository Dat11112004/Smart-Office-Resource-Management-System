namespace SORMS.API.DTOs
{
    public class ServiceRequestDto
    {
        public int Id { get; set; }
        public string ServiceType { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }

        public int ResidentId { get; set; }
        public int StaffId { get; set; }
    }

}
