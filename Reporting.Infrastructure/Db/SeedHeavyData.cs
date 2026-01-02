using Microsoft.EntityFrameworkCore;
using Reporting.Domain.Entities;
using Reporting.Infrastructure.Db;

namespace Reporting.Infrastructure.Db;

/// <summary>
/// Script to seed heavy data for performance benchmarking.
/// This creates a large dataset (50K+ tasks) to make performance differences between
/// EF Core, Dapper, and Stored Procedures more noticeable.
/// 
/// Usage: Call SeedHeavyData.Seed() after initial seed data is created.
/// </summary>
public static class SeedHeavyData
{
    /// <summary>
    /// Seeds heavy data for benchmarking purposes.
    /// Creates additional teams, users, projects, and a large number of tasks
    /// to simulate a production-like workload.
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="targetTaskCount">Target total number of tasks to create (default: 50,000)</param>
    public static void Seed(ReportingDbContext context, int targetTaskCount = 50_000)
    {
        Console.WriteLine($"Starting heavy data seeding. Target: {targetTaskCount:N0} tasks...");

        // Get existing teams to distribute new users across
        var existingTeams = context.Teams.ToList();
        if (!existingTeams.Any())
        {
            throw new InvalidOperationException("Base seed data must be created first. Run SeedData.Initialize() before this.");
        }

        var random = new Random(42); // Fixed seed for reproducibility
        var statuses = new[] { "Pending", "InProgress", "Completed", "Blocked" };
        
        // Create additional teams for more realistic distribution
        var additionalTeams = new List<Team>();
        for (int i = 0; i < 10; i++)
        {
            additionalTeams.Add(new Team { Name = $"Team {i + 1}" });
        }
        context.Teams.AddRange(additionalTeams);
        context.SaveChanges();
        
        var allTeams = context.Teams.ToList();
        Console.WriteLine($"Created {additionalTeams.Count} additional teams. Total teams: {allTeams.Count}");

        // Create additional users distributed across teams
        var additionalUsers = new List<User>();
        foreach (var team in allTeams)
        {
            // Each team gets 15-25 users
            var usersPerTeam = random.Next(15, 26);
            for (int i = 0; i < usersPerTeam; i++)
            {
                additionalUsers.Add(new User
                {
                    Name = $"User_{team.Name}_{i + 1}",
                    TeamId = team.Id
                });
            }
        }
        context.Users.AddRange(additionalUsers);
        context.SaveChanges();
        
        var allUsers = context.Users.ToList();
        Console.WriteLine($"Created {additionalUsers.Count} additional users. Total users: {allUsers.Count}");

        // Create additional projects
        var additionalProjects = new List<Project>();
        for (int i = 0; i < 20; i++)
        {
            additionalProjects.Add(new Project { Name = $"Project_{i + 1}" });
        }
        context.Projects.AddRange(additionalProjects);
        context.SaveChanges();
        
        var allProjects = context.Projects.ToList();
        Console.WriteLine($"Created {additionalProjects.Count} additional projects. Total projects: {allProjects.Count}");

        // Create tasks in batches for better performance
        var batchSize = 1000;
        var totalTasksCreated = 0;
        var tasks = new List<TaskEntity>(batchSize);
        var startDate = DateTime.UtcNow.AddMonths(-24); // 2 years of data
        var endDate = DateTime.UtcNow;

        Console.WriteLine($"Creating tasks in batches of {batchSize}...");

        while (totalTasksCreated < targetTaskCount)
        {
            tasks.Clear();
            var remainingTasks = targetTaskCount - totalTasksCreated;
            var currentBatchSize = Math.Min(batchSize, remainingTasks);

            for (int i = 0; i < currentBatchSize; i++)
            {
                var user = allUsers[random.Next(allUsers.Count)];
                var status = statuses[random.Next(statuses.Length)];
                
                // Distribute tasks across the 2-year period
                var daysAgo = random.Next(0, 730);
                var createdAt = startDate.AddDays(daysAgo);
                
                DateTime? completedAt = null;
                if (status == "Completed")
                {
                    // Completed tasks have completion date 1-30 days after creation
                    var completionDelay = random.Next(1, 31);
                    completedAt = createdAt.AddDays(completionDelay);
                    if (completedAt > endDate)
                        completedAt = endDate;
                }

                tasks.Add(new TaskEntity
                {
                    Title = $"Task_{totalTasksCreated + i + 1}",
                    Status = status,
                    UserId = user.Id,
                    TeamId = user.TeamId,
                    ProjectId = allProjects[random.Next(allProjects.Count)].Id,
                    CreatedAt = createdAt,
                    CompletedAt = completedAt
                });
            }

            context.Tasks.AddRange(tasks);
            context.SaveChanges();
            totalTasksCreated += currentBatchSize;

            if (totalTasksCreated % 10000 == 0)
            {
                Console.WriteLine($"Created {totalTasksCreated:N0} tasks so far...");
            }
        }

        Console.WriteLine($"Heavy data seeding completed! Total tasks: {totalTasksCreated:N0}");
        Console.WriteLine($"Database now contains:");
        Console.WriteLine($"  - Teams: {allTeams.Count}");
        Console.WriteLine($"  - Users: {allUsers.Count}");
        Console.WriteLine($"  - Projects: {allProjects.Count}");
        Console.WriteLine($"  - Tasks: {totalTasksCreated:N0}");
    }
}

