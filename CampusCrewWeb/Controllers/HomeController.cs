using CampusCrew.Data;
using CampusCrew.Models;
using CampusCrew.Services;
using CampusCrew.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusCrew.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly CampusCrewApiClient _apiClient;

    public HomeController(ApplicationDbContext context, CampusCrewApiClient apiClient)
    {
        _context = context;
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        // 1. Fetch via Web API
        var stats = await _apiClient.GetHomeStatsAsync();
        var opportunities = await _apiClient.GetOpportunitiesAsync();
        var students = await _apiClient.GetStudentsAsync();
        var skills = await _apiClient.GetSkillsAsync();

        // 2. Prepare View Model
        var vm = new HomeIndexViewModel
        {
            FeaturedOpportunities = opportunities.OrderByDescending(o => o.Id).Take(3).ToList(),
            FeaturedStudents = students.OrderByDescending(s => s.Id).Take(4).ToList(),
            Skills = skills.OrderBy(s => s.Name).ToList(),
            TotalStudents = stats?.TotalStudents ?? _context.Students.Count(),
            TotalOpportunities = stats?.TotalOpportunities ?? _context.Opportunities.Count(),
            TotalSkills = stats?.TotalSkills ?? _context.Skills.Count(),
            TotalCollaborations = stats?.TotalCollaborations ?? _context.Collaborations.Count()
        };

        return View(vm);
    }

    public IActionResult About() => View();

    public async Task<IActionResult> Students(StudentFilterViewModel filter)
    {
        // 1. Fetch filtered students and skills list via Web API
        var students = await _apiClient.GetStudentsAsync(
            filter.Search,
            filter.Skill,
            filter.College,
            filter.Location,
            filter.Availability);

        var skills = await _apiClient.GetSkillsAsync();

        filter.Skills = skills.Select(s => s.Name).OrderBy(s => s).ToList();
        filter.Students = students;

        return View(filter);
    }

    public async Task<IActionResult> Opportunities(OpportunityFilterViewModel filter)
    {
        // 1. Fetch filtered opportunities and skills list via Web API
        var opportunities = await _apiClient.GetOpportunitiesAsync(
            filter.Search,
            filter.Skill,
            filter.Location);

        var skills = await _apiClient.GetSkillsAsync();

        filter.Skills = skills.Select(s => s.Name).OrderBy(s => s).ToList();
        filter.Opportunities = opportunities;

        return View(filter);
    }

    public async Task<IActionResult> OpportunityDetails(int id)
    {
        // Fetch via Web API
        var opportunity = await _apiClient.GetOpportunityByIdAsync(id);

        if (opportunity is null)
        {
            // Fallback check in context if API wasn't running
            opportunity = _context.Opportunities
                .Include(o => o.OpportunitySkills)
                    .ThenInclude(os => os.Skill)
                .Include(o => o.Organizer)
                .Include(o => o.Applications)
                .FirstOrDefault(o => o.Id == id);
        }

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
            var app = _context.Applications.FirstOrDefault(a => a.OpportunityId == id && a.StudentId == activeStudent.Id);
            if (app != null)
            {
                ViewBag.HasApplied = true;
                ViewBag.ExistingApplication = app;
            }
        }

        return View(opportunity);
    }
}
