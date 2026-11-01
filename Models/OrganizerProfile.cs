using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusCrew.Models;

public class OrganizerProfile
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }

    [Required]
    [MaxLength(100)]
    public string OrganizationName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string OrganizationType { get; set; } = string.Empty;

    [MaxLength(150)]
    public string College { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(20)]
    public string ContactNumber { get; set; } = string.Empty;
}

