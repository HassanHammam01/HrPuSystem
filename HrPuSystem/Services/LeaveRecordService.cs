using HrPuSystem.Data;
using HrPuSystem.Models;
using HrPuSystem.Exceptions;

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

        public async Task RequestLeaveAsync(int employeeId, int leaveTypeId, DateTime startDate, DateTime endDate, bool approved = false)
        {
                // Calculate total leave days requested
                int totalDays = (endDate - startDate).Days + 1;

                // Fetch the employee and leave type
                var employee = await _context.Employees.Include(e => e.LeaveRecords).ThenInclude(lr => lr.LeaveType)
                    .FirstOrDefaultAsync(e => e.EmployeeId == employeeId)
                    ?? throw new MyApplicationException("Employee could not be found");
                var leaveType = await _context.LeaveTypes.FirstOrDefaultAsync(lt => lt.LeaveTypeId == leaveTypeId)
                    ?? throw new MyApplicationException("Leave Type could not be found");

                // Check if leave type is Annual and if the balance is sufficient
                if (leaveType.Name == "Annual" && employee.AnnualLeaveBalance < totalDays)
                    throw new MyApplicationException("Insufficient annual leave balance.");

                var leaveRecord = new LeaveRecord
                {
                    EmployeeId = employeeId,
                    LeaveTypeId = leaveTypeId,
                    StartDate = startDate,
                    EndDate = endDate,
                    Approved = approved
                };

                _context.LeaveRecords.Add(leaveRecord);


                await _context.SaveChangesAsync();
                if (approved)
                {
                    employee.SetAnnualLeaveBalance();
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
            
        }
    }
}
