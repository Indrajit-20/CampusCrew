using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusCrew.Models;

public class Application
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int StudentId { get; set; }

    [ForeignKey(nameof(StudentId))]
    public virtual Student? Student { get; set; }

    [Required]
    public int OpportunityId { get; set; }

    [ForeignKey(nameof(OpportunityId))]
    public virtual Opportunity? Opportunity { get; set; }

    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    public StatusType Status { get; set; } = StatusType.Pending;

    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
}
