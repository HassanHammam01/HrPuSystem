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
        public int YearsOfService => CalculateYearsOfService();

        // NEW: Tracks the employee’s annual leave balance
        [Required]
        public int AnnualLeaveBalance { get; set; } = 0;
        public int TotalLeavesDiscarded { get; set; } = 0;

        // Navigation property for related leaves
        public ICollection<LeaveRecord> LeaveRecords { get; set; } // leaves = 5

        public void AddAnnualLeaves()
        {
            int totalLeaves = 0;

            if (YearsOfService < 5)
                totalLeaves = YearsOfService * 15;
            else if (YearsOfService >= 5)
            {
                totalLeaves = 4 * 15;
                totalLeaves += (YearsOfService - 4) * 17;
            }

            var leavesTaken = LeaveRecords.Sum(leave => leave.TotalDays);

            // AnnualLeaveBalance -> leaves allowed, with everything subtracted
            // totalLeaves -> total, with current year leaves added

            var totalLeavesLeft = totalLeaves - leavesTaken - TotalLeavesDiscarded;

            AnnualLeaveBalance = totalLeavesLeft;
        }

        private int CalculateYearsOfService()
        {
            var now = DateTime.Now;
            int yearsDifference = DateTime.Now.Year - DateOfHire.Year;

            // Check if full year has passed, if not, subtract 1
            if (now < DateOfHire.AddYears(yearsDifference))
                yearsDifference--;
            return yearsDifference;
        }

        public void SetAnnualLeaveBalanceAfterYearAndAHalf()
        {
            var leavesToDiscard = CalculateLeavesToDiscardFromLastYearAndAHalf();
            AnnualLeaveBalance = AnnualLeaveBalance - leavesToDiscard;
            TotalLeavesDiscarded += leavesToDiscard;
        }

        private int CalculateLeavesToDiscardFromLastYearAndAHalf()
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
        private static bool HasYearAndHalfPassed(DateTime givenDate)
        {
            DateTime currentDate = DateTime.Now;
            TimeSpan timePassed = currentDate - givenDate;

            // 1.5 years in days (approx)
            double daysInYearAndHalf = 1.5 * 365;

            return timePassed.TotalDays >= daysInYearAndHalf;
        }

        private void CalculateTotalLeavesToDisacrd()
        {
            var countOfYearAndHalfPeriods = CalculateYearAndAHalfs(DateOfHire);

            var startDate = DateOfHire;
            var endDate = startDate.AddMonths(18);

            for (int yearAndAHalf = 0; yearAndAHalf < countOfYearAndHalfPeriods; yearAndAHalf++)
            {
                var totalToDiscard = CalculateLeavesToDiscardBetweenTwoDates(startDate, endDate);

                startDate = DateOfHire.AddYears(yearAndAHalf);
                endDate = startDate.AddMonths(18);
            }
        }

        private int CalculateYearAndAHalfs(DateTime startDate)
        {
            var currentDate = DateTime.Now;

            int countOfYearAndHalfPeriods = 0;
            DateTime nextPeriod = startDate;

            // Loop to check how many 1.5 year periods have fully passed
            while (nextPeriod.AddMonths(18) <= currentDate)
            {
                nextPeriod = nextPeriod.AddMonths(18);
                countOfYearAndHalfPeriods++;
            }

            return countOfYearAndHalfPeriods;
        }

        private int CalculateLeavesToDiscardBetweenTwoDates(DateTime start, DateTime end)
        {
            var dateOfHireLastYear = DateOfHire.AddYears(YearsOfService - 1);
            if (HasYearAndHalfPassedBetweenTwoDates(start, end))
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

        private static bool HasYearAndHalfPassedBetweenTwoDates(DateTime startDate, DateTime endDate)
        {
            TimeSpan timePassed = startDate - endDate;

            // 1.5 years in days (approx)
            var daysInYearAndHalf = 1.5 * 365;

            return timePassed.TotalDays >= daysInYearAndHalf;
        }
    }
}
