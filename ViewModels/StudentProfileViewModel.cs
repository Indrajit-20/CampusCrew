using System.ComponentModel.DataAnnotations;

namespace CampusCrew.ViewModels;

public class StudentProfileViewModel
{
    [Required, StringLength(80, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string College { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Course { get; set; } = string.Empty;

    [Range(1, 6)]
    public int Year { get; set; } = 1;

    [Required, StringLength(80)]
    public string Location { get; set; } = string.Empty;

    [Required, StringLength(600, MinimumLength = 30)]
    public string Bio { get; set; } = string.Empty;

    [StringLength(600)]
    public string Experience { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Availability { get; set; } = string.Empty;

    [Url, StringLength(300)]
    [Display(Name = "Portfolio or social link")]
    public string SocialLink { get; set; } = string.Empty;

    public List<string> SelectedSkills { get; set; } = new();
}
