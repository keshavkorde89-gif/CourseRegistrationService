using CourseRegistration.Model;
using Microsoft.EntityFrameworkCore;

namespace CourseRegistration.Data;

/// <summary>
/// Defines the application's EF Core entity sets and deterministic demo data.
/// </summary>
public sealed class RegistrationDbContext : DbContext
{
    public RegistrationDbContext(DbContextOptions<RegistrationDbContext> options)
        : base(options)
    {
    }

    public DbSet<CourseOffering> Offerings => Set<CourseOffering>();
    public DbSet<Learner> Learners => Set<Learner>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<WaitlistEntry> WaitlistEntries => Set<WaitlistEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CourseOffering>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired();

            // HasData keeps the demonstration offering defined with the model.
            entity.HasData(new CourseOffering
            {
                Id = 1,
                Name = "Introduction to .NET",
                Capacity = 2
            });
        });

        modelBuilder.Entity<Learner>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired();

            // Deterministic learners make the demo and tests repeatable.
            entity.HasData(
                new Learner { Id = 1, Name = "Amit", HasPrerequisite = true },
                new Learner { Id = 2, Name = "Priya", HasPrerequisite = true },
                new Learner { Id = 3, Name = "Sneha", HasPrerequisite = true },
                new Learner { Id = 4, Name = "Rahul", HasPrerequisite = true },
                new Learner { Id = 5, Name = "Vikas", HasPrerequisite = false });
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(x => x.Id);
            // These indexes document intended uniqueness. The InMemory provider
            // does not enforce relational unique constraints.
            entity.HasIndex(x => new { x.OfferingId, x.LearnerId });
        });

        modelBuilder.Entity<WaitlistEntry>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.OfferingId, x.LearnerId });
        });
    }
}
