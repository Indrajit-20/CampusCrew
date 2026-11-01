using CampusCrew.Data;
using CampusCrew.Models;
using CampusCrew.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusCrew.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly PasswordHasher<User> _passwordHasher;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
        _passwordHasher = new PasswordHasher<User>();
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        // If already logged in, redirect directly to dashboard
        var role = HttpContext.Session.GetString("Role");
        if (!string.IsNullOrEmpty(role))
        {
            return role switch
            {
                nameof(UserRole.Organizer) => RedirectToAction("Dashboard", "Organizer"),
                nameof(UserRole.Admin) => RedirectToAction("Dashboard", "Admin"),
                _ => RedirectToAction("Dashboard", "Student")
            };
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(model);

        var emailLower = model.Email.Trim().ToLower();
        var user = _context.Users.FirstOrDefault(x => x.Email.ToLower() == emailLower);

        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        if (!user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "This account has been deactivated. Please contact administrator.");
            return View(model);
        }

        // Verify hashed password (fallback to plaintext check for legacy accounts if any)
        var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
        bool isValidPassword = verifyResult == PasswordVerificationResult.Success
            || (verifyResult == PasswordVerificationResult.Failed && user.PasswordHash == model.Password);

        if (!isValidPassword)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        if (user.Role == UserRole.Admin)
        {
            ModelState.AddModelError(string.Empty, "Administrator access is restricted to the dedicated Admin Portal (/Admin/Login).");
            return View(model);
        }

        SetSession(user);

        TempData["Success"] = $"Welcome back, {user.Name}!";

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

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
            AvailableSkills = _context.Skills.OrderBy(s => s.Name).Select(s => s.Name).ToList()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RegisterStudent(StudentRegisterViewModel model)
    {
        model.AvailableSkills = _context.Skills.OrderBy(s => s.Name).Select(s => s.Name).ToList();

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

        // 1. Create User account
        var user = new User
        {
            Name = model.Name.Trim(),
            Email = model.Email.Trim(),
            Role = UserRole.Student,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

        _context.Users.Add(user);
        _context.SaveChanges();

        // 2. Create Student profile (linked 1-to-1 via UserId)
        var student = new Student
        {
            UserId = user.Id,
            College = model.College.Trim(),
            Course = model.Course.Trim(),
            Year = model.Year,
            Location = model.Location.Trim(),
            Bio = string.IsNullOrWhiteSpace(model.Bio) ? "Student looking to collaborate on campus projects." : model.Bio.Trim(),
            Availability = "Flexible"
        };
        _context.Students.Add(student);
        _context.SaveChanges();

        // 3. Link student skills
        if (model.SelectedSkills != null && model.SelectedSkills.Any())
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

        // 1. Create User account
        var user = new User
        {
            Name = model.OrganizationName.Trim(),
            Email = model.OfficialEmail.Trim(),
            Role = UserRole.Organizer,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

        _context.Users.Add(user);
        _context.SaveChanges();

        // 2. Create OrganizerProfile (linked 1-to-1 via UserId)
        var organizerProfile = new OrganizerProfile
        {
            UserId = user.Id,
            OrganizationName = model.OrganizationName.Trim(),
            OrganizationType = model.OrganizationType.Trim(),
            College = model.College.Trim(),
            Location = model.Location.Trim(),
            Description = model.Description.Trim(),
            ContactNumber = model.ContactNumber.Trim()
        };
        _context.OrganizerProfiles.Add(organizerProfile);
        _context.SaveChanges();

        SetSession(user);
        TempData["Success"] = "Welcome to CampusCrew! Your organization account is ready. You can now post opportunities.";
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

    [HttpGet]
    [ActionName("Logout")]
    public IActionResult LogoutGet()
    {
        HttpContext.Session.Clear();
        TempData["Info"] = "You have been logged out.";
        return RedirectToAction("Index", "Home");
    }

    private void SetSession(User user)
    {
        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("DisplayName", user.Name);
        HttpContext.Session.SetString("Role", user.Role.ToString());
        HttpContext.Session.SetString("Email", user.Email);

        if (user.Role == UserRole.Student)
        {
            var studentId = _context.Students.Where(s => s.UserId == user.Id).Select(s => s.Id).FirstOrDefault();
            if (studentId > 0)
            {
                HttpContext.Session.SetInt32("StudentId", studentId);
            }
        }
        else if (user.Role == UserRole.Organizer)
        {
            var orgId = _context.OrganizerProfiles.Where(o => o.UserId == user.Id).Select(o => o.Id).FirstOrDefault();
            if (orgId > 0)
            {
                HttpContext.Session.SetInt32("OrganizerProfileId", orgId);
            }
        }
    }
}
