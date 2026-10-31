using System.ComponentModel.DataAnnotations;
using CampusCrew.Models;

namespace CampusCrew.ViewModels;

public class OpportunityFormViewModel
{
    [Required, StringLength(120, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(1200, MinimumLength = 30)]
    public string Description { get; set; } = string.Empty;

    [Required, StringLength(250)]
    [Display(Name = "Required skills")]
    public string RequiredSkills { get; set; } = string.Empty;

    [Required, DataType(DataType.Date)]
    [Display(Name = "Opportunity date")]
    public DateTime OpportunityDate { get; set; } = DateTime.Today.AddDays(14);

    [Required, StringLength(100)]
    public string Location { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string College { get; set; } = string.Empty;

    [Range(1, 500)]
    [Display(Name = "Number of participants")]
    public int NumberOfParticipants { get; set; } = 1;

    [Required, DataType(DataType.Date)]
    [Display(Name = "Application deadline")]
    public DateTime ApplicationDeadline { get; set; } = DateTime.Today.AddDays(7);

    [Required]
    public OpportunityType Type { get; set; } = OpportunityType.Collaboration;

    [StringLength(600)]
    [Display(Name = "Additional requirements")]
    public string AdditionalRequirements { get; set; } = string.Empty;
}
