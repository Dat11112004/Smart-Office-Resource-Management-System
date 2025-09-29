namespace SORMS.API.DTOs
{
    public class ReportDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime GeneratedDate { get; set; }
        public string CreatedBy { get; set; }
    }

}
