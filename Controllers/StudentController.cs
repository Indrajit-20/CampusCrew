using CampusCrew.Data;
using CampusCrew.Models;
using CampusCrew.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusCrew.Controllers;

public class StudentController : Controller
{
    private readonly ApplicationDbContext _context;

    public StudentController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Dashboard()
    {
        if (!IsStudentLoggedIn()) return RedirectToLogin();
        var student = GetActiveStudent();

        var studentApps = _context.Applications
            .Include(a => a.Opportunity)
                .ThenInclude(o => o!.Organizer)
            .Where(a => a.StudentId == student.Id)
            .OrderByDescending(a => a.AppliedOn)
            .ToList();

        var recentApps = studentApps.Take(4).Select(a => new StudentApplicationItemViewModel
        {
            ApplicationId = a.Id,
            OpportunityId = a.OpportunityId,
            OpportunityTitle = a.Opportunity?.Title ?? "Opportunity",
            OrganizerName = a.Opportunity?.Organizer?.Name ?? "Organizer",
            Location = a.Opportunity?.Location ?? "",
            OpportunityDate = a.Opportunity?.OpportunityDate ?? DateTime.Today,
            ApplicationDeadline = a.Opportunity?.ApplicationDeadline ?? DateTime.Today,
            AppliedOn = a.AppliedOn,
            Message = a.Message,
            Status = a.Status,
            OpportunityType = a.Opportunity?.Type ?? OpportunityType.Collaboration,
            HasReviewed = _context.Reviews.Any(r => r.ReviewerId == (student.UserId ?? student.Id) && r.OpportunityId == a.OpportunityId)
        }).ToList();

        var receivedRequests = _context.Collaborations
            .Where(c => c.ReceiverId == student.Id)
            .ToList();

        var model = new StudentDashboardViewModel
        {
            Profile = student,
            PortfolioCount = _context.Portfolios.Count(p => p.StudentId == student.Id),
            TotalApplications = studentApps.Count,
            PendingApplications = studentApps.Count(x => x.Status == StatusType.Pending),
            AcceptedApplications = studentApps.Count(x => x.Status == StatusType.Accepted),
            CompletedCollaborations = studentApps.Count(x => x.Status == StatusType.Completed),
            AverageRating = student.Rating,
            TotalReviews = student.TotalReviews,
            PendingCollaborationRequests = receivedRequests.Count(x => x.Status == StatusType.Pending),
            RecentApplications = recentApps
        };

        return View(model);
    }

    public IActionResult Profile()
    {
        if (!IsStudentLoggedIn()) return RedirectToLogin();
        var student = GetActiveStudent();

        ViewBag.Reviews = _context.Reviews
            .Where(r => r.StudentId == student.Id)
            .OrderByDescending(r => r.CreatedAt)
            .ToList();
        ViewBag.IsSelf = true;
        return View(student);
    }

