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
            // Create multiple teams so team-based aggregation and filters make sense
            var teams = new List<Team>
            {
                new Team { Name = "Team Alpha" },
                new Team { Name = "Team Beta" },
                new Team { Name = "Team Gamma" },
                new Team { Name = "Team Delta" },
                new Team { Name = "Team Ops" }
            };
            context.Teams.AddRange(teams);
            context.SaveChanges();

            // -------- Users --------
            // Spread users across teams so that:
            // - some teams are small, some large
            // - team performance summary has meaningful variation
            var users = new List<User>
            {
                new User { Name = "Alice",      TeamId = teams[0].Id },
                new User { Name = "Bob",        TeamId = teams[0].Id },
                new User { Name = "Charlie",    TeamId = teams[0].Id },
                new User { Name = "Diana",      TeamId = teams[1].Id },
                new User { Name = "Eve",        TeamId = teams[1].Id },
                new User { Name = "Frank",      TeamId = teams[1].Id },
                new User { Name = "Grace",      TeamId = teams[2].Id },
                new User { Name = "Heidi",      TeamId = teams[2].Id },
                new User { Name = "Ivan",       TeamId = teams[2].Id },
                new User { Name = "Judy",       TeamId = teams[2].Id },
                new User { Name = "Kevin",      TeamId = teams[3].Id },
                new User { Name = "Laura",      TeamId = teams[3].Id },
                new User { Name = "Mallory",    TeamId = teams[3].Id },
                new User { Name = "Niaj",       TeamId = teams[4].Id },
                new User { Name = "Olivia",     TeamId = teams[4].Id },
                new User { Name = "Peggy",      TeamId = teams[4].Id },
                new User { Name = "Rupert",     TeamId = teams[4].Id },
                new User { Name = "Sybil",      TeamId = teams[4].Id }
            };
            context.Users.AddRange(users);
            context.SaveChanges();

            // -------- Projects --------
            // Multiple projects so queries over different dimensions make sense
            var projects = new List<Project>
            {
                new Project { Name = "Project X" },
                new Project { Name = "Project Y" },
                new Project { Name = "Migration" },
                new Project { Name = "Refactor Core" },
                new Project { Name = "Reporting Dashboard" }
            };
            context.Projects.AddRange(projects);
            context.SaveChanges();

            // -------- Tasks --------
            var random = new Random();
            var statuses = new[] { "Pending", "InProgress", "Completed", "Blocked" };
            var tasks = new List<TaskEntity>();

            // Create a reasonably large and realistic dataset for benchmarks:
            // - tasks spread over the last 18 months
            // - mix of completed / in-progress / pending / blocked
            // - each user gets a different load to simulate real teams
            foreach (var user in users)
            {
                // Between 100 and 400 tasks per user for a few thousand total rows
                var taskCountForUser = random.Next(100, 401);

                for (int i = 0; i < taskCountForUser; i++)
                {
                    var status = statuses[random.Next(statuses.Length)];

                    // Created sometime in the last 18 months
                    var daysBack = random.Next(0, 18 * 30);
                    var createdAt = DateTime.UtcNow.AddDays(-daysBack);

                    // Some tasks are completed within 1–30 days after creation
                    DateTime? completedAt = null;
                    if (status == "Completed")
                    {
                        completedAt = createdAt.AddDays(random.Next(1, 31));
                        if (completedAt > DateTime.UtcNow)
                        {
                            // Clamp to "now" so we don't end up with future completions
                            completedAt = DateTime.UtcNow;
                        }
                    }

                    tasks.Add(new TaskEntity
                    {
                        Title = $"Task {i + 1} for {user.Name} ({user.TeamId})",
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
