namespace SORMS.API.DTOs
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }

        public int ResidentId { get; set; }
    }

}
