using System.ComponentModel;
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
        [DisplayName("Date of Hire")]
        public DateTime DateOfHire { get; set; }

        // Employee tenure (used to determine annual leave)
        [DisplayName("Years of Service")]
        public int YearsOfService => CalculateYearsOfService();

        // NEW: Tracks the employee’s annual leave balance
        [Required]
        [DisplayName("Annual Leave Balance")]
        public int AnnualLeaveBalance { get; set; } = 0;
        public int TotalLeavesDiscarded { get; set; } = 0;

        // Navigation property for related leaves
        public ICollection<LeaveRecord> LeaveRecords { get; set; } = [];

        private Employee() { }
        public Employee(string fullName, string email, DateTime dateOfHire)
        {
            FullName = fullName;
            Email = email;
            DateOfHire = dateOfHire;
            SetAnnualLeaveBalance();
        }

        public void Update(string fullName, string email, DateTime dateOfHire)
        {
            FullName = fullName;
            Email = email;
            DateOfHire = dateOfHire;
            SetAnnualLeaveBalance();
        }


        /// <summary>
        /// Calculates the current annual leave balance considering:
        /// 1. Annual leave credits (15 days for first 4 years, 17 days afterwards)
        /// 2. Leaves from previous year that haven't expired yet (within 1.5 year window)
        /// 3. Current year's leaves
        /// 
        /// Example for an employee hired on 2022-01-01 (with now being 2025-07-01):
        /// 
        /// Year 1 (2022):
        /// - Gets 15 days on 2022-01-01
        /// - Must use by 2023-07-01 (1.5 years later)
        /// 
        /// Year 2 (2023):
        /// - Gets 15 new days on 2023-01-01
        /// - Until 2023-07-01: Can use remaining days from 2022 + 15 days from 2023
        /// - After 2023-07-01: Unused 2022 days are discarded, can only use 2023 days
        /// - Must use 2022 days by 2023-07-01
        /// 
        /// This pattern continues, with each year's leaves having a 1.5 year window to be used
        /// At any point, employee might have access to:
        /// - Current year's leaves
        /// - Previous year's remaining leaves (if within 1.5 year window)
        /// </summary>
        public void SetAnnualLeaveBalance()
        {
            // Calculate initial leaves (15 days) validity period
            var initialLeavesEndDate = DateOfHire.AddMonths(18);
            var now = DateTime.Now;
            int totalAvailableLeaves = 0;

            // Go through each year of service
            for (int year = 1; year <= YearsOfService; year++)
            {
                int leavesForYear = (year <= 4) ? 15 : 17;
                var yearStartDate = DateOfHire.AddYears(year - 1);
                var yearEndDate = DateOfHire.AddYears(year);
                var yearAndHalfDate = yearStartDate.AddMonths(18);

                // Skip if the year hasn't started yet
                if (yearStartDate > now) continue;

                // If 1.5 years have passed, these leaves are expired
                if (now > yearAndHalfDate) continue;

                // Add the leaves for this year
                totalAvailableLeaves += leavesForYear;

                // Calculate leaves used during this year's validity period
                var leavesUsedInPeriod = LeaveRecords.Where(lr => lr.Approved && lr.LeaveType.Name == "Annual")
                    .Where(lr => lr.StartDate >= yearStartDate &&
                               lr.StartDate < (now > yearAndHalfDate ? yearAndHalfDate : now))
                    .Sum(lr => lr.TotalDays);

                // Subtract used leaves
                totalAvailableLeaves -= leavesUsedInPeriod;
            }
            AnnualLeaveBalance = totalAvailableLeaves;
        }

        private int CalculateYearsOfService()
        {
            var now = DateTime.Now;
            int yearsDifference = DateTime.Now.Year - DateOfHire.Year;

            // Uncomment this if you want to allow employees who have not worked a full year to have annual leaves
            //if (DateTime.Now >= DateOfHire && DateTime.Now.Year == DateOfHire.Year)
            //    yearsDifference = 1;

            return yearsDifference;
        }
    }
}
