using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusCrew.Models;

public class StudentSkill
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int StudentId { get; set; }

    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;

    [Required]
    public int SkillId { get; set; }

    [ForeignKey(nameof(SkillId))]
    public virtual Skill Skill { get; set; } = null!;
}
