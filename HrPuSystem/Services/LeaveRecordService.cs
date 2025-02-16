using HrPuSystem.Data;
using HrPuSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HrPuSystem.Services
{
    public class LeaveRecordService
    {
        private readonly ApplicationDbContext _context;

        public LeaveRecordService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> RequestLeaveAsync(int employeeId, int leaveTypeId, DateTime startDate, DateTime endDate)
        {
            // Calculate total leave days requested
            int totalDays = (endDate - startDate).Days + 1;

            // Fetch the employee and leave type
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
            var leaveType = await _context.LeaveTypes.FirstOrDefaultAsync(lt => lt.LeaveTypeId == leaveTypeId);

            if (employee == null || leaveType == null) return false;

            // Check if leave type is Annual and if the balance is sufficient
            if (leaveType.Name == "Annual")
            {
                if (employee.AnnualLeaveBalance < totalDays)
                {
                    throw new InvalidOperationException("Insufficient annual leave balance.");
                }

                // Deduct the leave days from the annual balance
                employee.AnnualLeaveBalance -= totalDays;
            }

            // Create the leave record
            var leaveRecord = new LeaveRecord
            {
                EmployeeId = employeeId,
                LeaveTypeId = leaveTypeId,
                StartDate = startDate,
                EndDate = endDate,
                Approved = false // Initially not approved
            };

            _context.LeaveRecords.Add(leaveRecord);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
