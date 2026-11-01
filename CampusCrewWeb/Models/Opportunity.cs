using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusCrew.Models;

public class Opportunity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int OrganizerId { get; set; }

    [ForeignKey(nameof(OrganizerId))]
    public virtual User? Organizer { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public OpportunityType OpportunityType { get; set; } = OpportunityType.Collaboration;

    [MaxLength(100)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(150)]
    public string College { get; set; } = string.Empty;

    public int RequiredMembers { get; set; } = 1;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime Deadline { get; set; } = DateTime.Today.AddDays(7);

    public DateTime OpportunityDate { get; set; } = DateTime.Today.AddDays(14);

    public StatusType Status { get; set; } = StatusType.Accepted;

    [MaxLength(1000)]
    public string AdditionalRequirements { get; set; } = string.Empty;

    // Navigation properties
    public virtual List<OpportunitySkill> OpportunitySkills { get; set; } = new();
    public virtual List<Application> Applications { get; set; } = new();
}
