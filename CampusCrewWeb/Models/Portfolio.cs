using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusCrew.Models;

public class Portfolio
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int StudentId { get; set; }

    [ForeignKey(nameof(StudentId))]
    public virtual Student? Student { get; set; }

    [Required]
    [MaxLength(150)]
    public string ProjectTitle { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(100)]
    public string SkillName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    [MaxLength(500)]
    public string ExternalLink { get; set; } = string.Empty;

    public DateTime ProjectDate { get; set; }

    [MaxLength(100)]
    public string Role { get; set; } = string.Empty;
}

