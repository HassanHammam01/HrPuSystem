using HrPuSystem.Data;
using HrPuSystem.Models;
using HrPuSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HrPuSystem.Models.Filters;

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
        [HttpGet("/leaveRecords")]
        public async Task<IActionResult> Index(LeaveRecordFilter filter)
        {
            var query = _context.LeaveRecords
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filter.EmployeeName))
                query = query.Where(lr => lr.Employee.FullName.Contains(filter.EmployeeName));

            if (filter.LeaveTypeId.HasValue)
                query = query.Where(lr => lr.LeaveTypeId == filter.LeaveTypeId);

            if (filter.StartDate.HasValue)
                query = query.Where(lr => lr.StartDate >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(lr => lr.EndDate <= filter.EndDate.Value);

            if (!string.IsNullOrEmpty(filter.Status))
            {
                query = query.Where(lr =>
                    (filter.Status == "approved" && lr.Approved) ||
                    (filter.Status == "pending" && !lr.Approved));
            }

            // Order by most recent first
            query = query.OrderByDescending(lr => lr.StartDate);

            // Setup ViewBag data for filters
            ViewBag.LeaveTypes = await _context.LeaveTypes
                .OrderBy(lt => lt.Name)
                .ToListAsync();

            var result = await PaginatedList<LeaveRecord>.CreateAsync(
                query,
                filter.PageNumber,
                filter.PageSize);

            ViewBag.Filter = filter;
            return View(result);
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
