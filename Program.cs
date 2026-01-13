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

// 2. IDENTITY: Updated to support Roles
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>() // ADD THIS: Enables the Admin role system
    .AddEntityFrameworkStores<ApplicationDbContext>();

// 3. UI & PAGES
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// 4. SIGNALR
builder.Services.AddSignalR();

// 5. BACKGROUND SERVICE
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

// 6. SECURITY: Identifies user and then checks their Role (Admin/User)
app.UseAuthentication();
app.UseAuthorization();

// 7. ENDPOINTS
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<NotificationHub>("/notificationHub");
app.MapRazorPages();

// 8. SEED ADMIN DATA: Automatically creates the Admin role and assigns it to you
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedAdminUser(services);
}

app.Run();

// --- SEED METHOD ---
// This runs every time the app starts to ensure you have Admin access
async Task SeedAdminUser(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // Create Admin Role if it doesn't exist
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // Assign Admin Role to a specific email
    var adminUser = await userManager.FindByEmailAsync("Asaad@Tuwaiq.com");
    if (adminUser != null && !await userManager.IsInRoleAsync(adminUser, "Admin"))
    {
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}