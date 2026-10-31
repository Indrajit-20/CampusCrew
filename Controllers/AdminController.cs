using CampusCrew.Data;
using CampusCrew.Models;
using CampusCrew.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusCrew.Controllers;

public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (IsAdminLoggedIn())
        {
            return RedirectToAction(nameof(Dashboard));
        }
        return View(new AdminLoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(AdminLoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var emailLower = model.Email.Trim().ToLower();
        var adminUser = _context.Users.FirstOrDefault(x =>
            x.Email.ToLower() == emailLower &&
            x.Password == model.Password &&
            x.Role == UserRole.Admin &&
            x.IsActive);

        if (adminUser is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid administrator credentials.");
            return View(model);
        }

        HttpContext.Session.SetString("DisplayName", adminUser.Name);
        HttpContext.Session.SetString("Role", adminUser.Role.ToString());
        HttpContext.Session.SetString("Email", adminUser.Email);

        TempData["Success"] = "Welcome to Administrator Portal.";
        return RedirectToAction(nameof(Dashboard));
    }

    public IActionResult Dashboard()
    {
        if (!IsAdminLoggedIn()) return RedirectToAdminLogin();

        var vm = new AdminDashboardViewModel
        {
            Students = _context.Students.ToList(),
            Users = _context.Users.ToList(),
            Opportunities = _context.Opportunities.Include(o => o.Applications).ToList(),
            Skills = _context.Skills.ToList(),
            Collaborations = _context.Collaborations.ToList(),
            Reviews = _context.Reviews.ToList()
        };

        return View(vm);
    }

    public IActionResult Users()
    {
        if (!IsAdminLoggedIn()) return RedirectToAdminLogin();
        return View(_context.Users.OrderByDescending(u => u.Id).ToList());
    }

    public IActionResult Skills()
    {
        if (!IsAdminLoggedIn()) return RedirectToAdminLogin();
        return View(_context.Skills.OrderBy(s => s.Name).ToList());
    }

    public IActionResult Opportunities()
    {
        if (!IsAdminLoggedIn()) return RedirectToAdminLogin();
        return View(_context.Opportunities.Include(o => o.Applications).OrderByDescending(o => o.Id).ToList());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddSkill(string name)
    {
        if (!IsAdminLoggedIn()) return RedirectToAdminLogin();

        if (!string.IsNullOrWhiteSpace(name) && name.Trim().Length <= 50)
        {
            var trimmed = name.Trim();
            if (!_context.Skills.Any(s => s.Name.ToLower() == trimmed.ToLower()))
            {
                _context.Skills.Add(new Skill { Name = trimmed });
                _context.SaveChanges();
                TempData["Success"] = $"Skill '{trimmed}' added.";
            }
            else
            {
                TempData["Error"] = $"Skill '{trimmed}' already exists.";
            }
        }
        else
        {
            TempData["Error"] = "Skill name is required (max 50 characters).";
        }
        return RedirectToAction(nameof(Skills));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteSkill(int id)
    {
        if (!IsAdminLoggedIn()) return RedirectToAdminLogin();

        var skill = _context.Skills.FirstOrDefault(s => s.Id == id);
        if (skill != null)
        {
            _context.Skills.Remove(skill);
            _context.SaveChanges();
            TempData["Success"] = "Skill removed.";
        }
        else
        {
            TempData["Error"] = "Skill not found.";
        }
        return RedirectToAction(nameof(Skills));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ToggleUserStatus(int id)
    {
        if (!IsAdminLoggedIn()) return RedirectToAdminLogin();

        var user = _context.Users.FirstOrDefault(u => u.Id == id);
        if (user != null)
        {
            var currentEmail = HttpContext.Session.GetString("Email");
            if (user.Email.Equals(currentEmail, StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Cannot deactivate your own account.";
                return RedirectToAction(nameof(Users));
            }

            user.IsActive = !user.IsActive;
            _context.SaveChanges();
            var statusStr = user.IsActive ? "activated" : "deactivated";
            TempData["Success"] = $"Account '{user.Name}' has been {statusStr}.";
        }
        else
        {
            TempData["Error"] = "User not found.";
        }
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddUser(string name, string email, string password, UserRole role)
    {
        if (!IsAdminLoggedIn()) return RedirectToAdminLogin();

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            TempData["Error"] = "All fields are required.";
            return RedirectToAction(nameof(Users));
        }

        var emailLower = email.Trim().ToLower();
        if (_context.Users.Any(u => u.Email.ToLower() == emailLower))
        {
            TempData["Error"] = "An account with this email already exists.";
            return RedirectToAction(nameof(Users));
        }

        var newUser = new User
        {
            Name = name.Trim(),
            Email = email.Trim(),
            Password = password,
            Role = role,
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(newUser);
        _context.SaveChanges();

        TempData["Success"] = $"Account '{name.Trim()}' ({role}) created.";
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteOpportunity(int id)
    {
        if (!IsAdminLoggedIn()) return RedirectToAdminLogin();

        var opp = _context.Opportunities.FirstOrDefault(o => o.Id == id);
        if (opp != null)
        {
            var title = opp.Title;
            _context.Opportunities.Remove(opp);
            _context.SaveChanges();
            TempData["Success"] = $"Opportunity '{title}' deleted.";
        }
        else
        {
            TempData["Error"] = "Opportunity not found.";
        }
        return RedirectToAction(nameof(Opportunities));
    }

    private bool IsAdminLoggedIn()
    {
        return HttpContext.Session.GetString("Role") == UserRole.Admin.ToString();
    }

    private IActionResult RedirectToAdminLogin()
    {
        TempData["Info"] = "Admin login required.";
        return RedirectToAction(nameof(Login));
    }
}
