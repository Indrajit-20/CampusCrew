namespace CampusCrew.ViewModels;

public class MyApplicationsViewModel
{
    public List<StudentApplicationItemViewModel> Applications { get; set; } = new();
    public int PendingCount => Applications.Count(a => a.Status == Models.StatusType.Pending);
    public int AcceptedCount => Applications.Count(a => a.Status == Models.StatusType.Accepted);
    public int RejectedCount => Applications.Count(a => a.Status == Models.StatusType.Rejected);
    public int CompletedCount => Applications.Count(a => a.Status == Models.StatusType.Completed);
}
