using CampusCrew.Models;

namespace CampusCrew.ViewModels;

public class StudentDashboardViewModel
{
    public Student Profile { get; set; } = null!;
    public int PortfolioCount { get; set; }
    public int TotalApplications { get; set; }
    public int PendingApplications { get; set; }
    public int AcceptedApplications { get; set; }
    public int CompletedCollaborations { get; set; }
    public int PendingCollaborationRequests { get; set; }
    public List<StudentApplicationItemViewModel> RecentApplications { get; set; } = new();
}
