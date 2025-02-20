namespace HrPuSystem.Models.Filters
{
    public class LeaveRecordFilter
    {
        public string? EmployeeName { get; set; }
        public int? LeaveTypeId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "StartDate";
        public bool SortDescending { get; set; } = true;
    }
}