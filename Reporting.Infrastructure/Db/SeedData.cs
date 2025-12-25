using Reporting.Domain.Entities;

namespace Reporting.Infrastructure.Db
{
    public static class SeedData
    {
        public static void Initialize(ReportingDbContext context)
        {
            // Database is already created/migrated in Program.cs
            // No need for EnsureCreated() when using migrations
            
            if (context.Teams.Any()) return; // اگر قبلا داده بودیم، کاری نکن

            // -------- Teams --------
            var teams = new List<Team>
            {
                new Team { Name = "Team Alpha" },
                new Team { Name = "Team Beta" }
            };
            context.Teams.AddRange(teams);
            context.SaveChanges();

            // -------- Users --------
                var users = new List<User>
            {
                new User { Name = "Alice", TeamId = teams[0].Id },
                new User { Name = "Bob", TeamId = teams[0].Id },
                new User { Name = "Charlie", TeamId = teams[1].Id },
                new User { Name = "Diana", TeamId = teams[1].Id }
            };
            context.Users.AddRange(users);
            context.SaveChanges();

            // -------- Projects --------
            var projects = new List<Project>
            {
                new Project { Name = "Project X" },
                new Project { Name = "Project Y" }
            };
            context.Projects.AddRange(projects);
            context.SaveChanges();

            // -------- Tasks --------
            var random = new Random();
            var statuses = new[] { "Pending", "InProgress", "Completed" };
            var tasks = new List<TaskEntity>();
            foreach (var user in users)
            {
                for (int i = 0; i < 5; i++) // Each user has 5 tasks
                {
                    var status = statuses[random.Next(statuses.Length)];
                    var createdAt = DateTime.UtcNow.AddDays(-random.Next(30));
                    DateTime? completedAt = status == "Completed"
                        ? createdAt.AddDays(random.Next(1, 10))
                        : null;

                        tasks.Add(new TaskEntity
                    {
                        Title = $"Task {i + 1} for {user.Name}",
                        Status = status,
                        UserId = user.Id,
                        TeamId = user.TeamId,
                        ProjectId = projects[random.Next(projects.Count)].Id,
                        CreatedAt = createdAt,
                        CompletedAt = completedAt
                    });
                }
            }

            context.Tasks.AddRange(tasks);
            context.SaveChanges();
        }
    }
}
