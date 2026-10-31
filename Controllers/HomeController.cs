using CampusCrew.Data;
using CampusCrew.Models;
using CampusCrew.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusCrew.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var vm = new HomeIndexViewModel
        {
            FeaturedOpportunities = _context.Opportunities
                .Include(o => o.OpportunitySkills)
                    .ThenInclude(os => os.Skill)
                .OrderByDescending(o => o.Id)
                .Take(3)
                .ToList(),
            FeaturedStudents = _context.Students
                .Include(s => s.StudentSkills)
                    .ThenInclude(ss => ss.Skill)
                .OrderByDescending(s => s.Rating)
                .Take(4)
                .ToList(),
            Skills = _context.Skills.ToList(),
            TotalStudents = _context.Students.Count(),
            TotalOpportunities = _context.Opportunities.Count(),
            TotalSkills = _context.Skills.Count(),
            TotalCollaborations = _context.Collaborations.Count()
        };
        return View(vm);
    }

    public IActionResult About() => View();

    public IActionResult Students(StudentFilterViewModel filter)
    {
        var query = _context.Students
            .Include(s => s.StudentSkills)
                .ThenInclude(ss => ss.Skill)
            .AsQueryable();

        var search = filter.Search?.Trim();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.Name.Contains(search) || x.Course.Contains(search) || x.Bio.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(filter.Skill))
        {
            query = query.Where(x => x.StudentSkills.Any(ss => ss.Skill.Name == filter.Skill));
        }

        if (!string.IsNullOrWhiteSpace(filter.College))
        {
            query = query.Where(x => x.College.Contains(filter.College));
        }

        if (!string.IsNullOrWhiteSpace(filter.Location))
        {
            query = query.Where(x => x.Location.Contains(filter.Location));
        }

        if (!string.IsNullOrWhiteSpace(filter.Availability))
        {
            query = query.Where(x => x.Availability.Contains(filter.Availability));
        }

        filter.Skills = _context.Skills.Select(x => x.Name).ToList();
        filter.Students = query.ToList();
        return View(filter);
    }

    public IActionResult Opportunities(OpportunityFilterViewModel filter)
    {
        var query = _context.Opportunities
            .Include(o => o.OpportunitySkills)
                .ThenInclude(os => os.Skill)
            .AsQueryable();

        var search = filter.Search?.Trim();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.Title.Contains(search) || x.Description.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(filter.Skill))
        {
            query = query.Where(x => x.OpportunitySkills.Any(os => os.Skill.Name == filter.Skill));
        }

        if (!string.IsNullOrWhiteSpace(filter.College))
        {
            query = query.Where(x => x.College.Contains(filter.College));
        }

        if (!string.IsNullOrWhiteSpace(filter.Location))
        {
            query = query.Where(x => x.Location.Contains(filter.Location));
        }

        filter.Skills = _context.Skills.Select(x => x.Name).ToList();
        filter.Opportunities = query.OrderBy(x => x.ApplicationDeadline).ToList();
        return View(filter);
    }

    public IActionResult OpportunityDetails(int id)
    {
        var opportunity = _context.Opportunities
            .Include(o => o.OpportunitySkills)
                .ThenInclude(os => os.Skill)
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
}
