using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Waads.Data;
using Waads.Models;

namespace Waads.Controllers
{
    [Authorize]
    public class FollowUpsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FollowUpsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. FILTERED INDEX: Only show follow-ups belonging to the logged-in user
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var myFollowUps = await _context.FollowUps
                .Where(f => f.AssignedEmployeeId == userId)
                .OrderByDescending(f => f.DueDate)
                .ToListAsync();

            return View(myFollowUps);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,OrganizationName,ContactPerson,DueDate,Status")] FollowUp followUp)
        {
            if (ModelState.IsValid)
            {
                // Capture current user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                followUp.AssignedEmployeeId = userId;

                _context.Add(followUp);
                await _context.SaveChangesAsync();

                // Create initial alert
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

        // 2. SECURE EDIT: Ensure the user owns the record they are trying to edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var followUp = await _context.FollowUps
                .FirstOrDefaultAsync(f => f.Id == id && f.AssignedEmployeeId == userId);

            if (followUp == null) return Forbid(); // Security: Block unauthorized access

            return View(followUp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,OrganizationName,ContactPerson,DueDate,Status,AssignedEmployeeId")] FollowUp followUp)
        {
            if (id != followUp.Id) return NotFound();

            // Verify ownership again before saving changes
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (followUp.AssignedEmployeeId != userId) return Forbid();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(followUp);

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

        // 3. SECURE DELETE: Prevent users from deleting others' data via URL
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var followUp = await _context.FollowUps
                .FirstOrDefaultAsync(m => m.Id == id && m.AssignedEmployeeId == userId);

            if (followUp == null) return Forbid();

            return View(followUp);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var followUp = await _context.FollowUps
                .FirstOrDefaultAsync(f => f.Id == id && f.AssignedEmployeeId == userId);

            if (followUp != null)
            {
                var linkedAlerts = _context.Alerts.Where(a => a.FollowUpId == id);
                _context.Alerts.RemoveRange(linkedAlerts);
                _context.FollowUps.Remove(followUp);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool FollowUpExists(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return _context.FollowUps.Any(e => e.Id == id && e.AssignedEmployeeId == userId);
        }
    }
}