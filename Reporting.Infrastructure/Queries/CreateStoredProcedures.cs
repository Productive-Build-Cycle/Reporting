using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Reporting.Infrastructure.Queries;

/// <summary>
/// Helper class to create stored procedures in the database.
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

        var scripts = new[] { "GetCompletedTasksPerWeek.sql" };

        foreach (var fileName in scripts)
        {
            var scriptContent = GetScriptContent(fileName);
            if (string.IsNullOrWhiteSpace(scriptContent))
                continue;

            try
            {
                using var command = new SqlCommand(scriptContent, connection);
                command.ExecuteNonQuery();
            }
            catch (SqlException ex) when (ex.Number == 2714 || ex.Number == 3701)
            {
                // Procedure already exists or object doesn't exist - this is fine
            }
        }
    }

    private static string GetScriptContent(string fileName)
    {
        // Get the Queries folder path relative to the assembly
        var assembly = typeof(CreateStoredProcedures).Assembly;
        var assemblyLocation = Path.GetDirectoryName(assembly.Location);
        
        if (string.IsNullOrEmpty(assemblyLocation))
            throw new InvalidOperationException("Unable to determine assembly location.");

        // Try: bin/Debug/net10.0/Queries/fileName.sql
        var scriptPath = Path.Combine(assemblyLocation, "Queries", fileName);
        
        if (!File.Exists(scriptPath))
        {
            // Fallback: try project root Queries folder
            var projectRoot = Directory.GetCurrentDirectory();
            scriptPath = Path.Combine(projectRoot, "Reporting.Infrastructure", "Queries", fileName);
        }

        if (!File.Exists(scriptPath))
            throw new FileNotFoundException($"SQL script not found: {fileName}. Looked in: {scriptPath}");

        return File.ReadAllText(scriptPath);
    }
}