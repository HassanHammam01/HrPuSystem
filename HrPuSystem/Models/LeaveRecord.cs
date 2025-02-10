using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HrPuSystem.Models
{
    public class LeaveRecord
    {
        [Key]
        public int LeaveRecordId { get; set; }

        [Required]
        public int EmployeeId { get; set; }  // Foreign key to Employee table

        [Required]
        public int LeaveTypeId { get; set; }  // Foreign key to LeaveType table

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public bool Approved { get; set; }

        // Navigation properties
        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; }

        [ForeignKey("LeaveTypeId")]
        public LeaveType LeaveType { get; set; }

        // Calculate total days taken (can exclude weekends/public holidays based on future logic)
        public int TotalDays => (EndDate - StartDate).Days + 1;
    }
}
