using CampusCrew.Data;
using CampusCrew.Dtos;
using CampusCrew.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusCrew.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OpportunitiesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public OpportunitiesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET /api/opportunities?search=&skill=&location=
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OpportunityDto>>> GetOpportunities(
        [FromQuery] string? search,
        [FromQuery] string? skill,
        [FromQuery] string? location)
    {
        var query = _context.Opportunities
            .Include(o => o.Organizer)
            .Include(o => o.OpportunitySkills)
                .ThenInclude(os => os.Skill)
            .Include(o => o.Applications)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(o => o.Title.Contains(s) || o.Description.Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(skill))
        {
            var sk = skill.Trim();
            query = query.Where(o => o.OpportunitySkills.Any(os => os.Skill != null && os.Skill.Name == sk));
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            var loc = location.Trim();
            query = query.Where(o => o.Location.Contains(loc));
        }

        var list = await query
            .OrderBy(o => o.Deadline)
            .Select(o => new OpportunityDto
            {
                Id = o.Id,
                OrganizerId = o.OrganizerId,
                OrganizerName = o.Organizer != null ? o.Organizer.Name : string.Empty,
                Title = o.Title,
                Description = o.Description,
                OpportunityType = o.OpportunityType,
                Location = o.Location,
                College = o.College,
                RequiredMembers = o.RequiredMembers,
                CreatedAt = o.CreatedAt,
                Deadline = o.Deadline,
                OpportunityDate = o.OpportunityDate,
                Status = o.Status,
                AdditionalRequirements = o.AdditionalRequirements,
                RequiredSkills = o.OpportunitySkills
                    .Where(os => os.Skill != null)
                    .Select(os => os.Skill!.Name)
                    .ToList(),
                ApplicationCount = o.Applications.Count
            })
            .ToListAsync();

        return Ok(list);
    }

    // GET /api/opportunities/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OpportunityDto>> GetOpportunity(int id)
    {
        var o = await _context.Opportunities
            .Include(o => o.Organizer)
            .Include(o => o.OpportunitySkills)
                .ThenInclude(os => os.Skill)
            .Include(o => o.Applications)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);

        if (o == null)
        {
            return NotFound(new { message = $"Opportunity with ID {id} not found." });
        }

        var dto = new OpportunityDto
        {
            Id = o.Id,
            OrganizerId = o.OrganizerId,
            OrganizerName = o.Organizer != null ? o.Organizer.Name : string.Empty,
            Title = o.Title,
            Description = o.Description,
            OpportunityType = o.OpportunityType,
            Location = o.Location,
            College = o.College,
            RequiredMembers = o.RequiredMembers,
            CreatedAt = o.CreatedAt,
            Deadline = o.Deadline,
            OpportunityDate = o.OpportunityDate,
            Status = o.Status,
            AdditionalRequirements = o.AdditionalRequirements,
            RequiredSkills = o.OpportunitySkills
                .Where(os => os.Skill != null)
                .Select(os => os.Skill!.Name)
                .ToList(),
            ApplicationCount = o.Applications.Count
        };

        return Ok(dto);
    }

    // POST /api/opportunities
    [HttpPost]
    public async Task<ActionResult<OpportunityDto>> CreateOpportunity([FromBody] CreateOpportunityDto input)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var opp = new Opportunity
        {
            OrganizerId = input.OrganizerId,
            Title = input.Title.Trim(),
            Description = input.Description.Trim(),
            OpportunityType = input.OpportunityType,
            Location = input.Location?.Trim() ?? string.Empty,
            College = input.College?.Trim() ?? string.Empty,
            RequiredMembers = input.RequiredMembers,
            CreatedAt = DateTime.UtcNow,
            Deadline = input.Deadline,
            OpportunityDate = input.OpportunityDate,
            Status = StatusType.Accepted,
            AdditionalRequirements = input.AdditionalRequirements?.Trim() ?? string.Empty
        };

        _context.Opportunities.Add(opp);
        await _context.SaveChangesAsync();

        if (input.Skills != null && input.Skills.Count > 0)
        {
            foreach (var skillName in input.Skills.Distinct())
            {
                var skill = await _context.Skills.FirstOrDefaultAsync(s => s.Name.ToLower() == skillName.ToLower());
                if (skill != null)
                {
                    _context.OpportunitySkills.Add(new OpportunitySkill
                    {
                        OpportunityId = opp.Id,
                        SkillId = skill.Id
                    });
                }
            }
            await _context.SaveChangesAsync();
        }

        return CreatedAtAction(nameof(GetOpportunity), new { id = opp.Id }, new OpportunityDto
        {
            Id = opp.Id,
            OrganizerId = opp.OrganizerId,
            Title = opp.Title,
            Description = opp.Description,
            OpportunityType = opp.OpportunityType,
            Location = opp.Location,
            College = opp.College,
            RequiredMembers = opp.RequiredMembers,
            CreatedAt = opp.CreatedAt,
            Deadline = opp.Deadline,
            OpportunityDate = opp.OpportunityDate,
            Status = opp.Status,
            AdditionalRequirements = opp.AdditionalRequirements,
            RequiredSkills = input.Skills ?? new()
        });
    }

    // DELETE /api/opportunities/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteOpportunity(int id)
    {
        var opp = await _context.Opportunities.FindAsync(id);
        if (opp == null)
        {
            return NotFound();
        }

        _context.Opportunities.Remove(opp);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

