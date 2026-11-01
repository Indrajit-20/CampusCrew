using CampusCrew.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CampusCrew.Data;

public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext context)
    {
        bool needsReset = false;
        try
        {
            context.Database.Migrate();
            // Verify if the current database schema matches the updated models
            _ = context.Users.Select(u => u.PasswordHash).FirstOrDefault();
            _ = context.Opportunities.Select(o => o.CreatedAt).FirstOrDefault();
        }
        catch
        {
            needsReset = true;
        }

        if (needsReset)
        {
            try
            {
                // Drop obsolete local database if schema was mismatched from previous version
                context.Database.EnsureDeleted();
                context.Database.Migrate();
            }
            catch
            {
                context.Database.EnsureCreated();
            }
        }

        if (context.Users.Any())
        {
            return; // DB has already been seeded
        }

        var hasher = new PasswordHasher<User>();

        // 1. Seed Skills
        var skillNames = new[] {
            "Photography", "Video Editing", "Graphic Design", "Programming",
            "Gaming", "Music", "Singing", "Beauty", "Content Creation",
            "Writing", "Tutoring", "Event Management"
        };
        var skills = skillNames.Select(name => new Skill { Name = name }).ToList();
        context.Skills.AddRange(skills);
        context.SaveChanges();

        // 2. Seed Users with Hashed Passwords
        var users = new List<User>
        {
            new User { Name = "Rohan Sharma", Email = "rohan@campuscrew.demo", Role = UserRole.Student },
            new User { Name = "Vikram Singh (Campus Event Head)", Email = "club@campuscrew.demo", Role = UserRole.Organizer },
            new User { Name = "Prof. Rajesh Kumar (Admin)", Email = "admin@campuscrew.demo", Role = UserRole.Admin },
            new User { Name = "Priya Patel", Email = "priya@campuscrew.demo", Role = UserRole.Student },
            new User { Name = "Rahul Verma", Email = "rahul@campuscrew.demo", Role = UserRole.Student },
            new User { Name = "Ananya Iyer", Email = "ananya@campuscrew.demo", Role = UserRole.Student },
            new User { Name = "Aditya Joshi", Email = "aditya@campuscrew.demo", Role = UserRole.Student },
            new User { Name = "Aarav Mehta", Email = "aarav@campuscrew.demo", Role = UserRole.Student }
        };

        foreach (var user in users)
        {
            user.PasswordHash = hasher.HashPassword(user, "Campus123");
        }

        context.Users.AddRange(users);
        context.SaveChanges();

        var uRohan = users[0];
        var uOrg = users[1];
        var uPriya = users[3];
        var uRahul = users[4];
        var uAnanya = users[5];
        var uAditya = users[6];
        var uAarav = users[7];

        // 3. Seed Organizer Profile
        var orgProfile = new OrganizerProfile
        {
            UserId = uOrg.Id,
            OrganizationName = "Campus Cultural & Tech Committee",
            OrganizationType = "College Club",
            College = "Delhi University",
            Location = "New Delhi",
            Description = "Official student activities council organizing intra and inter-college cultural, technical, and sports events.",
            ContactNumber = "+91 98765 43210"
        };
        context.OrganizerProfiles.Add(orgProfile);
        context.SaveChanges();

        // 4. Seed Students
        var sRohan = new Student
        {
            UserId = uRohan.Id,
            College = "Delhi University",
            Course = "BCA",
            Year = 3,
            Location = "New Delhi",
            Bio = "Enthusiastic BCA student passionate about event photography, video editing, and campus social media.",
            Availability = "Weekends and evenings"
        };

        var sPriya = new Student
        {
            UserId = uPriya.Id,
            College = "Gujarat University",
            Course = "B.Des",
            Year = 2,
            Location = "Ahmedabad",
            Bio = "Creative design student focused on branding, event banners, posters, and digital artwork.",
            Availability = "Flexible"
        };

        var sRahul = new Student
        {
            UserId = uRahul.Id,
            College = "Mumbai University",
            Course = "B.Tech (IT)",
            Year = 3,
            Location = "Mumbai",
            Bio = "Full-stack web developer building practical software for student hackathons and clubs.",
            Availability = "Weekday evenings"
        };

        var sAnanya = new Student
        {
            UserId = uAnanya.Id,
            College = "Anna University",
            Course = "B.Com",
            Year = 2,
            Location = "Chennai",
            Bio = "Music coordinator and vocal artist who loves organizing campus concerts and cultural gatherings.",
            Availability = "Weekends"
        };

        var sAditya = new Student
        {
            UserId = uAditya.Id,
            College = "Pune University",
            Course = "BCA",
            Year = 1,
            Location = "Pune",
            Bio = "First-year computer science student eager to collaborate on tech projects and learn full-stack web dev.",
            Availability = "Flexible"
        };

        var sAarav = new Student
        {
            UserId = uAarav.Id,
            College = "Bangalore University",
            Course = "B.Sc Computer Science",
            Year = 2,
            Location = "Bengaluru",
            Bio = "Creative coder passionate about front-end interfaces, writing, and campus community building.",
            Availability = "Weekends"
        };

        context.Students.AddRange(sRohan, sPriya, sRahul, sAnanya, sAditya, sAarav);
        context.SaveChanges();

        // 5. Link skills to students
        void AddStudentSkill(Student student, string skillName)
        {
            var sk = skills.FirstOrDefault(s => s.Name == skillName);
            if (sk != null)
                context.StudentSkills.Add(new StudentSkill { StudentId = student.Id, SkillId = sk.Id });
        }

        AddStudentSkill(sRohan, "Photography");
        AddStudentSkill(sRohan, "Video Editing");
        AddStudentSkill(sRohan, "Content Creation");

        AddStudentSkill(sPriya, "Graphic Design");
        AddStudentSkill(sPriya, "Content Creation");
        AddStudentSkill(sPriya, "Writing");

        AddStudentSkill(sRahul, "Programming");
        AddStudentSkill(sRahul, "Gaming");
        AddStudentSkill(sRahul, "Event Management");

        AddStudentSkill(sAnanya, "Music");
        AddStudentSkill(sAnanya, "Singing");
        AddStudentSkill(sAnanya, "Event Management");

        AddStudentSkill(sAditya, "Programming");
        AddStudentSkill(sAditya, "Writing");
        AddStudentSkill(sAditya, "Tutoring");

        AddStudentSkill(sAarav, "Programming");
        AddStudentSkill(sAarav, "Graphic Design");
        AddStudentSkill(sAarav, "Content Creation");

        context.SaveChanges();

        // 6. Seed Portfolios
        context.Portfolios.AddRange(new[]
        {
            new Portfolio { StudentId = sRohan.Id, ProjectTitle = "Annual Campus Fest 2025", Description = "Lead photographer covering 3 days of student music and sports events.", SkillName = "Photography", Role = "Lead Photographer", ProjectDate = new DateTime(2025, 11, 12), ImageUrl = "", ExternalLink = "https://instagram.com" },
            new Portfolio { StudentId = sRohan.Id, ProjectTitle = "Robotics Club Teaser Reel", Description = "Edited 60-second teaser reel for annual bot-battle robotics club.", SkillName = "Video Editing", Role = "Video Editor", ProjectDate = new DateTime(2026, 1, 15), ImageUrl = "", ExternalLink = "" },
            new Portfolio { StudentId = sPriya.Id, ProjectTitle = "Hackathon Brand Identity", Description = "Designed visual banners, identity, and attendee badges for 400+ participants.", SkillName = "Graphic Design", Role = "Brand Designer", ProjectDate = new DateTime(2025, 12, 1), ImageUrl = "", ExternalLink = "" },
            new Portfolio { StudentId = sRahul.Id, ProjectTitle = "HackPortal Web Application", Description = "Live scoring, judging and submission portal for college hackathon.", SkillName = "Programming", Role = "Full-stack Developer", ProjectDate = new DateTime(2026, 2, 10), ImageUrl = "", ExternalLink = "https://github.com" },
            new Portfolio { StudentId = sAnanya.Id, ProjectTitle = "Acoustic Campus Night", Description = "Organized an open-air acoustic music evening for 200+ students.", SkillName = "Music", Role = "Lead Vocalist & Coordinator", ProjectDate = new DateTime(2026, 1, 20), ImageUrl = "", ExternalLink = "" }
        });
        context.SaveChanges();

        // 7. Seed Opportunities (Dynamic sensible future dates for viva/demo presentation)
        var opp1 = new Opportunity
        {
            OrganizerId = uOrg.Id,
            Title = "National Youth Fest Coverage",
            Description = "We are seeking skilled student photographers and video editors to document our upcoming 3-day youth festival.",
            OpportunityDate = DateTime.Today.AddDays(25),
            Location = "University Main Auditorium",
            RequiredMembers = 6,
            Deadline = DateTime.Today.AddDays(15),
            OpportunityType = OpportunityType.Paid,
            Status = StatusType.Accepted
        };

        var opp2 = new Opportunity
        {
            OrganizerId = uOrg.Id,
            Title = "Student Innovation Demo Day",
            Description = "Build registration portals and promotional graphics for our upcoming student startup demo showcase.",
            OpportunityDate = DateTime.Today.AddDays(20),
            Location = "Innovation Center, Tech Campus",
            RequiredMembers = 4,
            Deadline = DateTime.Today.AddDays(12),
            OpportunityType = OpportunityType.Collaboration,
            Status = StatusType.Accepted
        };

        var opp3 = new Opportunity
        {
            OrganizerId = uOrg.Id,
            Title = "Inter-College Cultural & Music Evening",
            Description = "Join the stage and backstage organizing team for an energetic evening of student live music and dance.",
            OpportunityDate = DateTime.Today.AddDays(30),
            Location = "Open Air Amphitheatre",
            RequiredMembers = 8,
            Deadline = DateTime.Today.AddDays(18),
            OpportunityType = OpportunityType.Unpaid,
            Status = StatusType.Accepted
        };

        context.Opportunities.AddRange(opp1, opp2, opp3);
        context.SaveChanges();

        // Link Opportunity Skills
        void AddOppSkill(Opportunity opp, string skillName)
        {
            var sk = skills.FirstOrDefault(s => s.Name == skillName);
            if (sk != null)
                context.OpportunitySkills.Add(new OpportunitySkill { OpportunityId = opp.Id, SkillId = sk.Id });
        }

        AddOppSkill(opp1, "Photography");
        AddOppSkill(opp1, "Video Editing");
        AddOppSkill(opp1, "Content Creation");
        AddOppSkill(opp2, "Programming");
        AddOppSkill(opp2, "Graphic Design");
        AddOppSkill(opp2, "Writing");
        AddOppSkill(opp3, "Music");
        AddOppSkill(opp3, "Singing");
        AddOppSkill(opp3, "Event Management");
        context.SaveChanges();

        // 8. Seed Applications
        context.Applications.AddRange(new[]
        {
            new Application { OpportunityId = opp1.Id, StudentId = sPriya.Id, Message = "I would love to design the official event banners and posters.", AppliedAt = DateTime.UtcNow.AddDays(-2), Status = StatusType.Pending },
            new Application { OpportunityId = opp1.Id, StudentId = sRohan.Id, Message = "Experienced with festival event photography and reels.", AppliedAt = DateTime.UtcNow.AddDays(-4), Status = StatusType.Accepted },
            new Application { OpportunityId = opp2.Id, StudentId = sRahul.Id, Message = "I can build the web registration portal using ASP.NET Core.", AppliedAt = DateTime.UtcNow.AddDays(-1), Status = StatusType.Completed },
            new Application { OpportunityId = opp3.Id, StudentId = sAnanya.Id, Message = "Excited to perform acoustic vocals and manage stage sounds.", AppliedAt = DateTime.UtcNow.AddDays(-5), Status = StatusType.Completed }
        });
        context.SaveChanges();

        // 9. Seed Peer-to-Peer Collaborations
        context.Collaborations.AddRange(new[]
        {
            new Collaboration
            {
                RequesterId = sPriya.Id,
                ReceiverId = sRohan.Id,
                ProjectTitle = "College Magazine Photojournalism",
                Message = "Hi Rohan! Would you like to collaborate on the photography for our upcoming campus magazine issue?",
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                Status = StatusType.Pending
            },
            new Collaboration
            {
                RequesterId = sRohan.Id,
                ReceiverId = sRahul.Id,
                ProjectTitle = "Student Portfolio Portal",
                Message = "Hi Rahul! Want to build an open-source portfolio showcase template together?",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                Status = StatusType.Pending
            }
        });
        context.SaveChanges();
    }
}
