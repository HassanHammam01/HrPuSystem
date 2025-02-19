using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HrPuSystem.Data;
using HrPuSystem.Models;
using HrPuSystem.Models.Filters;

namespace HrPuSystem.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Employees
        [HttpGet("/employees")]
        public async Task<IActionResult> Index(EmployeeFilter filter)
        {
            var query = _context.Employees
                .Include(e => e.LeaveRecords)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filter.FullName))
                query = query.Where(e => e.FullName.Contains(filter.FullName));

            if (!string.IsNullOrWhiteSpace(filter.Email))
                query = query.Where(e => e.Email.Contains(filter.Email));

            if (filter.DateOfHireFrom.HasValue)
                query = query.Where(e => e.DateOfHire >= filter.DateOfHireFrom.Value);

            if (filter.DateOfHireTo.HasValue)
                query = query.Where(e => e.DateOfHire <= filter.DateOfHireTo.Value);

            // Order by name
            query = query.OrderBy(e => e.FullName);

            var result = await PaginatedList<Employee>.CreateAsync(
                query,
                filter.PageNumber,
                filter.PageSize);

            ViewBag.Filter = filter;
            return View(result);
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id, int? year, string? status)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Set current year as default only if year parameter is not provided in the URL
            if (!Request.Query.ContainsKey("year"))
            {
                year = DateTime.Now.Year;
            }

            var employee = await _context.Employees
                .Include(e => e.LeaveRecords.Where(lr =>
                    (!year.HasValue || lr.StartDate.Year == year.Value) &&
                    (string.IsNullOrEmpty(status) ||
                    (status == "approved" && lr.Approved) ||
                    (status == "pending" && !lr.Approved))))
                    .ThenInclude(lr => lr.LeaveType)
                .FirstOrDefaultAsync(m => m.EmployeeId == id);

            if (employee == null)
            {
                return NotFound();
            }

            ViewBag.SelectedYear = year;
            ViewBag.SelectedStatus = status;
            ViewBag.AvailableYears = await _context.LeaveRecords
                .Where(lr => lr.EmployeeId == id)
                .Select(lr => lr.StartDate.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();

            // Add current year if not in the list
            if (!ViewBag.AvailableYears.Contains(DateTime.Now.Year))
            {
                var availableYears = ViewBag.AvailableYears as List<int> ?? new List<int>();
                availableYears.Insert(0, DateTime.Now.Year);
                ViewBag.AvailableYears = availableYears;
            }

            return View(employee);
        }

        // GET: Employees/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(string fullName, string email, DateTime dateOfHire)
        {
            var employee = new Employee(fullName, email, dateOfHire);

            await _context.AddAsync(employee);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Employees/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, string fullName, string email, DateTime dateOfHire)
        {
            var employee = await _context.Employees.Include(e => e.LeaveRecords).ThenInclude(lr => lr.LeaveType)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);
            if (employee == null)
            {
                return NotFound();
            }

            try
            {
                employee.Update(fullName, email, dateOfHire);
                _context.Update(employee);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(employee.EmployeeId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Employees/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(m => m.EmployeeId == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.EmployeeId == id);
        }
    }
}
