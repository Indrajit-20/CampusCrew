using System.ComponentModel.DataAnnotations;

namespace CampusCrew.Models;

public class Skill
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(80)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Category { get; set; }

    // Navigation properties
    public virtual List<StudentSkill> StudentSkills { get; set; } = new();
    public virtual List<OpportunitySkill> OpportunitySkills { get; set; } = new();
}
