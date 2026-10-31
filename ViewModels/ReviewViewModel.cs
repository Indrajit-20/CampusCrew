using System.ComponentModel.DataAnnotations;

namespace CampusCrew.ViewModels;

public class ReviewViewModel
{
    [Required]
    public int RevieweeStudentId { get; set; }

    [Required, Range(1, 5)]
    public int Rating { get; set; }

    [Required, StringLength(800, MinimumLength = 5)]
    public string Comment { get; set; } = string.Empty;

    public string? RelatedEntityTitle { get; set; }
    public int? RelatedOpportunityId { get; set; }
    public int? RelatedCollaborationId { get; set; }
    public string? ReturnUrl { get; set; }
}
