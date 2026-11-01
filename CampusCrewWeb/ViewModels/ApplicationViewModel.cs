using System.ComponentModel.DataAnnotations;

namespace CampusCrew.ViewModels;

public class ApplicationViewModel
{
    public int OpportunityId { get; set; }

    [Required, StringLength(800, MinimumLength = 20)]
    [Display(Name = "Why are you a good fit?")]
    public string Message { get; set; } = string.Empty;
}
