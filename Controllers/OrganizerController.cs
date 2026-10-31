using CampusCrew.Data;
using CampusCrew.Models;
using CampusCrew.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusCrew.Controllers;

public class OrganizerController : Controller
{
    private readonly ApplicationDbContext _context;

    public OrganizerController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Dashboard()
    {
        if (!IsOrganizerLoggedIn()) return RedirectToLogin();

        var email = HttpContext.Session.GetString("Email") ?? string.Empty;
        var user = _context.Users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());

        var myOpps = _context.Opportunities
            .Include(o => o.Applications)
            .Include(o => o.OpportunitySkills)
                .ThenInclude(os => os.Skill)
            .Where(o => user != null && o.OrganizerId == user.Id)
            .ToList();

        if (myOpps.Count == 0)
        {
            myOpps = _context.Opportunities
                .Include(o => o.Applications)
                .Include(o => o.OpportunitySkills)
                    .ThenInclude(os => os.Skill)
                .ToList();
        }

        ViewBag.TotalOpportunities = myOpps.Count;
        ViewBag.PendingApplicants = myOpps.Sum(o => o.Applications.Count(a => a.Status == StatusType.Pending));
        ViewBag.AcceptedStudents = myOpps.Sum(o => o.Applications.Count(a => a.Status == StatusType.Accepted));
        ViewBag.CompletedCount = myOpps.Sum(o => o.Applications.Count(a => a.Status == StatusType.Completed));

        return View(myOpps);
    }

    public IActionResult Opportunities()
    {
        if (!IsOrganizerLoggedIn()) return RedirectToLogin();

        var email = HttpContext.Session.GetString("Email") ?? string.Empty;
        var user = _context.Users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());

        var myOpps = _context.Opportunities
            .Include(o => o.Applications)
            .Include(o => o.OpportunitySkills)
                .ThenInclude(os => os.Skill)
            .Where(o => user != null && o.OrganizerId == user.Id)
            .ToList();

        if (myOpps.Count == 0)
        {
            myOpps = _context.Opportunities
                .Include(o => o.Applications)
                .Include(o => o.OpportunitySkills)
                    .ThenInclude(os => os.Skill)
                .ToList();
        }

        return View(myOpps);
    }

    public IActionResult Details(int id)
    {
        if (!IsOrganizerLoggedIn()) return RedirectToLogin();

        var opportunity = _context.Opportunities
            .Include(o => o.OpportunitySkills)
                .ThenInclude(os => os.Skill)
            .Include(o => o.Applications)
            .FirstOrDefault(o => o.Id == id);

        return opportunity != null ? View(opportunity) : NotFound();
    }

    [HttpGet]
    public IActionResult CreateOpportunity()
    {
        if (!IsOrganizerLoggedIn()) return RedirectToLogin();
        return View(new OpportunityFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateOpportunity(OpportunityFormViewModel model)
    {
        if (!IsOrganizerLoggedIn()) return RedirectToLogin();

        if (model.ApplicationDeadline.Date >= model.OpportunityDate.Date)
        {
            ModelState.AddModelError(nameof(model.ApplicationDeadline), "Deadline must be before the opportunity date.");
        }

        if (!ModelState.IsValid) return View(model);

        var email = HttpContext.Session.GetString("Email") ?? string.Empty;
        var user = _context.Users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());

        var opportunity = new Opportunity
        {
            Title = model.Title.Trim(),
            Description = model.Description.Trim(),
            OpportunityDate = model.OpportunityDate,
            Location = model.Location.Trim(),
            College = model.College.Trim(),
            NumberOfParticipants = model.NumberOfParticipants,
            ApplicationDeadline = model.ApplicationDeadline,
            Type = model.Type,
            AdditionalRequirements = model.AdditionalRequirements?.Trim() ?? string.Empty,
            OrganizerId = user?.Id
        };

        _context.Opportunities.Add(opportunity);
        _context.SaveChanges();

        // Add Skills
        var skills = model.RequiredSkills.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct(StringComparer.OrdinalIgnoreCase);
        foreach (var skillName in skills)
        {
            var skill = _context.Skills.FirstOrDefault(s => s.Name == skillName);
            if (skill == null)
            {
                skill = new Skill { Name = skillName };
                _context.Skills.Add(skill);
                _context.SaveChanges();
            }
            _context.OpportunitySkills.Add(new OpportunitySkill
            {
                OpportunityId = opportunity.Id,
                SkillId = skill.Id
            });
        }
        _context.SaveChanges();

        TempData["Success"] = "Opportunity published!";
        return RedirectToAction(nameof(Opportunities));
    }

    public IActionResult Applicants(int id)
    {
        if (!IsOrganizerLoggedIn()) return RedirectToLogin();

        var opp = _context.Opportunities
            .Include(o => o.Applications)
                .ThenInclude(a => a.Student)
            .FirstOrDefault(o => o.Id == id);

        if (opp is null) return NotFound();

        ViewBag.StudentsMap = _context.Students
            .Include(s => s.StudentSkills)
                .ThenInclude(ss => ss.Skill)
            .ToDictionary(s => s.Id, s => s);

        return View(opp);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateApplication(int opportunityId, int applicationId, StatusType status)
    {
        if (!IsOrganizerLoggedIn()) return RedirectToLogin();

        var application = _context.Applications.Include(a => a.Student).FirstOrDefault(x => x.Id == applicationId && x.OpportunityId == opportunityId);
        if (application is null) return NotFound();

        if (application.Status == StatusType.Pending && (status == StatusType.Accepted || status == StatusType.Rejected))
        {
            application.Status = status;
            _context.SaveChanges();
            TempData["Success"] = $"Application by {application.Student?.Name} marked as {status}.";
        }
        else if (application.Status == StatusType.Accepted && status == StatusType.Completed)
        {
            application.Status = StatusType.Completed;
            _context.SaveChanges();
            TempData["Success"] = $"Collaboration with {application.Student?.Name} marked as Completed!";
        }
        else
        {
            TempData["Info"] = $"Status could not be updated to {status}.";
        }

        return RedirectToAction(nameof(Applicants), new { id = opportunityId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult InviteStudent(int studentId, int opportunityId, string message)
    {
        if (!IsOrganizerLoggedIn()) return RedirectToLogin();

        var student = _context.Students.FirstOrDefault(s => s.Id == studentId);
        var opp = _context.Opportunities.Include(o => o.Applications).FirstOrDefault(o => o.Id == opportunityId);
        if (student == null || opp == null) return NotFound();

        if (opp.Applications.Any(a => a.StudentId == studentId))
        {
            TempData["Info"] = $"{student.Name} is already an applicant for this opportunity.";
            return RedirectToAction("StudentProfile", "Student", new { id = studentId });
        }

        var app = new Application
        {
            OpportunityId = opp.Id,
            StudentId = student.Id,
            Message = string.IsNullOrWhiteSpace(message) ? "Direct invitation from organizer" : message.Trim(),
            AppliedOn = DateTime.UtcNow,
            Status = StatusType.Accepted
        };

        _context.Applications.Add(app);
        _context.SaveChanges();

        TempData["Success"] = $"Invitation sent to {student.Name} for '{opp.Title}'!";
        return RedirectToAction("StudentProfile", "Student", new { id = studentId });
    }

    private bool IsOrganizerLoggedIn()
    {
        return HttpContext.Session.GetString("Role") == UserRole.Organizer.ToString();
    }

    private IActionResult RedirectToLogin()
    {
        TempData["Info"] = "Organization login required.";
        return RedirectToAction("Login", "Account");
    }
}
