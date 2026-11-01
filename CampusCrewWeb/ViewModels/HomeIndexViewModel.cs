using CampusCrew.Models;

namespace CampusCrew.ViewModels;

public class HomeIndexViewModel
{
    public List<Opportunity> FeaturedOpportunities { get; set; } = new();
    public List<Student> FeaturedStudents { get; set; } = new();
    public List<Skill> Skills { get; set; } = new();
    public int TotalStudents { get; set; }
    public int TotalOpportunities { get; set; }
    public int TotalSkills { get; set; }
    public int TotalCollaborations { get; set; }
}
