namespace CampusCrew.Dtos;

public class PortfolioDto
{
    public int Id { get; set; }
    public string ProjectTitle { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ExternalLink { get; set; } = string.Empty;
    public string SkillName { get; set; } = string.Empty;
}

public class StudentDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string College { get; set; } = string.Empty;
    public string Course { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string Availability { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = new();
    public List<PortfolioDto> Portfolios { get; set; } = new();
}
