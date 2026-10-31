using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusCrew.Models;

public class Review
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ReviewerId { get; set; }

    [ForeignKey(nameof(ReviewerId))]
    public virtual User? Reviewer { get; set; }

    [MaxLength(100)]
    public string ReviewerName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string ReviewerRole { get; set; } = string.Empty;

    [Required]
    public int StudentId { get; set; }

    [ForeignKey(nameof(StudentId))]
    public virtual Student? Student { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(1000)]
    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(150)]
    public string RelatedEntityTitle { get; set; } = string.Empty;

    public int? OpportunityId { get; set; }

    [ForeignKey(nameof(OpportunityId))]
    public virtual Opportunity? Opportunity { get; set; }

    public int? CollaborationId { get; set; }

    [ForeignKey(nameof(CollaborationId))]
    public virtual Collaboration? Collaboration { get; set; }
}
