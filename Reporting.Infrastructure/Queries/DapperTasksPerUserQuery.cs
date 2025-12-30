using Dapper;
using Microsoft.Data.SqlClient;
using Reporting.Application.DTOs;
using Reporting.Application.Interfaces;
using System.Data;

namespace Reporting.Infrastructure.Queries;

/// <summary>
/// Dapper implementation of ITasksPerUserQuery using raw SQL queries.
/// This implementation uses Dapper for lightweight, high-performance data access.
/// 
/// Performance characteristics:
/// - Direct SQL execution with minimal overhead
/// - No LINQ expression tree compilation
/// - Manual SQL construction allows for fine-tuned queries
/// - Typically faster than EF Core for read-heavy scenarios
/// </summary>
public sealed class DapperTasksPerUserQuery : ITasksPerUserQuery
{
    private readonly string _connectionString;

    public DapperTasksPerUserQuery(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<List<TasksPerUserReportDto>> ExecuteAsync(
        string? status = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        // Build SQL query dynamically based on provided filters
        var sql = @"
            SELECT 
                u.Id AS UserId,
                u.Name AS UserName,
                COUNT(t.Id) AS TasksCount
            FROM Tasks t
            INNER JOIN Users u ON t.UserId = u.Id
            WHERE 1 = 1"; // WHERE 1=1 allows easy appending of conditions

        var parameters = new DynamicParameters();

        // Add optional filters
        if (!string.IsNullOrWhiteSpace(status))
        {
            sql += " AND t.Status = @Status";
            parameters.Add("@Status", status);
        }

        if (from.HasValue)
        {
            sql += " AND t.CreatedAt >= @From";
            parameters.Add("@From", from.Value);
        }

        if (to.HasValue)
        {
            sql += " AND t.CreatedAt <= @To";
            parameters.Add("@To", to.Value);
        }

        // Group by user and order by task count descending
        sql += @"
            GROUP BY u.Id, u.Name
            ORDER BY TasksCount DESC";

        // Execute query using Dapper's QueryAsync with CommandDefinition for cancellation token support
        await using var connection = new SqlConnection(_connectionString);
        var command = new CommandDefinition(
            sql,
            parameters,
            cancellationToken: cancellationToken);

        var result = await connection.QueryAsync<TasksPerUserReportDto>(command);

        return result.ToList();
    }
}

