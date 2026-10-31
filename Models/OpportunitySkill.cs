using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusCrew.Models;

public class OpportunitySkill
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int OpportunityId { get; set; }

    [ForeignKey(nameof(OpportunityId))]
    public virtual Opportunity Opportunity { get; set; } = null!;

    [Required]
    public int SkillId { get; set; }

    [ForeignKey(nameof(SkillId))]
    public virtual Skill Skill { get; set; } = null!;
}
