using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusCrew.Models;

public class Collaboration
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int RequesterId { get; set; }

    [ForeignKey(nameof(RequesterId))]
    public virtual Student? Requester { get; set; }

    [Required]
    public int ReceiverId { get; set; }

    [ForeignKey(nameof(ReceiverId))]
    public virtual Student? Receiver { get; set; }

    [MaxLength(150)]
    public string ProjectTitle { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public StatusType Status { get; set; } = StatusType.Pending;
}

