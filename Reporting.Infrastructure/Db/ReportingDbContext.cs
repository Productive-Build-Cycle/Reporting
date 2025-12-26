using Microsoft.EntityFrameworkCore;
using Reporting.Application.DTOs;
using Reporting.Domain.Entities;

namespace Reporting.Infrastructure.Db;

public sealed class ReportingDbContext : DbContext
{
    public ReportingDbContext(DbContextOptions<ReportingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Team> Teams { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<TaskEntity> Tasks { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Team
        modelBuilder.Entity<Team>()
                .HasKey(t => t.Id);

        // User
        modelBuilder.Entity<User>()
                .HasKey(u => u.Id);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Team)
            .WithMany(t => t.Users)
            .HasForeignKey(u => u.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        // Project
        modelBuilder.Entity<Project>()
                .HasKey(p => p.Id);

        // Task
        modelBuilder.Entity<TaskEntity>()
                .HasKey(t => t.Id);

        modelBuilder.Entity<TaskEntity>()
            .HasOne(t => t.User)
            .WithMany(u => u.Tasks)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict); // NO CASCADE

        modelBuilder.Entity<TaskEntity>()
            .HasOne(t => t.Team)
            .WithMany()
            .HasForeignKey(t => t.TeamId)
            .OnDelete(DeleteBehavior.Restrict); // NO CASCADE

        modelBuilder.Entity<TaskEntity>()
            .HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for reporting performance
        modelBuilder.Entity<TaskEntity>()
            .HasIndex(t => t.UserId);

        modelBuilder.Entity<TaskEntity>()
            .HasIndex(t => t.TeamId);

        modelBuilder.Entity<TaskEntity>()
            .HasIndex(t => t.Status);

        modelBuilder.Entity<TaskEntity>()
            .HasIndex(t => t.CreatedAt);

        modelBuilder.Entity<TaskEntity>()
            .HasIndex(t => t.CompletedAt);

        // Reporting DTO
        modelBuilder.Entity<TasksPerUserRequestDto>(eb =>
        {
            eb.HasNoKey();
            eb.ToView(null); // no db object
        });

    }
}
