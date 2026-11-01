using CampusCrew.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusCrew.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<OrganizerProfile> OrganizerProfiles { get; set; } = null!;
    public DbSet<Skill> Skills { get; set; } = null!;
    public DbSet<StudentSkill> StudentSkills { get; set; } = null!;
    public DbSet<Opportunity> Opportunities { get; set; } = null!;
    public DbSet<OpportunitySkill> OpportunitySkills { get; set; } = null!;
    public DbSet<Application> Applications { get; set; } = null!;
    public DbSet<Portfolio> Portfolios { get; set; } = null!;
    public DbSet<Collaboration> Collaborations { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Unique User Email
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // 1-to-1: User <-> Student
        modelBuilder.Entity<Student>()
            .HasOne(s => s.User)
            .WithOne(u => u.Student)
            .HasForeignKey<Student>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Student>()
            .HasIndex(s => s.UserId)
            .IsUnique();

        // 1-to-1: User <-> OrganizerProfile
        modelBuilder.Entity<OrganizerProfile>()
            .HasOne(op => op.User)
            .WithOne(u => u.OrganizerProfile)
            .HasForeignKey<OrganizerProfile>(op => op.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrganizerProfile>()
            .HasIndex(op => op.UserId)
            .IsUnique();

        // 1-to-Many: User (Organizer) -> Opportunity
        modelBuilder.Entity<Opportunity>()
            .HasOne(o => o.Organizer)
            .WithMany(u => u.Opportunities)
            .HasForeignKey(o => o.OrganizerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Many-to-Many: Student <-> Skill via StudentSkill (prevent duplicates)
        modelBuilder.Entity<StudentSkill>()
            .HasIndex(ss => new { ss.StudentId, ss.SkillId })
            .IsUnique();

        modelBuilder.Entity<StudentSkill>()
            .HasOne(ss => ss.Student)
            .WithMany(s => s.StudentSkills)
            .HasForeignKey(ss => ss.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StudentSkill>()
            .HasOne(ss => ss.Skill)
            .WithMany(sc => sc.StudentSkills)
            .HasForeignKey(ss => ss.SkillId)
            .OnDelete(DeleteBehavior.Cascade);

        // Many-to-Many: Opportunity <-> Skill via OpportunitySkill (prevent duplicates)
        modelBuilder.Entity<OpportunitySkill>()
            .HasIndex(os => new { os.OpportunityId, os.SkillId })
            .IsUnique();

        modelBuilder.Entity<OpportunitySkill>()
            .HasOne(os => os.Opportunity)
            .WithMany(o => o.OpportunitySkills)
            .HasForeignKey(os => os.OpportunityId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OpportunitySkill>()
            .HasOne(os => os.Skill)
            .WithMany(sc => sc.OpportunitySkills)
            .HasForeignKey(os => os.SkillId)
            .OnDelete(DeleteBehavior.Cascade);

        // 1-to-Many: Opportunity -> Application
        modelBuilder.Entity<Application>()
            .HasOne(a => a.Opportunity)
            .WithMany(o => o.Applications)
            .HasForeignKey(a => a.OpportunityId)
            .OnDelete(DeleteBehavior.Cascade);

        // 1-to-Many: Student -> Application (Restrict to avoid multiple cascade paths)
        modelBuilder.Entity<Application>()
            .HasOne(a => a.Student)
            .WithMany(s => s.Applications)
            .HasForeignKey(a => a.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Prevent duplicate applications from same student to same opportunity
        modelBuilder.Entity<Application>()
            .HasIndex(a => new { a.StudentId, a.OpportunityId })
            .IsUnique();

        // 1-to-Many: Student -> Portfolio
        modelBuilder.Entity<Portfolio>()
            .HasOne(p => p.Student)
            .WithMany(s => s.Portfolios)
            .HasForeignKey(p => p.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Peer-to-Peer Collaboration: Requester & Receiver
        modelBuilder.Entity<Collaboration>()
            .HasOne(c => c.Requester)
            .WithMany(s => s.SentCollaborations)
            .HasForeignKey(c => c.RequesterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Collaboration>()
            .HasOne(c => c.Receiver)
            .WithMany(s => s.ReceivedCollaborations)
            .HasForeignKey(c => c.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
