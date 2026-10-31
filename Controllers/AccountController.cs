using CampusCrew.Data;
using CampusCrew.Models;
using CampusCrew.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusCrew.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var emailLower = model.Email.Trim().ToLower();
        var user = _context.Users.FirstOrDefault(x => x.Email.ToLower() == emailLower && x.Password == model.Password && x.IsActive);

        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "The email or password does not match our records.");
            return View(model);
        }
        if (user.Role == UserRole.Admin)
        {
            ModelState.AddModelError(string.Empty, "Administrator access is restricted to the dedicated Admin Portal (/admin/login).");
            return View(model);
        }

        SetSession(user);
        return user.Role switch
        {
            UserRole.Organizer => RedirectToAction("Dashboard", "Organizer"),
            _ => RedirectToAction("Dashboard", "Student")
        };
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpGet]
    public IActionResult RegisterStudent()
    {
        var model = new StudentRegisterViewModel
        {
            AvailableSkills = _context.Skills.Select(s => s.Name).ToList()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RegisterStudent(StudentRegisterViewModel model)
    {
        model.AvailableSkills = _context.Skills.Select(s => s.Name).ToList();

        var emailLower = model.Email.Trim().ToLower();
        if (_context.Users.Any(x => x.Email.ToLower() == emailLower))
        {
            ModelState.AddModelError(nameof(model.Email), "An account with this email already exists.");
        }

        if (model.SelectedSkills == null || !model.SelectedSkills.Any())
        {
            ModelState.AddModelError(nameof(model.SelectedSkills), "Please select at least one skill.");
        }

        if (!ModelState.IsValid) return View(model);

        var user = new User
        {
            Name = model.Name.Trim(),
            Email = model.Email.Trim(),
            Password = model.Password,
            Role = UserRole.Student,
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);
        _context.SaveChanges();

        var student = new Student
        {
            UserId = user.Id,
            Name = model.Name.Trim(),
            Email = model.Email.Trim(),
            College = model.College.Trim(),
            Course = model.Course.Trim(),
            Year = model.Year,
            Location = model.Location.Trim(),
            Bio = string.IsNullOrWhiteSpace(model.Bio) ? "Student looking to collaborate on campus projects." : model.Bio.Trim(),
            Experience = $"{model.Course} student at {model.College}.",
            Availability = "Flexible / Weekends",
            Rating = 5.0
        };
        _context.Students.Add(student);
        _context.SaveChanges();

        // Link student skills
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
            _context.SaveChanges();
        }

        SetSession(user);
        TempData["Success"] = "Welcome to CampusCrew! Your student profile has been created.";
        return RedirectToAction("Dashboard", "Student");
    }

    [HttpGet]
    public IActionResult RegisterOrganization() => View(new OrganizationRegisterViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RegisterOrganization(OrganizationRegisterViewModel model)
    {
        var emailLower = model.OfficialEmail.Trim().ToLower();
        if (_context.Users.Any(x => x.Email.ToLower() == emailLower))
        {
            ModelState.AddModelError(nameof(model.OfficialEmail), "An account with this email already exists.");
        }

        if (!ModelState.IsValid) return View(model);

        var user = new User
        {
            Name = model.OrganizationName.Trim(),
            Email = model.OfficialEmail.Trim(),
            Password = model.Password,
            Role = UserRole.Organizer,
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);
        _context.SaveChanges();

        SetSession(user);
        TempData["Success"] = "Welcome to CampusCrew! You can now post opportunities.";
        return RedirectToAction("Dashboard", "Organizer");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        TempData["Info"] = "You have been logged out.";
        return RedirectToAction("Index", "Home");
    }

    private void SetSession(User user)
    {
        HttpContext.Session.SetString("DisplayName", user.Name);
        HttpContext.Session.SetString("Role", user.Role.ToString());
        HttpContext.Session.SetString("Email", user.Email);
    }
}
