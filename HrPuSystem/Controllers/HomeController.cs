using HrPuSystem.Data;
using HrPuSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace HrPuSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            await UpdateAnnualLeavesAsync();

            // Get total employees
            ViewBag.TotalEmployees = await _context.Employees.CountAsync();

            // Get active and pending leave requests
            var today = DateTime.Today;
            ViewBag.ActiveLeaveRequests = await _context.LeaveRecords
                .CountAsync(lr => lr.Approved && lr.StartDate <= today && lr.EndDate >= today);

            ViewBag.PendingLeaveRequests = await _context.LeaveRecords
                .CountAsync(lr => !lr.Approved);

            // Get total leave types
            ViewBag.TotalLeaveTypes = await _context.LeaveTypes.CountAsync();

            // Get leave type distribution
            var allLeaveRequests = await _context.LeaveRecords
                .Include(lr => lr.LeaveType)
                .ToListAsync();

            var leaveTypeGroups = allLeaveRequests
                .GroupBy(lr => lr.LeaveType.Name)
                .Select(g => new
                {
                    Name = g.Key,
                    Count = g.Count(),
                    Percentage = (int)((double)g.Count() / allLeaveRequests.Count * 100)
                })
                .OrderByDescending(lt => lt.Count)
                .ToList();

            ViewBag.LeaveTypeDistribution = leaveTypeGroups;

            // Get recent leave requests
            ViewBag.RecentLeaveRequests = await _context.LeaveRecords
                .Include(lr => lr.Employee)
                .Include(lr => lr.LeaveType)
                .OrderByDescending(lr => lr.LeaveRecordId)
                .Take(5)
                .ToListAsync();

            return View();
        }

        private async Task UpdateAnnualLeavesAsync()
        {
            var employees = await _context.Employees
                .Include(e => e.LeaveRecords)
                .ThenInclude(lr => lr.LeaveType)
                .ToListAsync();

            foreach (var employee in employees)
                employee.SetAnnualLeaveBalance();

            _context.UpdateRange(employees);
            await _context.SaveChangesAsync();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
