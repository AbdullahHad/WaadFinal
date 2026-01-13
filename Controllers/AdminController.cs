using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Waads.Data;
using Waads.Models;

namespace Waads.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 1. GLOBAL DASHBOARD: View every Waad in the system
        public async Task<IActionResult> Index()
        {
            var allFollowUps = await _context.FollowUps
                .Include(f => f.User) // Now works with the navigation property
                .OrderByDescending(f => f.DueDate) // Matches your model
                .ToListAsync();

            return View(allFollowUps);
        }

        // 2. USER MANAGEMENT: List all registered employees
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        // 3. USER DRILL-DOWN: See Waads for a specific person
        public async Task<IActionResult> UserDetails(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var userFollowUps = await _context.FollowUps
                .Where(f => f.AssignedEmployeeId == id)
                .OrderByDescending(f => f.DueDate)
                .ToListAsync();

            ViewBag.UserName = user.Email;
            return View(userFollowUps);
        }

        // 4. ADMIN OVERRIDE: Manually change status
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, FollowUpStatus newStatus)
        {
            var followUp = await _context.FollowUps.FindAsync(id);
            if (followUp != null)
            {
                followUp.Status = newStatus;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}