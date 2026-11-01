using CampusCrew.Data;
using CampusCrew.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusCrew.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public StatsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<HomeStatsDto>> GetStats()
    {
        var stats = new HomeStatsDto
        {
            TotalStudents = await _context.Students.CountAsync(),
            TotalOpportunities = await _context.Opportunities.CountAsync(),
            TotalSkills = await _context.Skills.CountAsync(),
            TotalCollaborations = await _context.Collaborations.CountAsync()
        };

        return Ok(stats);
    }
}

