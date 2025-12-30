using Dapper;
using Microsoft.Data.SqlClient;
using Reporting.Application.DTOs;
using Reporting.Application.Interfaces;
using System.Data;

namespace Reporting.Infrastructure.Queries;

/// <summary>
/// Dapper implementation of ITeamPerformanceQuery using raw SQL queries.
/// This implementation uses Dapper for lightweight, high-performance data access.
/// 
/// Performance characteristics:
/// - Direct SQL execution with minimal overhead
/// - No LINQ expression tree compilation
/// - Manual SQL construction allows for fine-tuned queries
/// - Typically faster than EF Core for read-heavy scenarios
/// - Uses parameterized queries to prevent SQL injection
/// </summary>
public sealed class DapperTeamPerformanceQuery : ITeamPerformanceQuery
{
    private readonly string _connectionString;

    public DapperTeamPerformanceQuery(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<List<TeamPerformanceSummaryResponseDto>> ExecuteAsync(
        TeamPerformanceSummaryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Base SQL query with aggregation logic
        // Uses SQL CASE statements for conditional counting
        // WHERE 1=1 allows easy appending of optional filter conditions
        var sql = @"
            SELECT 
                t.Id AS TeamId,
                t.Name AS TeamName,
                COUNT(task.Id) AS TotalTasks,
                -- Count completed tasks: Status='Completed' OR CompletedAt IS NOT NULL
                SUM(CASE WHEN task.Status = 'Completed' OR task.CompletedAt IS NOT NULL THEN 1 ELSE 0 END) AS CompletedTasks,
                -- Calculate completion rate as percentage (avoid division by zero)
                CASE 
                    WHEN COUNT(task.Id) > 0 
                    THEN CAST(SUM(CASE WHEN task.Status = 'Completed' OR task.CompletedAt IS NOT NULL THEN 1 ELSE 0 END) * 100.0 / COUNT(task.Id) AS DECIMAL(18,2))
                    ELSE 0 
                END AS CompletionRate
            FROM Teams t
            INNER JOIN Tasks task ON t.Id = task.TeamId
            WHERE 1 = 1";

        // Build dynamic parameters for optional filters
        var parameters = new DynamicParameters();

        // Add optional date range filters
        if (request.StartDate.HasValue)
        {
            sql += " AND task.CreatedAt >= @StartDate";
            parameters.Add("@StartDate", request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            sql += " AND task.CreatedAt <= @EndDate";
            parameters.Add("@EndDate", request.EndDate.Value);
        }

        // Add optional team filter
        if (request.TeamId.HasValue)
        {
            sql += " AND t.Id = @TeamId";
            parameters.Add("@TeamId", request.TeamId.Value);
        }

        // Group by team and order results
        sql += @"
            GROUP BY t.Id, t.Name
            ORDER BY t.Id";

        // Execute query using Dapper with CommandDefinition for cancellation token support
        await using var connection = new SqlConnection(_connectionString);
        var command = new CommandDefinition(
            sql,
            parameters,
            cancellationToken: cancellationToken);
        
        var result = await connection.QueryAsync<TeamPerformanceSummaryResponseDto>(command);

        return result.ToList();
    }
}

