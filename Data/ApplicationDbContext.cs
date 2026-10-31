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
    public DbSet<Skill> Skills { get; set; } = null!;
    public DbSet<StudentSkill> StudentSkills { get; set; } = null!;
    public DbSet<Opportunity> Opportunities { get; set; } = null!;
    public DbSet<OpportunitySkill> OpportunitySkills { get; set; } = null!;
    public DbSet<Application> Applications { get; set; } = null!;
    public DbSet<Portfolio> Portfolios { get; set; } = null!;
    public DbSet<Collaboration> Collaborations { get; set; } = null!;
    public DbSet<Review> Reviews { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1-to-1: User <-> Student
        modelBuilder.Entity<Student>()
            .HasOne(s => s.User)
            .WithOne(u => u.Student)
            .HasForeignKey<Student>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Many-to-Many: Student <-> Skill via StudentSkill
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

        // Many-to-Many: Opportunity <-> Skill via OpportunitySkill
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

        // 1-to-Many: Student -> Application (Restrict to avoid multiple cascade)
        modelBuilder.Entity<Application>()
            .HasOne(a => a.Student)
            .WithMany(s => s.Applications)
            .HasForeignKey(a => a.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Collaboration: Requester & Receiver
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

        // Review relationships
        modelBuilder.Entity<Review>()
            .HasOne(r => r.Reviewer)
            .WithMany()
            .HasForeignKey(r => r.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.Student)
            .WithMany(s => s.Reviews)
            .HasForeignKey(r => r.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.Opportunity)
            .WithMany(o => o.Reviews)
            .HasForeignKey(r => r.OpportunityId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.Collaboration)
            .WithMany()
            .HasForeignKey(r => r.CollaborationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
