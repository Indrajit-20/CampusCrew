using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusCrew.Models;

public class Opportunity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public DateTime OpportunityDate { get; set; }

    [MaxLength(100)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(150)]
    public string College { get; set; } = string.Empty;

    public int NumberOfParticipants { get; set; }

    public DateTime ApplicationDeadline { get; set; }

    public OpportunityType Type { get; set; }

    [MaxLength(1000)]
    public string AdditionalRequirements { get; set; } = string.Empty;

    // Foreign Key to Organizer (User)
    public int? OrganizerId { get; set; }

    [ForeignKey(nameof(OrganizerId))]
    public virtual User? Organizer { get; set; }

    // Navigation properties
    public virtual List<OpportunitySkill> OpportunitySkills { get; set; } = new();
    public virtual List<Application> Applications { get; set; } = new();
    public virtual List<Review> Reviews { get; set; } = new();

    // Helper property for views
    private List<string> _requiredSkills = new();

    [NotMapped]
    public List<string> RequiredSkills
    {
        get
        {
            if (OpportunitySkills != null && OpportunitySkills.Any())
            {
                return OpportunitySkills.Where(s => s.Skill != null).Select(s => s.Skill.Name).ToList();
            }
            return _requiredSkills;
        }
        set => _requiredSkills = value ?? new();
    }
}
