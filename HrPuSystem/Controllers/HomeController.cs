using HrPuSystem.Data;
using HrPuSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace HrPuSystem.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet("/")]
        [HttpGet("/Home")]
        [HttpGet("/Home/Index")]
        public async Task<IActionResult> Index(int? year = null, int? month = null)
        {
            await UpdateAnnualLeavesAsync();

            var today = DateTime.Today;
            var selectedYear = year ?? today.Year;
            var startOfYear = new DateTime(selectedYear, 1, 1);
            var months = Enumerable.Range(1, 12).ToList();

            // Get total employees
            ViewBag.TotalEmployees = await _context.Employees.CountAsync();

            // Get active and pending leave requests
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
                    Percentage = allLeaveRequests.Any() ? (int)((double)g.Count() / allLeaveRequests.Count * 100) : 0
                })
                .OrderByDescending(lt => lt.Count)
                .ToList();

            ViewBag.LeaveTypeDistribution = leaveTypeGroups;

            // Get monthly leave data for chart with year filter
            var monthlyLeaves = await _context.LeaveRecords
                .Where(lr => lr.StartDate.Year == selectedYear)
                .GroupBy(lr => lr.StartDate.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Month, x => x.Count);

            ViewBag.MonthLabels = months.Select(m => new DateTime(2024, m, 1).ToString("MMMM")).ToArray();
            ViewBag.MonthlyLeaves = months.Select(m => monthlyLeaves.ContainsKey(m) ? monthlyLeaves[m] : 0).ToArray();

            // Get available years for the filter
            var availableYears = await _context.LeaveRecords
                .Select(lr => lr.StartDate.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();

            if (!availableYears.Contains(today.Year))
            {
                availableYears.Insert(0, today.Year);
            }

            ViewBag.AvailableYears = availableYears;
            ViewBag.SelectedYear = selectedYear;
            ViewBag.SelectedMonth = month;

            // Get employee summary with month filter
            var employeeQuery = _context.Employees
                .Include(e => e.LeaveRecords)
                .Select(e => new
                {
                    e.EmployeeId,
                    e.FullName,
                    e.Email,
                    e.DateOfHire,
                    e.AnnualLeaveBalance,
                    ActiveLeaves = e.LeaveRecords.Count(lr =>
                        lr.Approved &&
                        lr.StartDate <= today &&
                        lr.EndDate >= today &&
                        (!month.HasValue || (lr.StartDate.Month == month.Value || lr.EndDate.Month == month.Value))),
                    TotalLeaves = e.LeaveRecords.Count(lr =>
                        !month.HasValue ||
                        (lr.StartDate.Month == month.Value || lr.EndDate.Month == month.Value))
                });

            ViewBag.EmployeeSummary = await employeeQuery
                .OrderBy(e => e.FullName)
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

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
