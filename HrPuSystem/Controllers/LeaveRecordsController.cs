using HrPuSystem.Data;
using HrPuSystem.Models;
using HrPuSystem.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HrPuSystem.Controllers
{
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
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.LeaveRecords.Include(l => l.Employee).Include(l => l.LeaveType);
            return View(await applicationDbContext.ToListAsync());
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

            var leaveRecord = await _context.LeaveRecords.FindAsync(id);
            if (leaveRecord == null)
            {
                return NotFound();
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

            return View(leaveRecord);
        }

        // POST: LeaveRecords/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var leaveRecord = await _context.LeaveRecords.FindAsync(id);
            if (leaveRecord != null)
            {
                _context.LeaveRecords.Remove(leaveRecord);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LeaveRecordExists(int id)
        {
            return _context.LeaveRecords.Any(e => e.LeaveRecordId == id);
        }
    }
}
