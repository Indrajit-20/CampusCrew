namespace CampusCrew.ViewModels;

public class StudentFilterViewModel
{
    public string Search { get; set; } = string.Empty;
    public string Skill { get; set; } = string.Empty;
    public string College { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Availability { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = new();
    public List<CampusCrew.Models.Student> Students { get; set; } = new();
}
