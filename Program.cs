using Microsoft.EntityFrameworkCore;
using Waads.Data;
using Microsoft.AspNetCore.Identity;
using Waads.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. DATABASE: Connect to SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. IDENTITY: Setup Login/Register
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// 3. UI & PAGES: Register MVC and Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// 4. SIGNALR: Register real-time services
// This MUST be registered so the Background Service can find 'IHubContext'
builder.Services.AddSignalR();

// 5. BACKGROUND SERVICE: Register the OverdueTaskService
// The Background Service now has access to both the Database and the SignalR Hub
builder.Services.AddHostedService<OverdueTaskService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 6. SECURITY: Order matters for Identity
app.UseAuthentication();
app.UseAuthorization();

// 7. ENDPOINTS: Map MVC, SignalR, and Razor Pages
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map the real-time bridge for the pop-ups
app.MapHub<NotificationHub>("/notificationHub");

app.MapRazorPages();

app.Run();