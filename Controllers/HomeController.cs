using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Waads.Data;
using Waads.Models;
[Authorize] // This ensures only logged-in employees can access follow-ups
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Get current logged-in user ID
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // If not logged in, show the default welcome or redirect
        if (string.IsNullOrEmpty(userId)) return View(new EmployeeDashboardViewModel());

        var model = new EmployeeDashboardViewModel
        {
            PendingCount = await _context.FollowUps.CountAsync(f => f.AssignedEmployeeId == userId && f.Status == FollowUpStatus.Pending),
            OverdueCount = await _context.FollowUps.CountAsync(f => f.AssignedEmployeeId == userId && f.Status == FollowUpStatus.Overdue),
            CompletedCount = await _context.FollowUps.CountAsync(f => f.AssignedEmployeeId == userId && f.Status == FollowUpStatus.Completed),
            NewAlertsCount = await _context.Alerts.CountAsync(a => _context.FollowUps.Any(f => f.Id == a.FollowUpId && f.AssignedEmployeeId == userId) && a.Status == AlertStatus.New),

            RecentFollowUps = await _context.FollowUps
                .Where(f => f.AssignedEmployeeId == userId)
                .OrderByDescending(f => f.DueDate)
                .Take(5).ToListAsync(),

            RecentAlerts = await _context.Alerts
                .Where(a => _context.FollowUps.Any(f => f.Id == a.FollowUpId && f.AssignedEmployeeId == userId))
                .OrderByDescending(a => a.CreatedAt)
                .Take(5).ToListAsync()
        };

        return View(model);
    }
}