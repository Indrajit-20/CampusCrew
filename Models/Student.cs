using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusCrew.Models;

public class Student
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }

    [MaxLength(150)]
    public string College { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Course { get; set; } = string.Empty;

    public int Year { get; set; }

    [MaxLength(100)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Bio { get; set; } = string.Empty;

    [MaxLength(150)]
    public string Availability { get; set; } = string.Empty;

    // Navigation properties
    public virtual List<StudentSkill> StudentSkills { get; set; } = new();
    public virtual List<Portfolio> Portfolios { get; set; } = new();
    public virtual List<Application> Applications { get; set; } = new();
    public virtual List<Collaboration> SentCollaborations { get; set; } = new();
    public virtual List<Collaboration> ReceivedCollaborations { get; set; } = new();

    // Convenience properties for views (Delegated to User entity, NOT duplicated in DB)
    [NotMapped]
    public string Name => User?.Name ?? string.Empty;

    [NotMapped]
    public string Email => User?.Email ?? string.Empty;

    // Convenience property for views (Not stored in database)
    [NotMapped]
    public List<string> Skills => StudentSkills?.Where(s => s.Skill != null).Select(s => s.Skill!.Name).ToList() ?? new();
}
