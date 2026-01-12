using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Waads.Data;
using Waads.Models;

namespace Waads.Services
{
    public class OverdueTaskService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<NotificationHub> _hubContext; // Added for pop-ups

        public OverdueTaskService(IServiceProvider serviceProvider, IHubContext<NotificationHub> hubContext)
        {
            _serviceProvider = serviceProvider;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var now = DateTime.Now;

                    // 1. Logic: Find tasks that just became overdue
                    var overdueTasks = await context.FollowUps
                        .Where(f => f.DueDate <= now && f.Status == FollowUpStatus.Pending)
                        .ToListAsync();

                    foreach (var task in overdueTasks)
                    {
                        task.Status = FollowUpStatus.Overdue;

                        // 2. Create the database alert record
                        var alert = new Alert
                        {
                            FollowUpId = task.Id,
                            Message = $"SYSTEM: Commitment '{task.Title}' is now OVERDUE.",
                            CreatedAt = now,
                            Status = AlertStatus.New
                        };
                        context.Alerts.Add(alert);

                        // 3. SIGNALR POP-UP: Send a real-time message to the specific user
                        // task.AssignedEmployeeId ensures the pop-up only goes to the right person
                        await _hubContext.Clients.User(task.AssignedEmployeeId)
                            .SendAsync("ReceiveNotification", $"CRITICAL: '{task.Title}' is now overdue!", stoppingToken);
                    }

                    if (overdueTasks.Any())
                    {
                        await context.SaveChangesAsync();
                    }
                }

                // Polling interval: checking every 1 minute
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}