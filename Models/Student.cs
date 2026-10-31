using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusCrew.Models;

public class Student
{
    [Key]
    public int Id { get; set; }

    public int? UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(150)]
    public string College { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Course { get; set; } = string.Empty;

    public int Year { get; set; }

    [MaxLength(100)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Bio { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Experience { get; set; } = string.Empty;

    [MaxLength(150)]
    public string Availability { get; set; } = string.Empty;

    public double Rating { get; set; } = 5.0;

    public int TotalReviews { get; set; } = 0;

    // Navigation properties
    public virtual List<StudentSkill> StudentSkills { get; set; } = new();
    public virtual List<Portfolio> Portfolios { get; set; } = new();
    public virtual List<Application> Applications { get; set; } = new();
    public virtual List<Collaboration> SentCollaborations { get; set; } = new();
    public virtual List<Collaboration> ReceivedCollaborations { get; set; } = new();
    public virtual List<Review> Reviews { get; set; } = new();

    // Helper property for views
    [NotMapped]
    public List<string> Skills
    {
        get => StudentSkills?.Where(s => s.Skill != null).Select(s => s.Skill.Name).ToList() ?? new();
        set { }
    }
}

