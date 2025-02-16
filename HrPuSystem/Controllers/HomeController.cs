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
            return View();
        }

        private async Task UpdateAnnualLeavesAsync()
        {
            var employees = await _context.Employees.Include(e => e.LeaveRecords).ToListAsync();
            
            foreach (var employee in employees)
            {
                employee.SetAnnualLeaveBalanceAfterYearAndAHalf();
                employee.AddAnnualLeaves();
            }

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
