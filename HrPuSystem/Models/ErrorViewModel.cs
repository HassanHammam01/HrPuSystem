namespace HrPuSystem.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public string? ExceptionMessage { get; set; }
        public bool IsDevelopment { get; set; }
        public bool IsApplicationException { get; set; }
    }
}