    [HttpGet]
    public IActionResult EditProfile()
    {
        if (!IsStudentLoggedIn()) return RedirectToLogin();
        var student = GetActiveStudent();

        var selectedSkills = student.StudentSkills
            .Where(ss => ss.Skill != null)
            .Select(ss => ss.Skill.Name)
            .ToList();

        return View(new StudentProfileViewModel
        {
            Name = student.Name,
            College = student.College,
            Course = student.Course,
            Year = student.Year,
            Location = student.Location,
            Bio = student.Bio,
            Experience = student.Experience,
            Availability = student.Availability,
            SelectedSkills = selectedSkills
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditProfile(StudentProfileViewModel model)
    {
        if (!IsStudentLoggedIn()) return RedirectToLogin();
        if (!ModelState.IsValid)
        {
            model.SelectedSkills ??= new();
            return View(model);
        }

        var student = GetActiveStudent();
        student.Name = model.Name.Trim();
        student.College = model.College.Trim();
        student.Course = model.Course.Trim();
        student.Year = model.Year;
        student.Location = model.Location.Trim();
        student.Bio = model.Bio.Trim();
        student.Experience = model.Experience.Trim();
        student.Availability = model.Availability.Trim();

        // Update User name if linked
        if (student.UserId.HasValue)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == student.UserId.Value);
            if (user != null)
            {
                user.Name = student.Name;
                HttpContext.Session.SetString("DisplayName", student.Name);
            }
        }

        // Update skills
        var existingSkills = _context.StudentSkills.Where(ss => ss.StudentId == student.Id).ToList();
        _context.StudentSkills.RemoveRange(existingSkills);

        if (model.SelectedSkills != null)
        {
            foreach (var skillName in model.SelectedSkills.Distinct())
            {
                var skill = _context.Skills.FirstOrDefault(s => s.Name == skillName);
                if (skill != null)
                {
                    _context.StudentSkills.Add(new StudentSkill
                    {
                        StudentId = student.Id,
                        SkillId = skill.Id
                    });
                }
            }
        }

        _context.SaveChanges();
        TempData["Success"] = "Profile updated successfully.";
        return RedirectToAction(nameof(Profile));
    }

    public IActionResult MyApplications()
    {
        if (!IsStudentLoggedIn()) return RedirectToLogin();
        var student = GetActiveStudent();

        var apps = _context.Applications
            .Include(a => a.Opportunity)
                .ThenInclude(o => o!.Organizer)
            .Where(a => a.StudentId == student.Id)
            .OrderByDescending(a => a.AppliedOn)
            .Select(a => new StudentApplicationItemViewModel
            {
                ApplicationId = a.Id,
                OpportunityId = a.OpportunityId,
                OpportunityTitle = a.Opportunity != null ? a.Opportunity.Title : "Opportunity",
                OrganizerName = a.Opportunity != null && a.Opportunity.Organizer != null ? a.Opportunity.Organizer.Name : "Organizer",
                Location = a.Opportunity != null ? a.Opportunity.Location : "",
                OpportunityDate = a.Opportunity != null ? a.Opportunity.OpportunityDate : DateTime.Today,
                ApplicationDeadline = a.Opportunity != null ? a.Opportunity.ApplicationDeadline : DateTime.Today,
                AppliedOn = a.AppliedOn,
                Message = a.Message,
                Status = a.Status,
                OpportunityType = a.Opportunity != null ? a.Opportunity.Type : OpportunityType.Collaboration,
                HasReviewed = _context.Reviews.Any(r => r.ReviewerId == (student.UserId ?? student.Id) && r.OpportunityId == a.OpportunityId)
            })
            .ToList();

        return View(new MyApplicationsViewModel { Applications = apps });
    }

    public IActionResult CollaborationRequests(string tab = "received")
    {
        if (!IsStudentLoggedIn()) return RedirectToLogin();
        var student = GetActiveStudent();

        var received = _context.Collaborations
            .Include(c => c.Requester)
            .Where(c => c.ReceiverId == student.Id)
            .OrderByDescending(c => c.CreatedAt)
            .ToList();

        var sent = _context.Collaborations
            .Include(c => c.Receiver)
            .Where(c => c.RequesterId == student.Id)
            .OrderByDescending(c => c.CreatedAt)
            .ToList();

        var hub = new CollaborationRequestsHubViewModel
        {
            ReceivedRequests = received,
            SentRequests = sent,
            ActiveTab = tab.ToLowerInvariant() == "sent" ? "sent" : "received"
        };

        return View(hub);
    }

    [HttpGet]
    public IActionResult RequestCollaboration(int id)
    {
        if (!IsStudentLoggedIn()) return RedirectToLogin();
        var targetStudent = _context.Students
            .Include(s => s.StudentSkills).ThenInclude(ss => ss.Skill)
            .FirstOrDefault(s => s.Id == id);

        if (targetStudent is null) return NotFound();

        var activeStudent = GetActiveStudent();
        if (targetStudent.Id == activeStudent.Id)
        {
            TempData["Info"] = "You cannot send a collaboration request to yourself.";
            return RedirectToAction(nameof(Profile));
        }

        var model = new CollaborationRequestViewModel
        {
            ReceiverId = targetStudent.Id,
            ReceiverName = targetStudent.Name,
            ReceiverSkills = string.Join(", ", targetStudent.Skills)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RequestCollaboration(CollaborationRequestViewModel model)
    {
        if (!IsStudentLoggedIn()) return RedirectToLogin();
        var targetStudent = _context.Students.FirstOrDefault(s => s.Id == model.ReceiverId);
        if (targetStudent is null) return NotFound();

        var activeStudent = GetActiveStudent();
        if (targetStudent.Id == activeStudent.Id)
        {
            TempData["Info"] = "You cannot send a collaboration request to yourself.";
            return RedirectToAction(nameof(Profile));
        }

        if (!ModelState.IsValid) return View(model);

        var collab = new Collaboration
        {
            RequesterId = activeStudent.Id,
            ReceiverId = targetStudent.Id,
            ProjectTitle = model.ProjectTitle.Trim(),
            Message = model.Message.Trim(),
            CreatedAt = DateTime.UtcNow,
            Status = StatusType.Pending
        };

        _context.Collaborations.Add(collab);
        _context.SaveChanges();

        TempData["Success"] = $"Collaboration request sent to {targetStudent.Name}!";
        return RedirectToAction(nameof(CollaborationRequests), new { tab = "sent" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AcceptCollaboration(int id)
    {
        if (!IsStudentLoggedIn()) return RedirectToLogin();
        var student = GetActiveStudent();
        var req = _context.Collaborations.Include(c => c.Requester).FirstOrDefault(c => c.Id == id);
        if (req == null || req.ReceiverId != student.Id) return NotFound();

        req.Status = StatusType.Accepted;
        _context.SaveChanges();

        TempData["Success"] = $"Accepted collaboration from {req.Requester?.Name}!";
        return RedirectToAction(nameof(CollaborationRequests), new { tab = "received" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RejectCollaboration(int id)
    {
        if (!IsStudentLoggedIn()) return RedirectToLogin();
        var student = GetActiveStudent();
        var req = _context.Collaborations.Include(c => c.Requester).FirstOrDefault(c => c.Id == id);
        if (req == null || req.ReceiverId != student.Id) return NotFound();

        req.Status = StatusType.Rejected;
        _context.SaveChanges();

        TempData["Info"] = $"Declined collaboration from {req.Requester?.Name}.";
        return RedirectToAction(nameof(CollaborationRequests), new { tab = "received" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult MarkCollaborationCompleted(int id)
    {
        if (!IsStudentLoggedIn()) return RedirectToLogin();
        var student = GetActiveStudent();
        var req = _context.Collaborations.FirstOrDefault(c => c.Id == id);
        if (req == null || (req.ReceiverId != student.Id && req.RequesterId != student.Id)) return NotFound();

        req.Status = StatusType.Completed;
        _context.SaveChanges();

        TempData["Success"] = "Collaboration marked as completed!";
        return RedirectToAction(nameof(CollaborationRequests), new { tab = "received" });
    }

    public IActionResult StudentProfile(int id)
    {
        var student = _context.Students
            .Include(s => s.StudentSkills).ThenInclude(ss => ss.Skill)
            .Include(s => s.Portfolios)
            .FirstOrDefault(s => s.Id == id);

        if (student is null) return NotFound();

        var userRole = HttpContext.Session.GetString("Role");
        var activeEmail = HttpContext.Session.GetString("Email");
        var activeStudent = !string.IsNullOrEmpty(activeEmail) ? _context.Students.FirstOrDefault(s => s.Email.ToLower() == activeEmail.ToLower()) : null;

        bool isSelf = activeStudent != null && activeStudent.Id == student.Id;
        ViewBag.IsSelf = isSelf;
        ViewBag.ViewerRole = userRole;
        ViewBag.Reviews = _context.Reviews
            .Where(r => r.StudentId == student.Id)
            .OrderByDescending(r => r.CreatedAt)
            .ToList();

        // If Organizer, fetch their opportunities for invitations
        if (userRole == UserRole.Organizer.ToString())
        {
            var orgEmail = HttpContext.Session.GetString("Email");
            var orgUser = _context.Users.FirstOrDefault(u => u.Email.ToLower() == (orgEmail ?? "").ToLower());
            ViewBag.OrganizerOpportunities = orgUser != null
                ? _context.Opportunities.Where(o => o.OrganizerId == orgUser.Id).ToList()
                : new List<Opportunity>();
        }

        return View(student);
    }

    public IActionResult OpportunityDetails(int id)
    {
        var opportunity = _context.Opportunities
            .Include(o => o.OpportunitySkills).ThenInclude(os => os.Skill)
            .Include(o => o.Organizer)
            .Include(o => o.Applications)
            .FirstOrDefault(o => o.Id == id);

        if (opportunity is null) return NotFound();

        var userRole = HttpContext.Session.GetString("Role");
        var activeEmail = HttpContext.Session.GetString("Email");
        var activeStudent = (!string.IsNullOrEmpty(activeEmail) && userRole == UserRole.Student.ToString())
            ? _context.Students.FirstOrDefault(s => s.Email.ToLower() == activeEmail.ToLower())
            : null;

        ViewBag.UserRole = userRole;
        ViewBag.HasApplied = false;
        ViewBag.ExistingApplication = (Application?)null;
        ViewBag.IsDeadlinePassed = opportunity.ApplicationDeadline.Date < DateTime.Today;

        if (activeStudent != null)
        {
            var app = opportunity.Applications.FirstOrDefault(a => a.StudentId == activeStudent.Id);
            if (app != null)
            {
                ViewBag.HasApplied = true;
                ViewBag.ExistingApplication = app;
            }
        }

        return View(opportunity);
    }

    [HttpGet]
    public IActionResult AddPortfolio()
    {
        if (!IsStudentLoggedIn()) return RedirectToLogin();
        return View(new PortfolioViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddPortfolio(PortfolioViewModel model)
    {
        if (!IsStudentLoggedIn()) return RedirectToLogin();
        if (!ModelState.IsValid) return View(model);

        var student = GetActiveStudent();
        var item = new Portfolio
        {
            StudentId = student.Id,
            ProjectTitle = model.ProjectTitle.Trim(),
            Description = model.Description.Trim(),
            SkillName = model.SkillName,
            ImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl) ? "https://images.unsplash.com/photo-1517245386807-bb43f82c33c4?auto=format&fit=crop&w=800&q=80" : model.ImageUrl.Trim(),
            ExternalLink = model.ExternalLink?.Trim() ?? string.Empty,
            ProjectDate = model.ProjectDate,
            Role = model.Role.Trim()
        };

        _context.Portfolios.Add(item);
        _context.SaveChanges();

        TempData["Success"] = "Portfolio project added successfully.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    public IActionResult Apply(int id)
    {
        if (!IsStudentLoggedIn()) return RedirectToLogin();
        var opportunity = _context.Opportunities
            .Include(o => o.Applications)
            .FirstOrDefault(o => o.Id == id);

        if (opportunity is null) return NotFound();

        if (opportunity.ApplicationDeadline.Date < DateTime.Today)
        {
            TempData["Error"] = "The deadline for this opportunity has passed.";
            return RedirectToAction(nameof(OpportunityDetails), new { id });
        }

        var student = GetActiveStudent();
        if (opportunity.Applications.Any(x => x.StudentId == student.Id))
        {
            TempData["Info"] = "You have already applied for this opportunity.";
            return RedirectToAction(nameof(OpportunityDetails), new { id });
        }

        return View(new ApplicationViewModel { OpportunityId = opportunity.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Apply(ApplicationViewModel model)
    {
        if (!IsStudentLoggedIn()) return RedirectToLogin();
        var opportunity = _context.Opportunities
            .Include(o => o.Applications)
            .FirstOrDefault(o => o.Id == model.OpportunityId);

        if (opportunity is null) return NotFound();

        if (opportunity.ApplicationDeadline.Date < DateTime.Today)
        {
            TempData["Error"] = "The deadline has already passed.";
            return RedirectToAction(nameof(OpportunityDetails), new { id = opportunity.Id });
        }

        if (!ModelState.IsValid) return View(model);

        var student = GetActiveStudent();
        if (opportunity.Applications.Any(x => x.StudentId == student.Id))
        {
            TempData["Info"] = "You have already applied for this opportunity.";
            return RedirectToAction(nameof(OpportunityDetails), new { id = opportunity.Id });
        }

        var app = new Application
        {
            OpportunityId = opportunity.Id,
            StudentId = student.Id,
            Message = model.Message.Trim(),
            AppliedOn = DateTime.UtcNow,
            Status = StatusType.Pending
        };

        _context.Applications.Add(app);
        _context.SaveChanges();

        TempData["Success"] = "Your application has been submitted!";
        return RedirectToAction(nameof(MyApplications));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SubmitReview(ReviewViewModel model)
    {
        var role = HttpContext.Session.GetString("Role");
        if (string.IsNullOrEmpty(role))
        {
            TempData["Info"] = "Please log in to submit a review.";
            return RedirectToAction("Login", "Account");
        }

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fill out rating (1-5) and your comment.";
            return RedirectToAction(nameof(MyApplications));
        }

        var reviewerName = HttpContext.Session.GetString("DisplayName") ?? "Reviewer";
        var reviewerEmail = HttpContext.Session.GetString("Email") ?? string.Empty;
        int reviewerId = 0;

        if (role == UserRole.Student.ToString())
        {
            var student = _context.Students.FirstOrDefault(s => s.Email.ToLower() == reviewerEmail.ToLower());
            reviewerId = student?.UserId ?? student?.Id ?? 1;
        }
        else
        {
            var org = _context.Users.FirstOrDefault(u => u.Email.ToLower() == reviewerEmail.ToLower());
            reviewerId = org?.Id ?? 2;
        }

        // Duplicate review check
        if (model.RelatedOpportunityId.HasValue && _context.Reviews.Any(r => r.ReviewerId == reviewerId && r.OpportunityId == model.RelatedOpportunityId.Value && r.StudentId == model.RevieweeStudentId))
        {
            TempData["Info"] = "You have already submitted a review for this.";
            return RedirectToAction(nameof(MyApplications));
        }

        if (model.RelatedCollaborationId.HasValue && _context.Reviews.Any(r => r.ReviewerId == reviewerId && r.CollaborationId == model.RelatedCollaborationId.Value))
        {
            TempData["Info"] = "You have already submitted a review for this collaboration.";
            return RedirectToAction(nameof(CollaborationRequests));
        }

        var review = new Review
        {
            ReviewerId = reviewerId,
            ReviewerName = reviewerName,
            ReviewerRole = role,
            StudentId = model.RevieweeStudentId,
            Rating = Math.Clamp(model.Rating, 1, 5),
            Comment = model.Comment.Trim(),
            CreatedAt = DateTime.UtcNow,
            RelatedEntityTitle = model.RelatedEntityTitle ?? "",
            OpportunityId = model.RelatedOpportunityId,
            CollaborationId = model.RelatedCollaborationId
        };

        _context.Reviews.Add(review);
        _context.SaveChanges();

        // Recalculate student rating
        var reviewee = _context.Students.FirstOrDefault(s => s.Id == model.RevieweeStudentId);
        if (reviewee != null)
        {
            var reviews = _context.Reviews.Where(r => r.StudentId == reviewee.Id).ToList();
            reviewee.TotalReviews = reviews.Count;
            reviewee.Rating = reviews.Count > 0 ? Math.Round(reviews.Average(r => r.Rating), 1) : 5.0;
            _context.SaveChanges();
        }

        TempData["Success"] = "Your review has been published!";

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction(nameof(StudentProfile), new { id = model.RevieweeStudentId });
    }

    private Student GetActiveStudent()
    {
        var email = HttpContext.Session.GetString("Email") ?? string.Empty;
        var student = _context.Students
            .Include(s => s.StudentSkills).ThenInclude(ss => ss.Skill)
            .Include(s => s.Portfolios)
            .Include(s => s.Applications)
            .FirstOrDefault(s => s.Email.ToLower() == email.ToLower());

        if (student == null)
        {
            student = _context.Students
                .Include(s => s.StudentSkills).ThenInclude(ss => ss.Skill)
                .Include(s => s.Portfolios)
                .Include(s => s.Applications)
                .First();
        }

        return student;
    }

    private bool IsStudentLoggedIn()
    {
        return HttpContext.Session.GetString("Role") == UserRole.Student.ToString();
    }

    private IActionResult RedirectToLogin()
    {
        TempData["Info"] = "Student login required.";
        return RedirectToAction("Login", "Account");
    }
}
