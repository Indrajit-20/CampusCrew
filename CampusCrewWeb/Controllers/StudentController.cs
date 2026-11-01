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
            .OrderByDescending(a => a.AppliedAt)
            .ToList();

        var recentApps = studentApps.Take(4).Select(a => new StudentApplicationItemViewModel
        {
            ApplicationId = a.Id,
            OpportunityId = a.OpportunityId,
            OpportunityTitle = a.Opportunity?.Title ?? "Opportunity",
            OrganizerName = a.Opportunity?.Organizer?.Name ?? "Organizer",
            Location = a.Opportunity?.Location ?? "",
            OpportunityDate = a.Opportunity?.OpportunityDate ?? DateTime.Today,
            ApplicationDeadline = a.Opportunity?.Deadline ?? DateTime.Today,
            AppliedOn = a.AppliedAt,
            Message = a.Message,
            Status = a.Status,
            OpportunityType = a.Opportunity?.OpportunityType ?? OpportunityType.Collaboration
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
            PendingCollaborationRequests = receivedRequests.Count(x => x.Status == StatusType.Pending),
            RecentApplications = recentApps
        };

        return View(model);
    }

    public IActionResult Profile()
    {
        if (!IsStudentLoggedIn()) return RedirectToLogin();
        var student = GetActiveStudent();
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
            Name = student.User?.Name ?? string.Empty,
            College = student.College,
            Course = student.Course,
            Year = student.Year,
            Location = student.Location,
            Bio = student.Bio,
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

        // Update User Name
        if (student.User != null)
        {
            student.User.Name = model.Name.Trim();
            HttpContext.Session.SetString("DisplayName", student.User.Name);
        }

        // Update Student profile fields
        student.College = model.College.Trim();
        student.Course = model.Course.Trim();
        student.Year = model.Year;
        student.Location = model.Location.Trim();
        student.Bio = model.Bio.Trim();
        student.Availability = model.Availability.Trim();

        // Update skills
        var existingSkills = _context.StudentSkills.Where(ss => ss.StudentId == student.Id).ToList();
        _context.StudentSkills.RemoveRange(existingSkills);

        if (model.SelectedSkills != null && model.SelectedSkills.Any())
        {
            foreach (var skillName in model.SelectedSkills.Distinct())
            {
                var skill = _context.Skills.FirstOrDefault(s => s.Name.ToLower() == skillName.ToLower());
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
            .OrderByDescending(a => a.AppliedAt)
            .Select(a => new StudentApplicationItemViewModel
            {
                ApplicationId = a.Id,
                OpportunityId = a.OpportunityId,
                OpportunityTitle = a.Opportunity != null ? a.Opportunity.Title : "Opportunity",
                OrganizerName = a.Opportunity != null && a.Opportunity.Organizer != null ? a.Opportunity.Organizer.Name : "Organizer",
                Location = a.Opportunity != null ? a.Opportunity.Location : "",
                OpportunityDate = a.Opportunity != null ? a.Opportunity.OpportunityDate : DateTime.Today,
                ApplicationDeadline = a.Opportunity != null ? a.Opportunity.Deadline : DateTime.Today,
                AppliedOn = a.AppliedAt,
                Message = a.Message,
                Status = a.Status,
                OpportunityType = a.Opportunity != null ? a.Opportunity.OpportunityType : OpportunityType.Collaboration
            })
            .ToList();

        return View(new MyApplicationsViewModel { Applications = apps });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult WithdrawApplication(int id)
    {
        if (!IsStudentLoggedIn()) return RedirectToLogin();
        var student = GetActiveStudent();

        var app = _context.Applications.FirstOrDefault(a => a.Id == id && a.StudentId == student.Id);
        if (app != null && app.Status == StatusType.Pending)
        {
            _context.Applications.Remove(app);
            _context.SaveChanges();
            TempData["Success"] = "Application withdrawn successfully.";
        }
        else
        {
            TempData["Error"] = "Could not withdraw this application.";
        }

        return RedirectToAction(nameof(MyApplications));
    }

    public IActionResult CollaborationRequests(string tab = "received")
    {
        if (!IsStudentLoggedIn()) return RedirectToLogin();
        var student = GetActiveStudent();

        var received = _context.Collaborations
            .Include(c => c.Requester)
                .ThenInclude(r => r!.User)
            .Include(c => c.Requester)
                .ThenInclude(r => r!.StudentSkills)
                    .ThenInclude(ss => ss.Skill)
            .Where(c => c.ReceiverId == student.Id)
            .OrderByDescending(c => c.CreatedAt)
            .ToList();

        var sent = _context.Collaborations
            .Include(c => c.Receiver)
                .ThenInclude(r => r!.User)
            .Include(c => c.Receiver)
                .ThenInclude(r => r!.StudentSkills)
                    .ThenInclude(ss => ss.Skill)
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
            .Include(s => s.User)
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
            ReceiverName = targetStudent.User?.Name ?? "Student",
            ReceiverSkills = string.Join(", ", targetStudent.Skills)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RequestCollaboration(CollaborationRequestViewModel model)
    {
        if (!IsStudentLoggedIn()) return RedirectToLogin();
        var targetStudent = _context.Students.Include(s => s.User).FirstOrDefault(s => s.Id == model.ReceiverId);
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

        TempData["Success"] = $"Collaboration request sent to {targetStudent.User?.Name}!";
        return RedirectToAction(nameof(CollaborationRequests), new { tab = "sent" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AcceptCollaboration(int id)
    {
        if (!IsStudentLoggedIn()) return RedirectToLogin();
        var student = GetActiveStudent();
        var req = _context.Collaborations.Include(c => c.Requester).ThenInclude(r => r!.User).FirstOrDefault(c => c.Id == id);
        if (req == null || req.ReceiverId != student.Id) return NotFound();

        req.Status = StatusType.Accepted;
        _context.SaveChanges();

        TempData["Success"] = $"Accepted collaboration from {req.Requester?.User?.Name}!";
        return RedirectToAction(nameof(CollaborationRequests), new { tab = "received" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RejectCollaboration(int id)
    {
        if (!IsStudentLoggedIn()) return RedirectToLogin();
        var student = GetActiveStudent();
        var req = _context.Collaborations.Include(c => c.Requester).ThenInclude(r => r!.User).FirstOrDefault(c => c.Id == id);
        if (req == null || req.ReceiverId != student.Id) return NotFound();

        req.Status = StatusType.Rejected;
        _context.SaveChanges();

        TempData["Info"] = $"Declined collaboration from {req.Requester?.User?.Name}.";
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
            .Include(s => s.User)
            .Include(s => s.StudentSkills).ThenInclude(ss => ss.Skill)
            .Include(s => s.Portfolios)
            .FirstOrDefault(s => s.Id == id);

        if (student is null) return NotFound();

        var userRole = HttpContext.Session.GetString("Role");
        var userId = HttpContext.Session.GetInt32("UserId");

        bool isSelf = userId.HasValue && student.UserId == userId.Value;
        ViewBag.IsSelf = isSelf;
        ViewBag.ViewerRole = userRole;

        // If Organizer is viewing, fetch their active opportunities to invite this student
        if (userRole == UserRole.Organizer.ToString() && userId.HasValue)
        {
            ViewBag.OrganizerOpportunities = _context.Opportunities
                .Where(o => o.OrganizerId == userId.Value)
                .ToList();
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
        var userId = HttpContext.Session.GetInt32("UserId");
        Student? activeStudent = null;

        if (userRole == UserRole.Student.ToString() && userId.HasValue)
        {
            activeStudent = _context.Students.FirstOrDefault(s => s.UserId == userId.Value);
        }

        ViewBag.UserRole = userRole;
        ViewBag.HasApplied = false;
        ViewBag.ExistingApplication = (Application?)null;
        ViewBag.IsDeadlinePassed = opportunity.Deadline.Date < DateTime.Today;

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
            ImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl) ? "" : model.ImageUrl.Trim(),
            ExternalLink = model.ExternalLink?.Trim() ?? string.Empty,
            ProjectDate = model.ProjectDate,
            Role = model.Role.Trim()
        };

        _context.Portfolios.Add(item);
        _context.SaveChanges();

        TempData["Success"] = "Portfolio project added successfully.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePortfolio(int id)
    {
        if (!IsStudentLoggedIn()) return RedirectToLogin();
        var student = GetActiveStudent();

        var portfolio = _context.Portfolios.FirstOrDefault(p => p.Id == id && p.StudentId == student.Id);
        if (portfolio != null)
        {
            _context.Portfolios.Remove(portfolio);
            _context.SaveChanges();
            TempData["Success"] = "Project removed from portfolio.";
        }

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

        if (opportunity.Deadline.Date < DateTime.Today)
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

        ViewBag.Opportunity = opportunity;
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

        if (opportunity.Deadline.Date < DateTime.Today)
        {
            TempData["Error"] = "The deadline has already passed.";
            return RedirectToAction(nameof(OpportunityDetails), new { id = opportunity.Id });
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Opportunity = opportunity;
            return View(model);
        }

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
            AppliedAt = DateTime.UtcNow,
            Status = StatusType.Pending
        };

        _context.Applications.Add(app);
        _context.SaveChanges();

        TempData["Success"] = "Your application has been submitted!";
        return RedirectToAction(nameof(MyApplications));
    }

    private Student GetActiveStudent()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId.HasValue && userId.Value > 0)
        {
            var student = _context.Students
                .Include(s => s.User)
                .Include(s => s.StudentSkills).ThenInclude(ss => ss.Skill)
                .Include(s => s.Portfolios)
                .Include(s => s.Applications)
                .FirstOrDefault(s => s.UserId == userId.Value);

            if (student != null) return student;
        }

        var email = HttpContext.Session.GetString("Email") ?? string.Empty;
        var studentByEmail = _context.Students
            .Include(s => s.User)
            .Include(s => s.StudentSkills).ThenInclude(ss => ss.Skill)
            .Include(s => s.Portfolios)
            .Include(s => s.Applications)
            .FirstOrDefault(s => s.User != null && s.User.Email.ToLower() == email.ToLower());

        return studentByEmail ?? throw new InvalidOperationException("Student profile not found. Please log in.");
    }

    private bool IsStudentLoggedIn()
    {
        return HttpContext.Session.GetString("Role") == UserRole.Student.ToString()
            && (HttpContext.Session.GetInt32("UserId") ?? 0) > 0;
    }

    private IActionResult RedirectToLogin()
    {
        TempData["Info"] = "Student login required.";
        return RedirectToAction("Login", "Account");
    }
}
