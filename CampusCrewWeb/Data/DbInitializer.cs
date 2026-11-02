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
                context.Database.EnsureDeleted();
                context.Database.Migrate();
            }
            catch
            {
                context.Database.EnsureCreated();
            }
        }

        // If we already have 30 or more users, don't re-seed
        if (context.Users.Count() >= 30)
        {
            return;
        }

        // If fewer than 30 users exist, refresh database to load complete 30+ dataset
        try
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }
        catch
        {
            // fallback
        }

        var hasher = new PasswordHasher<User>();

        // 1. Seed Skills
        var skillNames = new[] {
            "Programming", "Web Development", "Python", "Graphic Design",
            "Video Editing", "Photography", "Content Creation", "Writing",
            "UI/UX Design", "Machine Learning", "Event Management", "Music",
            "Singing", "Public Speaking", "Digital Marketing", "Tutoring",
            "Robotics", "Gaming"
        };
        var skills = skillNames.Select(name => new Skill { Name = name }).ToList();
        context.Skills.AddRange(skills);
        context.SaveChanges();

        // 2. Seed 32 Users (1 Admin, 7 Organizations, 24 Students)
        var users = new List<User>
        {
            // Admin
            new User { Name = "Prof. Rajesh Kumar (Admin)", Email = "admin@campuscrew.demo", Role = UserRole.Admin },

            // 7 Organizers / Clubs
            new User { Name = "Vikram Singh (Campus Event Head)", Email = "events@campuscrew.demo", Role = UserRole.Organizer },
            new User { Name = "Neha Deshmukh (GDG & Coding Club)", Email = "coding@campuscrew.demo", Role = UserRole.Organizer },
            new User { Name = "Arjun Kapoor (Robotics Society)", Email = "robotics@campuscrew.demo", Role = UserRole.Organizer },
            new User { Name = "Meera Nair (Campus Media House)", Email = "media@campuscrew.demo", Role = UserRole.Organizer },
            new User { Name = "Kunal Shah (E-Cell Entrepreneurship)", Email = "ecell@campuscrew.demo", Role = UserRole.Organizer },
            new User { Name = "Shreya Banerjee (Literary & Drama Club)", Email = "drama@campuscrew.demo", Role = UserRole.Organizer },
            new User { Name = "Devendra Rawat (Sports Council)", Email = "sports@campuscrew.demo", Role = UserRole.Organizer },

            // 24 Students
            new User { Name = "Rohan Sharma", Email = "rohan@campuscrew.demo", Role = UserRole.Student },
            new User { Name = "Priya Patel", Email = "priya@campuscrew.demo", Role = UserRole.Student },
            new User { Name = "Rahul Verma", Email = "rahul@campuscrew.demo", Role = UserRole.Student },
            new User { Name = "Ananya Iyer", Email = "ananya@campuscrew.demo", Role = UserRole.Student },
            new User { Name = "Aditya Joshi", Email = "aditya@campuscrew.demo", Role = UserRole.Student },
            new User { Name = "Aarav Mehta", Email = "aarav@campuscrew.demo", Role = UserRole.Student },
            new User { Name = "Sneha Kulkarni", Email = "sneha@campuscrew.demo", Role = UserRole.Student },
            new User { Name = "Tanmay Bhatia", Email = "tanmay@campuscrew.demo", Role = UserRole.Student },
            new User { Name = "Divya Nambiar", Email = "divya@campuscrew.demo", Role = UserRole.Student },
            new User { Name = "Karan Singhania", Email = "karan@campuscrew.demo", Role = UserRole.Student },
            new User { Name = "Ishita Sen", Email = "ishita@campuscrew.demo", Role = UserRole.Student },
            new User { Name = "Manish Tiwari", Email = "manish@campuscrew.demo", Role = UserRole.Student },
            new User { Name = "Riya Saxena", Email = "riya@campuscrew.demo", Role = UserRole.Student },
            new User { Name = "Harsh Vardhan", Email = "harsh@campuscrew.demo", Role = UserRole.Student },
            new User { Name = "Pooja Hegde", Email = "pooja@campuscrew.demo", Role = UserRole.Student },
            new User { Name = "Gaurav Malhotra", Email = "gaurav@campuscrew.demo", Role = UserRole.Student },
            new User { Name = "Kavya Reddy", Email = "kavya@campuscrew.demo", Role = UserRole.Student },
            new User { Name = "Nikhil Chawla", Email = "nikhil@campuscrew.demo", Role = UserRole.Student },
            new User { Name = "Aishwarya Rao", Email = "aishwarya@campuscrew.demo", Role = UserRole.Student },
            new User { Name = "Siddharth Bose", Email = "siddharth@campuscrew.demo", Role = UserRole.Student },
            new User { Name = "Simran Kaur", Email = "simran@campuscrew.demo", Role = UserRole.Student },
            new User { Name = "Varun Pillai", Email = "varun@campuscrew.demo", Role = UserRole.Student },
            new User { Name = "Bhavna Mishra", Email = "bhavna@campuscrew.demo", Role = UserRole.Student },
            new User { Name = "Yashwant Rao", Email = "yashwant@campuscrew.demo", Role = UserRole.Student }
        };

        foreach (var user in users)
        {
            user.PasswordHash = hasher.HashPassword(user, "Campus123");
        }

        context.Users.AddRange(users);
        context.SaveChanges();

        // 3. Seed Organizer Profiles
        var organizerProfiles = new List<OrganizerProfile>
        {
            new OrganizerProfile
            {
                UserId = users[1].Id,
                OrganizationName = "Campus Cultural & Tech Council",
                OrganizationType = "College Council",
                College = "Delhi University",
                Location = "New Delhi",
                Description = "Central student body organizing intra- and inter-college cultural, technological, and sports festivals.",
                ContactNumber = "+91 98111 22334"
            },
            new OrganizerProfile
            {
                UserId = users[2].Id,
                OrganizationName = "GDG on Campus & Coding Club",
                OrganizationType = "Technical Society",
                College = "Mumbai University",
                Location = "Mumbai",
                Description = "Student developer club conducting hackathons, open-source sprints, and coding workshops.",
                ContactNumber = "+91 98222 33445"
            },
            new OrganizerProfile
            {
                UserId = users[3].Id,
                OrganizationName = "Autonomous Robotics Society",
                OrganizationType = "Robotics Lab",
                College = "Pune University",
                Location = "Pune",
                Description = "Hardware, drones, and AI robotics community collaborating on national bot competitions.",
                ContactNumber = "+91 98333 44556"
            },
            new OrganizerProfile
            {
                UserId = users[4].Id,
                OrganizationName = "Campus Media & Film Collective",
                OrganizationType = "Media Club",
                College = "Bangalore University",
                Location = "Bengaluru",
                Description = "Campus photography, short film production, social media coverage, and live festival broadcasting.",
                ContactNumber = "+91 98444 55667"
            },
            new OrganizerProfile
            {
                UserId = users[5].Id,
                OrganizationName = "Campus E-Cell (Entrepreneurship)",
                OrganizationType = "Incubator Club",
                College = "Gujarat University",
                Location = "Ahmedabad",
                Description = "Empowering student founders through pitch competitions, seed grants, and mentor networking.",
                ContactNumber = "+91 98555 66778"
            },
            new OrganizerProfile
            {
                UserId = users[6].Id,
                OrganizationName = "Literary, Drama & Music Society",
                OrganizationType = "Performing Arts",
                College = "Anna University",
                Location = "Chennai",
                Description = "Theatre productions, poetry slams, debate championships, and acoustic musical performances.",
                ContactNumber = "+91 98666 77889"
            },
            new OrganizerProfile
            {
                UserId = users[7].Id,
                OrganizationName = "Inter-College Sports Association",
                OrganizationType = "Athletics Board",
                College = "Delhi University",
                Location = "New Delhi",
                Description = "Organizing annual sports meets, esports tourneys, badminton, football, and cricket leagues.",
                ContactNumber = "+91 98777 88990"
            }
        };

        context.OrganizerProfiles.AddRange(organizerProfiles);
        context.SaveChanges();

        // 4. Seed 24 Students
        var studentData = new (int userIndex, string college, string course, int year, string location, string bio, string avail)[]
        {
            (8, "Delhi University", "BCA", 3, "New Delhi", "Enthusiastic BCA student passionate about event photography, video editing, and campus social media.", "Weekends and evenings"),
            (9, "Gujarat University", "B.Des", 2, "Ahmedabad", "Creative design student focused on branding, event banners, posters, and digital artwork.", "Flexible"),
            (10, "Mumbai University", "B.Tech (IT)", 3, "Mumbai", "Full-stack web developer building practical software for student hackathons and clubs.", "Weekday evenings"),
            (11, "Anna University", "B.Com", 2, "Chennai", "Music coordinator and vocal artist who loves organizing campus concerts and cultural gatherings.", "Weekends"),
            (12, "Pune University", "BCA", 1, "Pune", "First-year computer science student eager to collaborate on tech projects and learn full-stack web dev.", "Flexible"),
            (13, "Bangalore University", "B.Sc Computer Science", 2, "Bengaluru", "Creative coder passionate about front-end interfaces, writing, and campus community building.", "Weekends"),
            (14, "Pune University", "B.Tech (Computer Science)", 3, "Pune", "Machine Learning enthusiast working on Python AI models, research papers, and computer vision.", "Weekday evenings"),
            (15, "Delhi University", "BBA", 2, "New Delhi", "Event manager with experience in sponsorship handling, campus logistics, and festival coordination.", "Full-time available"),
            (16, "Kerala University", "B.Tech (Electronics)", 4, "Kochi", "Embedded systems and IoT hobbyist building sensor networks and smart robotics prototypes.", "Weekends"),
            (17, "Mumbai University", "BMS", 2, "Mumbai", "Social media strategist running campaigns for campus fests with 10k+ reach.", "Flexible"),
            (18, "Calcutta University", "B.A. English", 3, "Kolkata", "Content writer, editor, and debate finalist with a love for literature, blogs, and scriptwriting.", "Evenings"),
            (19, "Lucknow University", "BCA", 2, "Lucknow", "Python backend developer proficient in SQL databases and REST APIs.", "Weekends"),
            (20, "Delhi University", "B.Sc IT", 3, "New Delhi", "UI/UX designer proficient in Figma, prototyping, user research, and responsive web design.", "Flexible"),
            (21, "BITS Pilani", "B.E. Computer Science", 3, "Pilani", "Competitive programmer and hackathon winner specializing in C++ and distributed systems.", "Evenings"),
            (22, "Bangalore University", "B.Com", 1, "Bengaluru", "Classical and western vocalist interested in jamming sessions and inter-college music fests.", "Weekends"),
            (23, "Punjab University", "B.Tech (Mechanical)", 2, "Chandigarh", "CAD modeling, 3D printing enthusiast, and robotics team mechanical lead.", "Flexible"),
            (24, "Osmania University", "B.Tech (IT)", 3, "Hyderabad", "Android and mobile developer building student productivity and campus navigation tools.", "Weekends"),
            (25, "Delhi University", "B.Com (Hons)", 2, "New Delhi", "Finance and sponsorship coordinator for major university cultural festivals.", "Evenings"),
            (26, "Mumbai University", "B.Des (Animation)", 3, "Mumbai", "Motion graphic animator and 2D illustrator making animated explainers and fest trailers.", "Flexible"),
            (27, "Jadavpur University", "B.E. Electronics", 4, "Kolkata", "Audio engineer and electric guitar player managing sound engineering for campus live gigs.", "Weekends"),
            (28, "Amity University", "BCA", 2, "Noida", "Frontend developer proficient in React, HTML, CSS, JavaScript, and Tailwind.", "Evenings"),
            (29, "Madras University", "B.Sc Data Science", 2, "Chennai", "Data science student analyzing campus sports stats and building predictive dashboards.", "Flexible"),
            (30, "BHU Varanasi", "B.A. Mass Comm", 3, "Varanasi", "Photojournalist and campus reporter covering cultural events and sports journalism.", "Weekends"),
            (31, "Jaipur National University", "B.Tech (CSE)", 1, "Jaipur", "Aspiring web developer, competitive gamer, and esports tournament caster.", "Flexible")
        };

        var studentEntities = new List<Student>();
        foreach (var d in studentData)
        {
            var s = new Student
            {
                UserId = users[d.userIndex].Id,
                College = d.college,
                Course = d.course,
                Year = d.year,
                Location = d.location,
                Bio = d.bio,
                Availability = d.avail
            };
            studentEntities.Add(s);
        }

        context.Students.AddRange(studentEntities);
        context.SaveChanges();

        // 5. Link Skills to Students
        void AddSkill(Student st, string name)
        {
            var sk = skills.FirstOrDefault(x => x.Name.ToLower() == name.ToLower());
            if (sk != null)
                context.StudentSkills.Add(new StudentSkill { StudentId = st.Id, SkillId = sk.Id });
        }

        // Assign realistic skill pairs
        for (int i = 0; i < studentEntities.Count; i++)
        {
            var st = studentEntities[i];
            switch (i % 8)
            {
                case 0: AddSkill(st, "Photography"); AddSkill(st, "Video Editing"); AddSkill(st, "Content Creation"); break;
                case 1: AddSkill(st, "Graphic Design"); AddSkill(st, "UI/UX Design"); AddSkill(st, "Content Creation"); break;
                case 2: AddSkill(st, "Programming"); AddSkill(st, "Web Development"); AddSkill(st, "Python"); break;
                case 3: AddSkill(st, "Music"); AddSkill(st, "Singing"); AddSkill(st, "Event Management"); break;
                case 4: AddSkill(st, "Machine Learning"); AddSkill(st, "Python"); AddSkill(st, "Programming"); break;
                case 5: AddSkill(st, "Writing"); AddSkill(st, "Public Speaking"); AddSkill(st, "Digital Marketing"); break;
                case 6: AddSkill(st, "Robotics"); AddSkill(st, "Programming"); AddSkill(st, "Tutoring"); break;
                case 7: AddSkill(st, "Gaming"); AddSkill(st, "Event Management"); AddSkill(st, "Video Editing"); break;
            }
        }
        context.SaveChanges();

        // 6. Seed Portfolios
        context.Portfolios.AddRange(new[]
        {
            new Portfolio { StudentId = studentEntities[0].Id, ProjectTitle = "Annual Campus Fest 2025", Description = "Lead photographer covering 3 days of student music and sports events.", SkillName = "Photography", Role = "Lead Photographer", ProjectDate = DateTime.Today.AddDays(-120), ExternalLink = "https://instagram.com" },
            new Portfolio { StudentId = studentEntities[0].Id, ProjectTitle = "Robotics Club Teaser Reel", Description = "Edited 60-second teaser reel for annual bot-battle robotics club.", SkillName = "Video Editing", Role = "Video Editor", ProjectDate = DateTime.Today.AddDays(-80), ExternalLink = "" },
            new Portfolio { StudentId = studentEntities[1].Id, ProjectTitle = "Hackathon Brand Identity", Description = "Designed visual banners, identity, and attendee badges for 400+ participants.", SkillName = "Graphic Design", Role = "Brand Designer", ProjectDate = DateTime.Today.AddDays(-60), ExternalLink = "" },
            new Portfolio { StudentId = studentEntities[2].Id, ProjectTitle = "HackPortal Web Application", Description = "Live scoring, judging and submission portal for college hackathon.", SkillName = "Web Development", Role = "Full-stack Developer", ProjectDate = DateTime.Today.AddDays(-45), ExternalLink = "https://github.com" },
            new Portfolio { StudentId = studentEntities[3].Id, ProjectTitle = "Acoustic Campus Night", Description = "Organized an open-air acoustic music evening for 200+ students.", SkillName = "Music", Role = "Lead Vocalist & Coordinator", ProjectDate = DateTime.Today.AddDays(-30), ExternalLink = "" },
            new Portfolio { StudentId = studentEntities[6].Id, ProjectTitle = "Campus Attendance Face-Recognizer", Description = "Python OpenCV & PyTorch model for automated lecture attendance.", SkillName = "Machine Learning", Role = "AI Lead", ProjectDate = DateTime.Today.AddDays(-20), ExternalLink = "https://github.com" },
            new Portfolio { StudentId = studentEntities[10].Id, ProjectTitle = "Annual Magazine 'Verve' Issue", Description = "Curated, edited, and proofread 80-page annual student magazine.", SkillName = "Writing", Role = "Chief Editor", ProjectDate = DateTime.Today.AddDays(-50), ExternalLink = "" },
            new Portfolio { StudentId = studentEntities[13].Id, ProjectTitle = "ACM-ICPC Regionalist", Description = "Top 10 collegiate team in national algorithmic programming contest.", SkillName = "Programming", Role = "Team Lead", ProjectDate = DateTime.Today.AddDays(-75), ExternalLink = "" }
        });
        context.SaveChanges();

        // 7. Seed 10 Opportunities (Diverse types, future dates)
        var opportunities = new List<Opportunity>
        {
            new Opportunity
            {
                OrganizerId = users[1].Id,
                Title = "National Youth Fest 2026 - Media Coverage",
                Description = "Seeking student photographers, drone operators, and video editors to document our flagship 3-day inter-college festival with 5,000+ attendees.",
                OpportunityDate = DateTime.Today.AddDays(25),
                Location = "University Main Auditorium, New Delhi",
                College = "Delhi University",
                RequiredMembers = 6,
                Deadline = DateTime.Today.AddDays(15),
                OpportunityType = OpportunityType.Paid,
                Status = StatusType.Accepted
            },
            new Opportunity
            {
                OrganizerId = users[2].Id,
                Title = "HackCampus 36-Hour Hackathon Mentors & Devs",
                Description = "Looking for frontend and backend developers to build the registration portal and mentor hackathon participants during our 36-hour hack sprint.",
                OpportunityDate = DateTime.Today.AddDays(20),
                Location = "Computer Science Center, Mumbai",
                College = "Mumbai University",
                RequiredMembers = 5,
                Deadline = DateTime.Today.AddDays(12),
                OpportunityType = OpportunityType.Collaboration,
                Status = StatusType.Accepted
            },
            new Opportunity
            {
                OrganizerId = users[3].Id,
                Title = "RoboWar Combat Bot Mechanics & Programmers",
                Description = "Autonomous Robotics Society is designing a 15kg combat bot for national RoboWars. Need students with Python/C++ control and CAD design skills.",
                OpportunityDate = DateTime.Today.AddDays(35),
                Location = "Mechanical Workshop, Tech Campus",
                College = "Pune University",
                RequiredMembers = 4,
                Deadline = DateTime.Today.AddDays(18),
                OpportunityType = OpportunityType.Collaboration,
                Status = StatusType.Accepted
            },
            new Opportunity
            {
                OrganizerId = users[4].Id,
                Title = "Campus Short Film Festival - Post Production Crew",
                Description = "We are shooting an official campus docu-series and need video editors, color-graders, and sound designers to collaborate on the final cut.",
                OpportunityDate = DateTime.Today.AddDays(28),
                Location = "Media Lab, Bengaluru",
                College = "Bangalore University",
                RequiredMembers = 3,
                Deadline = DateTime.Today.AddDays(14),
                OpportunityType = OpportunityType.Paid,
                Status = StatusType.Accepted
            },
            new Opportunity
            {
                OrganizerId = users[5].Id,
                Title = "Student Startup Showcase & Pitch Deck Designers",
                Description = "E-Cell is hosting 20 student startups pitching to angel investors. We need graphic designers and content creators to format one-pagers and pitch slides.",
                OpportunityDate = DateTime.Today.AddDays(22),
                Location = "Incubation Center, Ahmedabad",
                College = "Gujarat University",
                RequiredMembers = 4,
                Deadline = DateTime.Today.AddDays(10),
                OpportunityType = OpportunityType.Collaboration,
                Status = StatusType.Accepted
            },
            new Opportunity
            {
                OrganizerId = users[6].Id,
                Title = "Inter-College Acoustic Live Jam & Stage Management",
                Description = "Join the stage crew for our annual open-air acoustic night. Looking for vocalists, instrumentalists, and stage logistics coordinators.",
                OpportunityDate = DateTime.Today.AddDays(30),
                Location = "Open Air Amphitheatre, Chennai",
                College = "Anna University",
                RequiredMembers = 8,
                Deadline = DateTime.Today.AddDays(16),
                OpportunityType = OpportunityType.Unpaid,
                Status = StatusType.Accepted
            },
            new Opportunity
            {
                OrganizerId = users[7].Id,
                Title = "Campus Esports Championship Organizers & Casters",
                Description = "Hosting inter-hostel Valorant and FIFA tournaments. Need tournament managers, live casters, and streaming overlays graphic designer.",
                OpportunityDate = DateTime.Today.AddDays(18),
                Location = "Student Center Gaming Arena",
                College = "Delhi University",
                RequiredMembers = 4,
                Deadline = DateTime.Today.AddDays(9),
                OpportunityType = OpportunityType.Paid,
                Status = StatusType.Accepted
            },
            new Opportunity
            {
                OrganizerId = users[2].Id,
                Title = "Open Source Contribution Sprint for Beginners",
                Description = "Collaborative sprint helping 1st and 2nd year students make their first pull requests in ASP.NET Core and React repositories.",
                OpportunityDate = DateTime.Today.AddDays(15),
                Location = "Online / Hybrid",
                College = "Mumbai University",
                RequiredMembers = 10,
                Deadline = DateTime.Today.AddDays(7),
                OpportunityType = OpportunityType.Collaboration,
                Status = StatusType.Accepted
            },
            new Opportunity
            {
                OrganizerId = users[1].Id,
                Title = "Clean Green Campus Drive - Campaign Designers",
                Description = "Student council sustainability drive. Need creative writers and digital marketers to design awareness posters and social media reels.",
                OpportunityDate = DateTime.Today.AddDays(24),
                Location = "North Campus, New Delhi",
                College = "Delhi University",
                RequiredMembers = 5,
                Deadline = DateTime.Today.AddDays(13),
                OpportunityType = OpportunityType.Unpaid,
                Status = StatusType.Accepted
            },
            new Opportunity
            {
                OrganizerId = users[5].Id,
                Title = "Full-Stack Dev for Campus Buy/Sell Marketplace",
                Description = "Incubated student venture looking for a co-founder / lead developer skilled in C#, ASP.NET Core, and SQL Server to launch a campus marketplace.",
                OpportunityDate = DateTime.Today.AddDays(40),
                Location = "Ahmedabad / Remote",
                College = "Gujarat University",
                RequiredMembers = 2,
                Deadline = DateTime.Today.AddDays(20),
                OpportunityType = OpportunityType.Paid,
                Status = StatusType.Accepted
            }
        };

        context.Opportunities.AddRange(opportunities);
        context.SaveChanges();

        // 8. Link Opportunity Skills
        void LinkOppSkill(Opportunity opp, string skillName)
        {
            var sk = skills.FirstOrDefault(s => s.Name.ToLower() == skillName.ToLower());
            if (sk != null)
                context.OpportunitySkills.Add(new OpportunitySkill { OpportunityId = opp.Id, SkillId = sk.Id });
        }

        LinkOppSkill(opportunities[0], "Photography");
        LinkOppSkill(opportunities[0], "Video Editing");
        LinkOppSkill(opportunities[0], "Content Creation");

        LinkOppSkill(opportunities[1], "Programming");
        LinkOppSkill(opportunities[1], "Web Development");
        LinkOppSkill(opportunities[1], "Graphic Design");

        LinkOppSkill(opportunities[2], "Robotics");
        LinkOppSkill(opportunities[2], "Programming");
        LinkOppSkill(opportunities[2], "Python");

        LinkOppSkill(opportunities[3], "Video Editing");
        LinkOppSkill(opportunities[3], "Content Creation");

        LinkOppSkill(opportunities[4], "Graphic Design");
        LinkOppSkill(opportunities[4], "UI/UX Design");
        LinkOppSkill(opportunities[4], "Content Creation");

        LinkOppSkill(opportunities[5], "Music");
        LinkOppSkill(opportunities[5], "Singing");
        LinkOppSkill(opportunities[5], "Event Management");

        LinkOppSkill(opportunities[6], "Gaming");
        LinkOppSkill(opportunities[6], "Event Management");
        LinkOppSkill(opportunities[6], "Graphic Design");

        LinkOppSkill(opportunities[7], "Programming");
        LinkOppSkill(opportunities[7], "Web Development");

        LinkOppSkill(opportunities[8], "Digital Marketing");
        LinkOppSkill(opportunities[8], "Writing");

        LinkOppSkill(opportunities[9], "Web Development");
        LinkOppSkill(opportunities[9], "Programming");

        context.SaveChanges();

        // 9. Seed Sample Applications
        context.Applications.AddRange(new[]
        {
            new Application { OpportunityId = opportunities[0].Id, StudentId = studentEntities[0].Id, Message = "Experienced with festival photography and fast reel turnaround.", AppliedAt = DateTime.UtcNow.AddDays(-2), Status = StatusType.Accepted },
            new Application { OpportunityId = opportunities[0].Id, StudentId = studentEntities[1].Id, Message = "I can design the event banners and promotional posters.", AppliedAt = DateTime.UtcNow.AddDays(-3), Status = StatusType.Pending },
            new Application { OpportunityId = opportunities[1].Id, StudentId = studentEntities[2].Id, Message = "Full stack C# ASP.NET Core developer ready to assist with portals.", AppliedAt = DateTime.UtcNow.AddDays(-1), Status = StatusType.Accepted },
            new Application { OpportunityId = opportunities[2].Id, StudentId = studentEntities[8].Id, Message = "Hardware and electronics lead, worked on robot sensors previously.", AppliedAt = DateTime.UtcNow.AddDays(-4), Status = StatusType.Pending },
            new Application { OpportunityId = opportunities[5].Id, StudentId = studentEntities[3].Id, Message = "Classical and acoustic vocals, excited to lead the stage vocalists.", AppliedAt = DateTime.UtcNow.AddDays(-5), Status = StatusType.Accepted },
            new Application { OpportunityId = opportunities[6].Id, StudentId = studentEntities[23].Id, Message = "Experienced gaming caster and LAN tournament admin.", AppliedAt = DateTime.UtcNow.AddDays(-2), Status = StatusType.Accepted }
        });
        context.SaveChanges();

        // 10. Seed Sample Collaborations
        context.Collaborations.AddRange(new[]
        {
            new Collaboration
            {
                RequesterId = studentEntities[1].Id,
                ReceiverId = studentEntities[0].Id,
                ProjectTitle = "College Magazine Photojournalism",
                Message = "Hi Rohan! Would you like to collaborate on photography for our upcoming campus magazine issue?",
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                Status = StatusType.Pending
            },
            new Collaboration
            {
                RequesterId = studentEntities[0].Id,
                ReceiverId = studentEntities[2].Id,
                ProjectTitle = "Student Portfolio Portal",
                Message = "Hi Rahul! Want to build an open-source portfolio showcase template together?",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                Status = StatusType.Accepted
            },
            new Collaboration
            {
                RequesterId = studentEntities[4].Id,
                ReceiverId = studentEntities[6].Id,
                ProjectTitle = "Campus AI Chatbot",
                Message = "Hi Sneha, let's collaborate on building an AI question-answering assistant for college notices.",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                Status = StatusType.Pending
            }
        });
        context.SaveChanges();
    }
}
