using Microsoft.EntityFrameworkCore;
using Waads.Data;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// 1. DATABASE CONNECTION: Retrieve the connection string for SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// 2. CONTEXT REGISTRATION: Connects EF Core to your database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3. IDENTITY SERVICES: Setup the core Login/Register logic
// SignIn.RequireConfirmedAccount = false allows immediate login after registration for testing
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// 4. UI SERVICES: Enable both Controllers (for Dashboard) and Razor Pages (for Login)
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

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

// 5. SECURITY PIPELINE: Order is critical here
app.UseAuthentication(); // First, identify WHO the user is
app.UseAuthorization();  // Second, check WHAT the user can access

// 6. ROUTING: Directs traffic to your Home/Dashboard by default
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 7. IDENTITY MAPPING: Connects the hidden Identity Login/Register pages
app.MapRazorPages();

app.Run();