namespace HrPuSystem.Models.Filters
{
    public class EmployeeFilter
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public DateTime? DateOfHireFrom { get; set; }
        public DateTime? DateOfHireTo { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}