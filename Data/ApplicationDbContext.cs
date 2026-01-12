using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Waads.Models;

namespace Waads.Data;

// IdentityDbContext gives you the Login/User tables automatically
public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // These lines create your specific Waads tables in SQL Server
    public DbSet<FollowUp> FollowUps { get; set; }
    public DbSet<Alert> Alerts { get; set; }
}