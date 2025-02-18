using HrPuSystem.Data;
using HrPuSystem.Models;
using HrPuSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HrPuSystem.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class LeaveRecordsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly LeaveRecordService _leaveRecordService;

        public LeaveRecordsController(ApplicationDbContext context, LeaveRecordService leaveRecordService)
        {
            _context = context;
            _leaveRecordService = leaveRecordService;
        }

        // GET: LeaveRecords
        public async Task<IActionResult> Index(int? year, string? status)
        {
            // Set current year as default only if year parameter is not provided in the URL
            if (!Request.Query.ContainsKey("year"))
            {
                year = DateTime.Now.Year;
            }

            var query = _context.LeaveRecords
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .Where(lr =>
                    (!year.HasValue || lr.StartDate.Year == year.Value) &&
                    (string.IsNullOrEmpty(status) ||
                    (status == "approved" && lr.Approved) ||
                    (status == "pending" && !lr.Approved)));

            ViewBag.SelectedYear = year;
            ViewBag.SelectedStatus = status;
            ViewBag.AvailableYears = await _context.LeaveRecords
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

            return View(await query.OrderByDescending(lr => lr.StartDate).ToListAsync());
        }

        // GET: LeaveRecords/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leaveRecord = await _context.LeaveRecords
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .FirstOrDefaultAsync(m => m.LeaveRecordId == id);
            if (leaveRecord == null)
            {
                return NotFound();
            }

            return View(leaveRecord);
        }

        // GET: LeaveRecords/Create
        public IActionResult Create()
        {
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "Email");
            ViewData["LeaveTypeId"] = new SelectList(_context.LeaveTypes, "LeaveTypeId", "Name");
            return View();
        }

        // POST: LeaveRecords/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int employeeId, int leaveTypeId, DateTime startDate, DateTime endDate, bool approved = true)
        {
            ViewData["LeaveTypeId"] = new SelectList(_context.LeaveTypes.OrderBy(lt => lt.Name), "LeaveTypeId", "Name", leaveTypeId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FullName", employeeId);

            await _leaveRecordService.RequestLeaveAsync(employeeId, leaveTypeId, startDate, endDate, approved);

            return RedirectToAction(nameof(Index));
        }

        // GET: LeaveRecords/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leaveRecord = await _context.LeaveRecords.Include(lr => lr.Employee).FirstOrDefaultAsync(lr => lr.LeaveRecordId == id);
            if (leaveRecord == null)
            {
                return NotFound();
            }

            if (leaveRecord.Approved)
            {
                return RedirectToAction(nameof(Details), new { id = leaveRecord.LeaveRecordId });
            }

            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "Email", leaveRecord.EmployeeId);
            ViewData["LeaveTypeId"] = new SelectList(_context.LeaveTypes, "LeaveTypeId", "Name", leaveRecord.LeaveTypeId);
            return View(leaveRecord);
        }

        // POST: LeaveRecords/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LeaveRecordId,EmployeeId,LeaveTypeId,StartDate,EndDate,Approved")] LeaveRecord leaveRecord)
        {
            if (id != leaveRecord.LeaveRecordId)
            {
                return NotFound();
            }

            var existingRecord = await _context.LeaveRecords.AsNoTracking().FirstOrDefaultAsync(lr => lr.LeaveRecordId == id);
            if (existingRecord != null && existingRecord.Approved)
            {
                return RedirectToAction(nameof(Details), new { id = leaveRecord.LeaveRecordId });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(leaveRecord);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LeaveRecordExists(leaveRecord.LeaveRecordId))
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
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "Email", leaveRecord.EmployeeId);
            ViewData["LeaveTypeId"] = new SelectList(_context.LeaveTypes, "LeaveTypeId", "Name", leaveRecord.LeaveTypeId);
            return View(leaveRecord);
        }

        // GET: LeaveRecords/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leaveRecord = await _context.LeaveRecords
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .FirstOrDefaultAsync(m => m.LeaveRecordId == id);
            if (leaveRecord == null)
            {
                return NotFound();
            }

            if (leaveRecord.Approved)
            {
                return RedirectToAction(nameof(Details), new { id = leaveRecord.LeaveRecordId });
            }

            return View(leaveRecord);
        }

        // POST: LeaveRecords/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var leaveRecord = await _context.LeaveRecords.FindAsync(id);
            if (leaveRecord != null && !leaveRecord.Approved)
            {
                _context.LeaveRecords.Remove(leaveRecord);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: LeaveRecords/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var leaveRecord = await _context.LeaveRecords
                .Include(l => l.Employee).ThenInclude(e => e.LeaveRecords)
                .Include(l => l.LeaveType)
                .FirstOrDefaultAsync(m => m.LeaveRecordId == id);

            if (leaveRecord == null)
            {
                return NotFound();
            }

            if (!leaveRecord.Approved)
            {
                leaveRecord.Approved = true;
                _context.Update(leaveRecord);
                await _context.SaveChangesAsync();
                leaveRecord.Employee.SetAnnualLeaveBalance();
                _context.Update(leaveRecord.Employee);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool LeaveRecordExists(int id)
        {
            return _context.LeaveRecords.Any(e => e.LeaveRecordId == id);
        }
    }
}
