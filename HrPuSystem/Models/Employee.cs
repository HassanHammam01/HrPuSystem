using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HrPuSystem.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public DateTime DateOfHire { get; set; }

        // Employee tenure (used to determine annual leave)
        public int YearsOfService => DateTime.Now.Year - DateOfHire.Year;

        // NEW: Tracks the employee’s annual leave balance
        [Required]
        public int AnnualLeaveBalance { get; set; }

        // Navigation property for related leaves
        public ICollection<LeaveRecord> LeaveRecords { get; set; } // leaves = 5

        public void SetAnnualLeaveBalanceAfterYearAndAHalf()
        {
            AnnualLeaveBalance = AnnualLeaveBalance - CalculateLeavesToDiscard();
        }

        public int CalculateLeavesToDiscard()
        {
            var dateOfHireLastYear = DateOfHire.AddYears(YearsOfService - 1);
            if (HasYearAndHalfPassed(dateOfHireLastYear))
            {
                var annualLeaveDays = YearsOfService >= 5 ? 17 : 15;

                var leavesTakenFromLastYear = LeaveRecords
                                            .Where(leave => leave.StartDate < DateOfHire.AddYears(YearsOfService) // before start of current year
                                                    && leave.StartDate >= dateOfHireLastYear)                      // after start of last year
                                            .Sum(leave => leave.TotalDays);

                return (annualLeaveDays - leavesTakenFromLastYear);
            }
            return 0;
        }

        public static bool HasYearAndHalfPassed(DateTime givenDate)
        {
            DateTime currentDate = DateTime.Now;
            TimeSpan timePassed = currentDate - givenDate;

            // 1.5 years in days (approx)
            double daysInYearAndHalf = 1.5 * 365;

            return timePassed.TotalDays >= daysInYearAndHalf;
        }
    }
}
