namespace HrPuSystem.Models.Filters
{
    public class LeaveTypeFilter
    {
        public string? Name { get; set; }
        public bool? IsPaid { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}