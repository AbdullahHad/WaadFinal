namespace Waads.Models
{
    public class EmployeeDashboardViewModel
    {
        // Metric Counts
        public int PendingCount { get; set; }
        public int OverdueCount { get; set; }
        public int CompletedCount { get; set; }
        public int NewAlertsCount { get; set; }

        // Data Lists for the Read-Only Tables
        public List<FollowUp> RecentFollowUps { get; set; } = new();
        public List<Alert> RecentAlerts { get; set; } = new();
    }
}