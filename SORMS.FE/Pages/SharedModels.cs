namespace SORMS.FE.Pages
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }

    public class ApiErrorResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
