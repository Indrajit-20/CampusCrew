using CampusCrew.Models;

namespace CampusCrew.Dtos;

public class OpportunityDto
{
    public int Id { get; set; }
    public int OrganizerId { get; set; }
    public string OrganizerName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public OpportunityType OpportunityType { get; set; }
    public string Location { get; set; } = string.Empty;
    public string College { get; set; } = string.Empty;
    public int RequiredMembers { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime Deadline { get; set; }
    public DateTime OpportunityDate { get; set; }
    public StatusType Status { get; set; }
    public string AdditionalRequirements { get; set; } = string.Empty;
    public List<string> RequiredSkills { get; set; } = new();
    public int ApplicationCount { get; set; }
}

public class CreateOpportunityDto
{
    public int OrganizerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public OpportunityType OpportunityType { get; set; } = OpportunityType.Collaboration;
    public string Location { get; set; } = string.Empty;
    public string College { get; set; } = string.Empty;
    public int RequiredMembers { get; set; } = 1;
    public DateTime Deadline { get; set; } = DateTime.Today.AddDays(7);
    public DateTime OpportunityDate { get; set; } = DateTime.Today.AddDays(14);
    public string AdditionalRequirements { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = new();
}

