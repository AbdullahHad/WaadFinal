using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Waads.Data;
using Waads.Models;

namespace Waads.Controllers
{
    public class AlertsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AlertsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Alerts
        public async Task<IActionResult> Index()
        {
            // Requirement: Track alerts and monitor status over time
            var alerts = await _context.Alerts
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
            return View(alerts);
        }

        // POST: Alerts/Acknowledge/5
        [HttpPost]
        public async Task<IActionResult> Acknowledge(int id)
        {
            var alert = await _context.Alerts.FindAsync(id);
            if (alert != null)
            {
                alert.Status = AlertStatus.Acknowledged;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}