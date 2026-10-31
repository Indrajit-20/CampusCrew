using CampusCrew.Models;

namespace CampusCrew.ViewModels;

public class StudentApplicationItemViewModel
{
    public int ApplicationId { get; set; }
    public int OpportunityId { get; set; }
    public string OpportunityTitle { get; set; } = string.Empty;
    public string OrganizerName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime OpportunityDate { get; set; }
    public DateTime ApplicationDeadline { get; set; }
    public DateTime AppliedOn { get; set; }
    public string Message { get; set; } = string.Empty;
    public StatusType Status { get; set; }
    public OpportunityType OpportunityType { get; set; }
    public bool HasReviewed { get; set; }
}

