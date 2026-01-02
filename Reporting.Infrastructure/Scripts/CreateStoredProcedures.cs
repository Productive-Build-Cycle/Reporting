using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Reporting.Infrastructure.Scripts;

/// <summary>
/// Helper class to create stored procedures in the database.
/// This ensures all stored procedures required for benchmarking are available.
/// </summary>
public static class CreateStoredProcedures
{
    /// <summary>
    /// Creates all stored procedures required for the reporting service.
    /// </summary>
    public static void CreateAll(DbContext context)
    {
        var connectionString = context.Database.GetConnectionString();
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Connection string is not available.");

        using var connection = new SqlConnection(connectionString);
        connection.Open();

        // Get the SQL scripts from embedded resources or file system
        var scripts = new[]
        {
            GetScriptContent("sp_GetTeamPerformanceSummary.sql"),
            GetScriptContent("sp_GetTasksPerUser.sql"),
            GetScriptContent("sp_GetCompletedTasksPerWeek.sql")
        };

        foreach (var script in scripts)
        {
            if (string.IsNullOrWhiteSpace(script))
                continue;

            try
            {
                // Split script by GO statements
                var batches = script.Split(new[] { "\r\nGO\r\n", "\r\nGO\n", "\nGO\n", "\nGO\r\n", "GO\r\n", "GO\n" }, 
                    StringSplitOptions.RemoveEmptyEntries);

                foreach (var batch in batches)
                {
                    var trimmed = batch.Trim();
                    if (string.IsNullOrWhiteSpace(trimmed))
                        continue;

                    using var command = new SqlCommand(trimmed, connection);
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException ex) when (ex.Number == 2714 || ex.Number == 3701)
            {
                // Procedure already exists or object doesn't exist (expected during DROP)
                // The SQL scripts use IF EXISTS DROP, so this is fine
            }
        }
    }

    private static string GetScriptContent(string fileName)
    {
        // Find Scripts folder - try multiple locations
        var assembly = typeof(CreateStoredProcedures).Assembly;
        var assemblyLocation = Path.GetDirectoryName(assembly.Location);
        
        var searchPaths = new List<string>();
        
        if (!string.IsNullOrEmpty(assemblyLocation))
        {
            searchPaths.Add(Path.Combine(assemblyLocation, "Scripts", fileName));
            searchPaths.Add(Path.Combine(assemblyLocation, "Reporting.Infrastructure", "Scripts", fileName));
        }
        
        searchPaths.Add(Path.Combine(AppContext.BaseDirectory, "Scripts", fileName));
        searchPaths.Add(Path.Combine(AppContext.BaseDirectory, "Reporting.Infrastructure", "Scripts", fileName));
        
        // Search from current directory up
        var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
        for (int i = 0; i < 5 && currentDir != null; i++)
        {
            var path = Path.Combine(currentDir.FullName, "Reporting.Infrastructure", "Scripts", fileName);
            if (!searchPaths.Contains(path))
                searchPaths.Add(path);
            currentDir = currentDir.Parent;
        }

        foreach (var path in searchPaths)
        {
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }
        }

        throw new FileNotFoundException(
            $"Stored procedure script not found: {fileName}. Searched in: {string.Join(", ", searchPaths)}");
    }
}