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

        var userId = GetCurrentUserId();
        var myOpps = _context.Opportunities
            .Include(o => o.Applications)
            .Include(o => o.OpportunitySkills)
                .ThenInclude(os => os.Skill)
            .Where(o => o.OrganizerId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToList();

        ViewBag.TotalOpportunities = myOpps.Count;
        ViewBag.PendingApplicants = myOpps.Sum(o => o.Applications.Count(a => a.Status == StatusType.Pending));
        ViewBag.AcceptedStudents = myOpps.Sum(o => o.Applications.Count(a => a.Status == StatusType.Accepted));
        ViewBag.CompletedCount = myOpps.Sum(o => o.Applications.Count(a => a.Status == StatusType.Completed));

        var profile = _context.OrganizerProfiles.FirstOrDefault(p => p.UserId == userId);
        ViewBag.Profile = profile;

        return View(myOpps);
    }

    public IActionResult Opportunities()
    {
        if (!IsOrganizerLoggedIn()) return RedirectToLogin();

        var userId = GetCurrentUserId();
        var myOpps = _context.Opportunities
            .Include(o => o.Applications)
            .Include(o => o.OpportunitySkills)
                .ThenInclude(os => os.Skill)
            .Where(o => o.OrganizerId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToList();

        return View(myOpps);
    }

    public IActionResult Details(int id)
    {
        if (!IsOrganizerLoggedIn()) return RedirectToLogin();

        var userId = GetCurrentUserId();
        var opportunity = _context.Opportunities
            .Include(o => o.OpportunitySkills)
                .ThenInclude(os => os.Skill)
            .Include(o => o.Applications)
                .ThenInclude(a => a.Student!)
                    .ThenInclude(s => s.User)
            .FirstOrDefault(o => o.Id == id && o.OrganizerId == userId);

        if (opportunity == null) return NotFound();

        return View(opportunity);
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

        var userId = GetCurrentUserId();

        var opportunity = new Opportunity
        {
            OrganizerId = userId,
            Title = model.Title.Trim(),
            Description = model.Description.Trim(),
            OpportunityDate = model.OpportunityDate,
            Location = model.Location.Trim(),
            RequiredMembers = model.NumberOfParticipants,
            Deadline = model.ApplicationDeadline,
            OpportunityType = model.Type,
            CreatedAt = DateTime.UtcNow,
            Status = StatusType.Accepted
        };

        _context.Opportunities.Add(opportunity);
        _context.SaveChanges();

        // Add Skills
        if (!string.IsNullOrWhiteSpace(model.RequiredSkills))
        {
            var skills = model.RequiredSkills.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase);

            foreach (var skillName in skills)
            {
                var skill = _context.Skills.FirstOrDefault(s => s.Name.ToLower() == skillName.ToLower());
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
        }

        TempData["Success"] = "Opportunity published successfully!";
        return RedirectToAction(nameof(Opportunities));
    }

    [HttpGet]
    public IActionResult EditOpportunity(int id)
    {
        if (!IsOrganizerLoggedIn()) return RedirectToLogin();
        var userId = GetCurrentUserId();

        var opp = _context.Opportunities
            .Include(o => o.OpportunitySkills)
                .ThenInclude(os => os.Skill)
            .FirstOrDefault(o => o.Id == id && o.OrganizerId == userId);

        if (opp == null) return NotFound();

        var vm = new OpportunityFormViewModel
        {
            Title = opp.Title,
            Description = opp.Description,
            Type = opp.OpportunityType,
            College = opp.College,
            Location = opp.Location,
            NumberOfParticipants = opp.RequiredMembers,
            OpportunityDate = opp.OpportunityDate,
            ApplicationDeadline = opp.Deadline,
            RequiredSkills = string.Join(", ", opp.OpportunitySkills.Where(os => os.Skill != null).Select(os => os.Skill!.Name))
        };

        ViewBag.OpportunityId = opp.Id;
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditOpportunity(int id, OpportunityFormViewModel model)
    {
        if (!IsOrganizerLoggedIn()) return RedirectToLogin();

        if (model.ApplicationDeadline.Date >= model.OpportunityDate.Date)
        {
            ModelState.AddModelError(nameof(model.ApplicationDeadline), "Deadline must be before the opportunity date.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.OpportunityId = id;
            return View(model);
        }

        var userId = GetCurrentUserId();
        var opp = _context.Opportunities
            .Include(o => o.OpportunitySkills)
            .FirstOrDefault(o => o.Id == id && o.OrganizerId == userId);

        if (opp == null) return NotFound();

        opp.Title = model.Title.Trim();
        opp.Description = model.Description.Trim();
        opp.OpportunityType = model.Type;
        opp.College = string.IsNullOrWhiteSpace(model.College) ? "" : model.College.Trim();
        opp.Location = model.Location.Trim();
        opp.RequiredMembers = model.NumberOfParticipants;
        opp.OpportunityDate = model.OpportunityDate;
        opp.Deadline = model.ApplicationDeadline;

        // Synchronize Skills
        _context.OpportunitySkills.RemoveRange(opp.OpportunitySkills);
        if (!string.IsNullOrWhiteSpace(model.RequiredSkills))
        {
            var skills = model.RequiredSkills.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase);

            foreach (var skillName in skills)
            {
                var skill = _context.Skills.FirstOrDefault(s => s.Name.ToLower() == skillName.ToLower());
                if (skill == null)
                {
                    skill = new Skill { Name = skillName };
                    _context.Skills.Add(skill);
                    _context.SaveChanges();
                }
                _context.OpportunitySkills.Add(new OpportunitySkill
                {
                    OpportunityId = opp.Id,
                    SkillId = skill.Id
                });
            }
        }

        _context.SaveChanges();
        TempData["Success"] = "Opportunity updated successfully!";
        return RedirectToAction(nameof(Opportunities));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteOpportunity(int id)
    {
        if (!IsOrganizerLoggedIn()) return RedirectToLogin();

        var userId = GetCurrentUserId();
        var opp = _context.Opportunities.FirstOrDefault(o => o.Id == id && o.OrganizerId == userId);
        if (opp == null) return NotFound();

        _context.Opportunities.Remove(opp);
        _context.SaveChanges();

        TempData["Success"] = "Opportunity removed successfully.";
        return RedirectToAction(nameof(Opportunities));
    }

    public IActionResult Applicants(int id)
    {
        if (!IsOrganizerLoggedIn()) return RedirectToLogin();

        var userId = GetCurrentUserId();
        var opp = _context.Opportunities
            .Include(o => o.Applications)
                .ThenInclude(a => a.Student!)
                    .ThenInclude(s => s.User)
            .Include(o => o.Applications)
                .ThenInclude(a => a.Student!)
                    .ThenInclude(s => s.StudentSkills)
                        .ThenInclude(ss => ss.Skill)
            .FirstOrDefault(o => o.Id == id && o.OrganizerId == userId);

        if (opp is null) return NotFound();

        ViewBag.StudentsMap = _context.Students
            .Include(s => s.User)
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

        var userId = GetCurrentUserId();
        var opp = _context.Opportunities.FirstOrDefault(o => o.Id == opportunityId && o.OrganizerId == userId);
        if (opp == null) return Forbid();

        var application = _context.Applications
            .Include(a => a.Student!)
                .ThenInclude(s => s.User)
            .FirstOrDefault(x => x.Id == applicationId && x.OpportunityId == opportunityId);

        if (application is null) return NotFound();

        var studentName = application.Student?.User?.Name ?? "Student";

        if (application.Status == StatusType.Pending && (status == StatusType.Accepted || status == StatusType.Rejected))
        {
            application.Status = status;
            _context.SaveChanges();
            TempData["Success"] = $"Application by {studentName} marked as {status}.";
        }
        else if (application.Status == StatusType.Accepted && status == StatusType.Completed)
        {
            application.Status = StatusType.Completed;
            _context.SaveChanges();
            TempData["Success"] = $"Collaboration with {studentName} marked as Completed!";
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

        var userId = GetCurrentUserId();
        var opp = _context.Opportunities.Include(o => o.Applications).FirstOrDefault(o => o.Id == opportunityId && o.OrganizerId == userId);
        var student = _context.Students.Include(s => s.User).FirstOrDefault(s => s.Id == studentId);

        if (student == null || opp == null) return NotFound();

        var studentName = student.User?.Name ?? "Student";

        if (opp.Applications.Any(a => a.StudentId == studentId))
        {
            TempData["Info"] = $"{studentName} is already an applicant for this opportunity.";
            return RedirectToAction("StudentProfile", "Student", new { id = studentId });
        }

        var app = new Application
        {
            OpportunityId = opp.Id,
            StudentId = student.Id,
            Message = string.IsNullOrWhiteSpace(message) ? "Direct invitation from organizer" : message.Trim(),
            AppliedAt = DateTime.UtcNow,
            Status = StatusType.Pending
        };

        _context.Applications.Add(app);
        _context.SaveChanges();

        TempData["Success"] = $"Invitation sent to {studentName} for '{opp.Title}'!";
        return RedirectToAction("StudentProfile", "Student", new { id = studentId });
    }

    private int GetCurrentUserId()
    {
        return HttpContext.Session.GetInt32("UserId") ?? 0;
    }

    private bool IsOrganizerLoggedIn()
    {
        return HttpContext.Session.GetString("Role") == UserRole.Organizer.ToString() && GetCurrentUserId() > 0;
    }

    private IActionResult RedirectToLogin()
    {
        TempData["Info"] = "Organizer login required.";
        return RedirectToAction("Login", "Account");
    }
}
