namespace Waads.Models
{
    public enum AlertStatus { New, Acknowledged, Resolved }

    public class Alert
    {
        public int Id { get; set; }
        public int FollowUpId { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public AlertStatus Status { get; set; } = AlertStatus.New;
    }
}