using System.Text.Json.Serialization;
using CampusCrew.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

// Configure JSON options to avoid circular reference loops
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

// Root landing endpoint so visiting http://localhost:5238 never shows a 404
app.MapGet("/", () => Results.Content(@"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='utf-8'/>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'/>
    <title>CampusCrew Web API</title>
    <style>
        body { font-family: system-ui, -apple-system, sans-serif; background-color: #f8f9fa; color: #212529; padding: 40px 20px; line-height: 1.6; }
        .box { max-width: 600px; margin: 0 auto; background: #ffffff; border: 1px solid #dee2e6; border-radius: 6px; padding: 28px; }
        h1 { color: #52734D; font-size: 1.4rem; margin-top: 0; }
        .badge { display: inline-block; padding: 3px 10px; background: #eef4ed; color: #3e5b39; border: 1px solid #d5e2d3; border-radius: 4px; font-size: 0.8rem; font-weight: 600; margin-bottom: 12px; }
        ul { padding-left: 20px; margin-top: 10px; }
        li { margin-bottom: 8px; }
        a { color: #52734D; font-weight: 600; text-decoration: none; }
        a:hover { text-decoration: underline; }
        .footer { margin-top: 20px; padding-top: 15px; border-top: 1px solid #dee2e6; font-size: 0.85rem; color: #6c757d; }
    </style>
</head>
<body>
    <div class='box'>
        <span class='badge'>Status: Live & Running</span>
        <h1>CampusCrew Web API</h1>
        <p>The backend REST service is active and connected to SQL Server. Here are the live endpoints:</p>
        <ul>
            <li><a href='/api/opportunities'>/api/opportunities</a> &mdash; Opportunities JSON</li>
            <li><a href='/api/students'>/api/students</a> &mdash; Students Directory JSON</li>
            <li><a href='/api/skills'>/api/skills</a> &mdash; Skills Catalog JSON</li>
            <li><a href='/api/stats'>/api/stats</a> &mdash; Platform Statistics JSON</li>
        </ul>
        <div class='footer'>
            Consumed by: <strong>CampusCrewWeb</strong> (Frontend MVC UI)
        </div>
    </div>
</body>
</html>", "text/html"));

app.MapControllers();

app.Run();
