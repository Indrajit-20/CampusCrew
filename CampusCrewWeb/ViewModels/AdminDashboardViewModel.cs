using CampusCrew.Models;

namespace CampusCrew.ViewModels;

public class AdminDashboardViewModel
{
    public List<Student> Students { get; set; } = new();
    public List<User> Users { get; set; } = new();
    public List<Opportunity> Opportunities { get; set; } = new();
    public List<Skill> Skills { get; set; } = new();
    public List<Collaboration> Collaborations { get; set; } = new();
}
