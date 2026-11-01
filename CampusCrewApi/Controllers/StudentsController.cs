using CampusCrew.Data;
using CampusCrew.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusCrew.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public StudentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET /api/students?search=&skill=&college=&location=&availability=
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StudentDto>>> GetStudents(
        [FromQuery] string? search,
        [FromQuery] string? skill,
        [FromQuery] string? college,
        [FromQuery] string? location,
        [FromQuery] string? availability)
    {
        var query = _context.Students
            .Include(s => s.User)
            .Include(s => s.StudentSkills)
                .ThenInclude(ss => ss.Skill)
            .Include(s => s.Portfolios)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(x => (x.User != null && x.User.Name.Contains(s))
                                  || x.Course.Contains(s)
                                  || x.Bio.Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(skill))
        {
            var sk = skill.Trim();
            query = query.Where(x => x.StudentSkills.Any(ss => ss.Skill != null && ss.Skill.Name == sk));
        }

        if (!string.IsNullOrWhiteSpace(college))
        {
            query = query.Where(x => x.College.Contains(college.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            query = query.Where(x => x.Location.Contains(location.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(availability))
        {
            query = query.Where(x => x.Availability.Contains(availability.Trim()));
        }

        var list = await query
            .OrderByDescending(s => s.Id)
            .Select(s => new StudentDto
            {
                Id = s.Id,
                UserId = s.UserId,
                Name = s.User != null ? s.User.Name : string.Empty,
                Email = s.User != null ? s.User.Email : string.Empty,
                College = s.College,
                Course = s.Course,
                Year = s.Year,
                Location = s.Location,
                Bio = s.Bio,
                Availability = s.Availability,
                Skills = s.StudentSkills
                    .Where(ss => ss.Skill != null)
                    .Select(ss => ss.Skill!.Name)
                    .ToList(),
                Portfolios = s.Portfolios.Select(p => new PortfolioDto
                {
                    Id = p.Id,
                    ProjectTitle = p.ProjectTitle,
                    Description = p.Description,
                    ExternalLink = p.ExternalLink,
                    SkillName = p.SkillName
                }).ToList()
            })
            .ToListAsync();

        return Ok(list);
    }

    // GET /api/students/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<StudentDto>> GetStudent(int id)
    {
        var s = await _context.Students
            .Include(s => s.User)
            .Include(s => s.StudentSkills)
                .ThenInclude(ss => ss.Skill)
            .Include(s => s.Portfolios)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (s == null)
        {
            return NotFound(new { message = $"Student with ID {id} not found." });
        }

        var dto = new StudentDto
        {
            Id = s.Id,
            UserId = s.UserId,
            Name = s.User != null ? s.User.Name : string.Empty,
            Email = s.User != null ? s.User.Email : string.Empty,
            College = s.College,
            Course = s.Course,
            Year = s.Year,
            Location = s.Location,
            Bio = s.Bio,
            Availability = s.Availability,
            Skills = s.StudentSkills
                .Where(ss => ss.Skill != null)
                .Select(ss => ss.Skill!.Name)
                .ToList(),
            Portfolios = s.Portfolios.Select(p => new PortfolioDto
            {
                Id = p.Id,
                ProjectTitle = p.ProjectTitle,
                Description = p.Description,
                ExternalLink = p.ExternalLink,
                SkillName = p.SkillName
            }).ToList()
        };

        return Ok(dto);
    }
}
