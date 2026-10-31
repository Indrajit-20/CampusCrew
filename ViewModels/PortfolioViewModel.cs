using System.ComponentModel.DataAnnotations;

namespace CampusCrew.ViewModels;

public class PortfolioViewModel
{
    [Required, StringLength(100)]
    [Display(Name = "Project title")]
    public string ProjectTitle { get; set; } = string.Empty;

    [StringLength(600)]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    [StringLength(80)]
    [Display(Name = "Skill")]
    public string SkillName { get; set; } = string.Empty;

    [Url, StringLength(300)]
    [Display(Name = "Image URL")]
    public string ImageUrl { get; set; } = string.Empty;

    [Url, StringLength(300)]
    [Display(Name = "External link")]
    public string ExternalLink { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Project date")]
    public DateTime ProjectDate { get; set; } = DateTime.Today;

    [StringLength(80)]
    [Display(Name = "Your role")]
    public string Role { get; set; } = string.Empty;
}
