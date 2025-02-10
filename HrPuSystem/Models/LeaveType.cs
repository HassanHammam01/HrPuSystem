using System.ComponentModel.DataAnnotations;

namespace HrPuSystem.Models
{
    public class LeaveType
    {
        [Key]
        public int LeaveTypeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }  // E.g., Maternity, Paternity, Annual, Sick, etc.

        [Required]
        public int DefaultDays { get; set; }  // Default leave days, e.g., 15 for annual, 70 for maternity

        [Required]
        public bool IsPaid { get; set; }  // To mark if the leave is paid or unpaid
    }
}
