using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // Required for User.FindFirstValue
using Waads.Data;
using Waads.Models;

namespace Waads.Controllers
{
    [Authorize] // This ensures only logged-in employees can access follow-ups
    public class FollowUpsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FollowUpsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: FollowUps - Displays a list of all commitments
        public async Task<IActionResult> Index()
        {
            return View(await _context.FollowUps.ToListAsync());
        }

        // GET: FollowUps/Create - Shows the form to add a new commitment
        public IActionResult Create()
        {
            return View();
        }

        // POST: FollowUps/Create - Saves new data and triggers automatic alerts
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,OrganizationName,ContactPerson,DueDate,Status")] FollowUp followUp)
        {
            if (ModelState.IsValid)
            {
                // 1. LINK TO LOGGED-IN USER: Capture the ID of the person currently signed in
                // This ensures the dashboard can filter and show this data to the correct employee
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                followUp.AssignedEmployeeId = userId;

                // 2. SAVE FOLLOW-UP: Persist the commitment to the database
                _context.Add(followUp);
                await _context.SaveChangesAsync();

                // 3. AUTOMATION: Automatically create a linked alert for accountability
                var alert = new Alert
                {
                    FollowUpId = followUp.Id,
                    Message = $"New follow-up created for {followUp.OrganizationName}: {followUp.Title}",
                    CreatedAt = DateTime.Now,
                    Status = AlertStatus.New
                };
                _context.Alerts.Add(alert);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(followUp);
        }

        // GET: FollowUps/Edit/5 - Retrieves a specific record for editing
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var followUp = await _context.FollowUps.FindAsync(id);
            if (followUp == null) return NotFound();
            return View(followUp);
        }

        // POST: FollowUps/Edit/5 - Updates record and triggers alerts for overdue status
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,OrganizationName,ContactPerson,DueDate,Status,AssignedEmployeeId")] FollowUp followUp)
        {
            if (id != followUp.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(followUp);

                    // 4. STATUS MONITORING: Create a critical alert if manually changed to Overdue
                    if (followUp.Status == FollowUpStatus.Overdue)
                    {
                        var overdueAlert = new Alert
                        {
                            FollowUpId = followUp.Id,
                            Message = $"CRITICAL: Follow-up '{followUp.Title}' is now marked as OVERDUE.",
                            CreatedAt = DateTime.Now,
                            Status = AlertStatus.New
                        };
                        _context.Alerts.Add(overdueAlert);
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FollowUpExists(followUp.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(followUp);
        }

        // GET: FollowUps/Delete/5 - Confirmation page for removal
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var followUp = await _context.FollowUps.FirstOrDefaultAsync(m => m.Id == id);
            if (followUp == null) return NotFound();
            return View(followUp);
        }

        // POST: FollowUps/Delete/5 - Removes record and all its associated alerts
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var followUp = await _context.FollowUps.FindAsync(id);
            if (followUp != null)
            {
                // CLEANUP: Remove linked alerts to prevent "orphaned" records in the database
                var linkedAlerts = _context.Alerts.Where(a => a.FollowUpId == id);
                _context.Alerts.RemoveRange(linkedAlerts);

                _context.FollowUps.Remove(followUp);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FollowUpExists(int id)
        {
            return _context.FollowUps.Any(e => e.Id == id);
        }
    }
}