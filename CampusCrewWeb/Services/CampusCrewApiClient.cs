using System.Net.Http.Json;
using CampusCrew.Dtos;
using CampusCrew.Models;

namespace CampusCrew.Services;

public class CampusCrewApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<CampusCrewApiClient> _logger;

    public CampusCrewApiClient(HttpClient http, ILogger<CampusCrewApiClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<List<Opportunity>> GetOpportunitiesAsync(string? search = null, string? skill = null, string? location = null)
    {
        try
        {
            var query = new List<string>();
            if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search.Trim())}");
            if (!string.IsNullOrWhiteSpace(skill)) query.Add($"skill={Uri.EscapeDataString(skill.Trim())}");
            if (!string.IsNullOrWhiteSpace(location)) query.Add($"location={Uri.EscapeDataString(location.Trim())}");

            var url = "api/opportunities" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
            var dtos = await _http.GetFromJsonAsync<List<OpportunityDto>>(url);
            if (dtos == null) return new();

            return dtos.Select(d => new Opportunity
            {
                Id = d.Id,
                OrganizerId = d.OrganizerId,
                Organizer = new User { Id = d.OrganizerId, Name = d.OrganizerName },
                Title = d.Title,
                Description = d.Description,
                OpportunityType = d.OpportunityType,
                Location = d.Location,
                College = d.College,
                RequiredMembers = d.RequiredMembers,
                CreatedAt = d.CreatedAt,
                Deadline = d.Deadline,
                OpportunityDate = d.OpportunityDate,
                Status = d.Status,
                AdditionalRequirements = d.AdditionalRequirements,
                OpportunitySkills = d.RequiredSkills.Select(s => new OpportunitySkill
                {
                    Skill = new Skill { Name = s }
                }).ToList()
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling API GetOpportunities");
            return new();
        }
    }

    public async Task<Opportunity?> GetOpportunityByIdAsync(int id)
    {
        try
        {
            var d = await _http.GetFromJsonAsync<OpportunityDto>($"api/opportunities/{id}");
            if (d == null) return null;

            return new Opportunity
            {
                Id = d.Id,
                OrganizerId = d.OrganizerId,
                Organizer = new User { Id = d.OrganizerId, Name = d.OrganizerName },
                Title = d.Title,
                Description = d.Description,
                OpportunityType = d.OpportunityType,
                Location = d.Location,
                College = d.College,
                RequiredMembers = d.RequiredMembers,
                CreatedAt = d.CreatedAt,
                Deadline = d.Deadline,
                OpportunityDate = d.OpportunityDate,
                Status = d.Status,
                AdditionalRequirements = d.AdditionalRequirements,
                OpportunitySkills = d.RequiredSkills.Select(s => new OpportunitySkill
                {
                    Skill = new Skill { Name = s }
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling API GetOpportunityById for {Id}", id);
            return null;
        }
    }

    public async Task<List<Student>> GetStudentsAsync(string? search = null, string? skill = null, string? college = null, string? location = null, string? availability = null)
    {
        try
        {
            var query = new List<string>();
            if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search.Trim())}");
            if (!string.IsNullOrWhiteSpace(skill)) query.Add($"skill={Uri.EscapeDataString(skill.Trim())}");
            if (!string.IsNullOrWhiteSpace(college)) query.Add($"college={Uri.EscapeDataString(college.Trim())}");
            if (!string.IsNullOrWhiteSpace(location)) query.Add($"location={Uri.EscapeDataString(location.Trim())}");
            if (!string.IsNullOrWhiteSpace(availability)) query.Add($"availability={Uri.EscapeDataString(availability.Trim())}");

            var url = "api/students" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
            var dtos = await _http.GetFromJsonAsync<List<StudentDto>>(url);
            if (dtos == null) return new();

            return dtos.Select(d => new Student
            {
                Id = d.Id,
                UserId = d.UserId,
                User = new User { Id = d.UserId, Name = d.Name, Email = d.Email },
                College = d.College,
                Course = d.Course,
                Year = d.Year,
                Location = d.Location,
                Bio = d.Bio,
                Availability = d.Availability,
                StudentSkills = d.Skills.Select(s => new StudentSkill
                {
                    Skill = new Skill { Name = s }
                }).ToList(),
                Portfolios = d.Portfolios.Select(p => new Portfolio
                {
                    Id = p.Id,
                    ProjectTitle = p.ProjectTitle,
                    Description = p.Description,
                    ExternalLink = p.ExternalLink,
                    SkillName = p.SkillName
                }).ToList()
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling API GetStudents");
            return new();
        }
    }

    public async Task<Student?> GetStudentByIdAsync(int id)
    {
        try
        {
            var d = await _http.GetFromJsonAsync<StudentDto>($"api/students/{id}");
            if (d == null) return null;

            return new Student
            {
                Id = d.Id,
                UserId = d.UserId,
                User = new User { Id = d.UserId, Name = d.Name, Email = d.Email },
                College = d.College,
                Course = d.Course,
                Year = d.Year,
                Location = d.Location,
                Bio = d.Bio,
                Availability = d.Availability,
                StudentSkills = d.Skills.Select(s => new StudentSkill
                {
                    Skill = new Skill { Name = s }
                }).ToList(),
                Portfolios = d.Portfolios.Select(p => new Portfolio
                {
                    Id = p.Id,
                    ProjectTitle = p.ProjectTitle,
                    Description = p.Description,
                    ExternalLink = p.ExternalLink,
                    SkillName = p.SkillName
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling API GetStudentById for {Id}", id);
            return null;
        }
    }

    public async Task<List<Skill>> GetSkillsAsync()
    {
        try
        {
            var dtos = await _http.GetFromJsonAsync<List<SkillDto>>("api/skills");
            if (dtos == null) return new();

            return dtos.Select(s => new Skill
            {
                Id = s.Id,
                Name = s.Name
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling API GetSkills");
            return new();
        }
    }

    public async Task<HomeStatsDto?> GetHomeStatsAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<HomeStatsDto>("api/stats");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling API GetHomeStats");
            return null;
        }
    }
}

