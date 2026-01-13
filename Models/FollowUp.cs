using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity; // Add this

namespace Waads.Models
{
    public enum FollowUpStatus { Pending, Overdue, Completed }

    public class FollowUp
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [Required]
        public string OrganizationName { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        [Required]
        public DateTime DueDate { get; set; }
        public FollowUpStatus Status { get; set; } = FollowUpStatus.Pending;
        public string? AssignedEmployeeId { get; set; }

        // Add this to link to the User details in the database
        public virtual IdentityUser? User { get; set; }
    }
}