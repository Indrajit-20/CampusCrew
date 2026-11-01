using CampusCrew.Models;
using System.ComponentModel.DataAnnotations;

namespace CampusCrew.ViewModels;

public class CollaborationRequestViewModel
{
    [Required]
    public int ReceiverId { get; set; }

    public string ReceiverName { get; set; } = string.Empty;
    public string ReceiverSkills { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please give your collaboration a project title.")]
    [StringLength(100, MinimumLength = 3)]
    [Display(Name = "Project Title")]
    public string ProjectTitle { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please write a message.")]
    [StringLength(1000, MinimumLength = 10)]
    [Display(Name = "Message")]
    public string Message { get; set; } = string.Empty;
}

public class CollaborationRequestsHubViewModel
{
    public List<Collaboration> ReceivedRequests { get; set; } = new();
    public List<Collaboration> SentRequests { get; set; } = new();
    public string ActiveTab { get; set; } = "received";
}
