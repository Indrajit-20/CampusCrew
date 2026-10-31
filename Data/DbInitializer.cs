using CampusCrew.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusCrew.Data;

public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext context)
    {
        context.Database.EnsureCreated();

        if (context.Users.Any())
        {
            return; // DB has been seeded
        }

        // 1. Seed Skills
        var skillNames = new[] {
            "Photography", "Video Editing", "Graphic Design", "Programming",
            "Gaming", "Music", "Singing", "Beauty", "Content Creation",
            "Writing", "Tutoring", "Event Management"
        };
        var skills = skillNames.Select(name => new Skill { Name = name }).ToList();
        context.Skills.AddRange(skills);
        context.SaveChanges();

        // 2. Seed Users with Real Indian Names
        var users = new List<User>
        {
            new User { Name = "Rohan Sharma", Email = "rohan@campuscrew.demo", Password = "Campus123", Role = UserRole.Student },
            new User { Name = "Vikram Singh (Campus Event Head)", Email = "club@campuscrew.demo", Password = "Campus123", Role = UserRole.Organizer },
            new User { Name = "Prof. Rajesh Kumar (Admin)", Email = "admin@campuscrew.demo", Password = "Campus123", Role = UserRole.Admin },
            new User { Name = "Priya Patel", Email = "priya@campuscrew.demo", Password = "Campus123", Role = UserRole.Student },
            new User { Name = "Rahul Verma", Email = "rahul@campuscrew.demo", Password = "Campus123", Role = UserRole.Student },
            new User { Name = "Ananya Iyer", Email = "ananya@campuscrew.demo", Password = "Campus123", Role = UserRole.Student },
            new User { Name = "Aditya Joshi", Email = "aditya@campuscrew.demo", Password = "Campus123", Role = UserRole.Student },
            new User { Name = "Aarav Mehta", Email = "aarav@campuscrew.demo", Password = "Campus123", Role = UserRole.Student }
        };
        context.Users.AddRange(users);
        context.SaveChanges();

        var uRohan = users[0];
        var uOrg = users[1];
        var uPriya = users[3];
        var uRahul = users[4];
        var uAnanya = users[5];
        var uAditya = users[6];
        var uAarav = users[7];

        // 3. Seed Students (Real Indian Colleges, Courses & Locations)
        var sRohan = new Student
        {
            UserId = uRohan.Id, Name = uRohan.Name, Email = uRohan.Email,
            College = "Delhi University", Course = "BCA", Year = 3,
            Location = "New Delhi",
            Bio = "Enthusiastic BCA student passionate about event photography, video editing, and campus social media.",
            Experience = "Handled official photography for university tech fest and cultural events.",
            Availability = "Weekends and evenings", Rating = 4.8, TotalReviews = 2
        };

        var sPriya = new Student
        {
            UserId = uPriya.Id, Name = uPriya.Name, Email = uPriya.Email,
            College = "Gujarat University", Course = "B.Des", Year = 2,
            Location = "Ahmedabad",
            Bio = "Creative design student focused on branding, event banners, posters, and digital artwork.",
            Experience = "Designed brand identities and social media creatives for college clubs.",
            Availability = "Flexible", Rating = 4.7, TotalReviews = 1
        };

        var sRahul = new Student
        {
            UserId = uRahul.Id, Name = uRahul.Name, Email = uRahul.Email,
            College = "Mumbai University", Course = "B.Tech (IT)", Year = 3,
            Location = "Mumbai",
            Bio = "Full-stack web developer building practical software for student hackathons and clubs.",
            Experience = "Built registration portals and live voting systems for university fests.",
            Availability = "Weekday evenings", Rating = 4.9, TotalReviews = 1
        };

        var sAnanya = new Student
        {
            UserId = uAnanya.Id, Name = uAnanya.Name, Email = uAnanya.Email,
            College = "Anna University", Course = "B.Com", Year = 2,
            Location = "Chennai",
            Bio = "Music coordinator and vocal artist who loves organizing campus concerts and cultural gatherings.",
            Experience = "Coordinated inter-college open mics and acoustic live music events.",
            Availability = "Weekends", Rating = 5.0, TotalReviews = 1
        };

        var sAditya = new Student
        {
            UserId = uAditya.Id, Name = uAditya.Name, Email = uAditya.Email,
            College = "Pune University", Course = "BCA", Year = 1,
            Location = "Pune",
            Bio = "First-year computer science student eager to collaborate on tech projects and learn full-stack web dev.",
            Experience = "Built personal portfolio website and helped coordinate campus coding workshop.",
            Availability = "Flexible", Rating = 5.0, TotalReviews = 0
        };

        var sAarav = new Student
        {
            UserId = uAarav.Id, Name = uAarav.Name, Email = uAarav.Email,
            College = "Bangalore University", Course = "B.Sc Computer Science", Year = 2,
            Location = "Bengaluru",
            Bio = "Creative coder passionate about front-end interfaces, writing, and campus community building.",
            Experience = "Active contributor to open-source student initiatives and technical blogs.",
            Availability = "Weekends", Rating = 4.9, TotalReviews = 0
        };

        context.Students.AddRange(sRohan, sPriya, sRahul, sAnanya, sAditya, sAarav);
        context.SaveChanges();

        // 4. Link skills to students
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

        // 5. Seed Portfolios
        context.Portfolios.AddRange(new[]
        {
            new Portfolio { StudentId = sRohan.Id, ProjectTitle = "Annual Campus Fest 2025", Description = "Lead photographer covering 3 days of student music and sports events.", SkillName = "Photography", Role = "Lead Photographer", ProjectDate = new DateTime(2025, 11, 12), ImageUrl = "", ExternalLink = "https://instagram.com" },
            new Portfolio { StudentId = sRohan.Id, ProjectTitle = "Robotics Club Teaser Reel", Description = "Edited 60-second teaser reel for annual bot-battle robotics club.", SkillName = "Video Editing", Role = "Video Editor", ProjectDate = new DateTime(2026, 1, 15), ImageUrl = "", ExternalLink = "" },
            new Portfolio { StudentId = sPriya.Id, ProjectTitle = "Hackathon Brand Identity", Description = "Designed visual banners, identity, and attendee badges for 400+ participants.", SkillName = "Graphic Design", Role = "Brand Designer", ProjectDate = new DateTime(2025, 12, 1), ImageUrl = "", ExternalLink = "" },
            new Portfolio { StudentId = sRahul.Id, ProjectTitle = "HackPortal Web Application", Description = "Live scoring, judging and submission portal for college hackathon.", SkillName = "Programming", Role = "Full-stack Developer", ProjectDate = new DateTime(2026, 2, 10), ImageUrl = "", ExternalLink = "https://github.com" },
            new Portfolio { StudentId = sAnanya.Id, ProjectTitle = "Acoustic Campus Night", Description = "Organized an open-air acoustic music evening for 200+ students.", SkillName = "Music", Role = "Lead Vocalist & Coordinator", ProjectDate = new DateTime(2026, 1, 20), ImageUrl = "", ExternalLink = "" }
        });
        context.SaveChanges();

        // 6. Seed Opportunities
        var opp1 = new Opportunity
        {
            OrganizerId = uOrg.Id,
            Title = "National Youth Fest 2026 Coverage",
            Description = "We are seeking skilled student photographers and video editors to document our 3-day youth festival.",
            OpportunityDate = new DateTime(2026, 10, 18),
            Location = "University Main Auditorium", College = "Delhi University",
            NumberOfParticipants = 6,
            ApplicationDeadline = new DateTime(2026, 9, 30),
            Type = OpportunityType.Paid,
            AdditionalRequirements = "DSLR or high-resolution smartphone required."
        };

        var opp2 = new Opportunity
        {
            OrganizerId = uOrg.Id,
            Title = "Student Innovation Demo Day",
            Description = "Build registration portals and promotional graphics for our upcoming student startup demo showcase.",
            OpportunityDate = new DateTime(2026, 11, 5),
            Location = "Innovation Center, Tech Campus", College = "Mumbai University",
            NumberOfParticipants = 4,
            ApplicationDeadline = new DateTime(2026, 10, 20),
            Type = OpportunityType.Collaboration,
            AdditionalRequirements = "Weekly team sync on Microsoft Teams or Zoom."
        };

        var opp3 = new Opportunity
        {
            OrganizerId = uOrg.Id,
            Title = "Inter-College Music & Cultural Evening",
            Description = "Join the stage and backstage organizing team for an energetic evening of student live music and dance.",
            OpportunityDate = new DateTime(2026, 9, 28),
            Location = "Open Air Amphitheatre", College = "Gujarat University",
            NumberOfParticipants = 10,
            ApplicationDeadline = new DateTime(2026, 9, 20),
            Type = OpportunityType.Unpaid,
            AdditionalRequirements = "Open to all passionate student artists and volunteers."
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

        // 7. Seed Applications
        context.Applications.AddRange(new[]
        {
            new Application { OpportunityId = opp1.Id, StudentId = sPriya.Id, Message = "I would love to design the official event banners and posters.", AppliedOn = DateTime.UtcNow.AddDays(-2), Status = StatusType.Pending },
            new Application { OpportunityId = opp1.Id, StudentId = sRohan.Id, Message = "Experienced with festival event photography and reels.", AppliedOn = DateTime.UtcNow.AddDays(-4), Status = StatusType.Accepted },
            new Application { OpportunityId = opp2.Id, StudentId = sRahul.Id, Message = "I can build the web registration portal using ASP.NET Core.", AppliedOn = DateTime.UtcNow.AddDays(-1), Status = StatusType.Completed },
            new Application { OpportunityId = opp3.Id, StudentId = sAnanya.Id, Message = "Excited to perform acoustic vocals and manage stage sounds.", AppliedOn = DateTime.UtcNow.AddDays(-5), Status = StatusType.Completed }
        });
        context.SaveChanges();

        // 8. Seed Collaborations
        context.Collaborations.AddRange(new[]
        {
            new Collaboration
            {
                RequesterId = sPriya.Id, ReceiverId = sRohan.Id,
                ProjectTitle = "College Magazine Photojournalism",
                Message = "Hi Rohan! Would you like to collaborate on the photography for our upcoming campus magazine issue?",
                CreatedAt = DateTime.UtcNow.AddDays(-3), Status = StatusType.Pending
            },
            new Collaboration
            {
                RequesterId = sRohan.Id, ReceiverId = sRahul.Id,
                ProjectTitle = "Student Portfolio Portal",
                Message = "Hi Rahul! Want to build an open-source portfolio showcase template together?",
                CreatedAt = DateTime.UtcNow.AddDays(-1), Status = StatusType.Pending
            }
        });
        context.SaveChanges();

        // 9. Seed Reviews
        context.Reviews.AddRange(new[]
        {
            new Review
            {
                ReviewerId = uOrg.Id, ReviewerName = uOrg.Name, ReviewerRole = "Organizer",
                StudentId = sRohan.Id, Rating = 5,
                Comment = "Rohan captured incredible high-quality photos for our youth fest. Highly punctual and creative!",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                RelatedEntityTitle = opp1.Title, OpportunityId = opp1.Id
            },
            new Review
            {
                ReviewerId = uRahul.Id, ReviewerName = uRahul.Name, ReviewerRole = "Student",
                StudentId = sRohan.Id, Rating = 5,
                Comment = "Great collaboration experience. Handled video editing with quick turnaround.",
                CreatedAt = DateTime.UtcNow.AddDays(-6),
                RelatedEntityTitle = "Fest Teaser Video"
            },
            new Review
            {
                ReviewerId = uOrg.Id, ReviewerName = uOrg.Name, ReviewerRole = "Organizer",
                StudentId = sPriya.Id, Rating = 5,
                Comment = "Priya designed excellent banner assets and social media kits.",
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                RelatedEntityTitle = opp2.Title, OpportunityId = opp2.Id
            }
        });
        context.SaveChanges();
    }
}
