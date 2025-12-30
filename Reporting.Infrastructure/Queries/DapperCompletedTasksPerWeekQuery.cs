using Dapper;
using Microsoft.Data.SqlClient;
using Reporting.Application.DTOs;
using Reporting.Application.Interfaces;
using System.Data;

namespace Reporting.Infrastructure.Queries;

/// <summary>
/// Dapper implementation of ICompletedTasksPerWeekQuery using raw SQL queries.
/// This implementation uses Dapper for lightweight, high-performance data access.
/// 
/// Performance characteristics:
/// - Direct SQL execution with SQL Server date functions (DATEPART)
/// - No LINQ overhead or expression tree compilation
/// - Manual SQL allows for optimized date calculations
/// - Typically faster than EF Core for aggregation queries
/// </summary>
public sealed class DapperCompletedTasksPerWeekQuery : ICompletedTasksPerWeekQuery
{
    private readonly string _connectionString;

    public DapperCompletedTasksPerWeekQuery(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<List<CompletedTasksPerWeekReportDto>> ExecuteAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        // SQL query using SQL Server DATEPART function for week calculation
        var sql = @"
            SELECT
                DATEPART(YEAR, CompletedAt) AS [Year],
                DATEPART(WEEK, CompletedAt) AS [WeekNumber],
                COUNT(*) AS [CompletedTasksCount]
            FROM Tasks
            WHERE
                CompletedAt IS NOT NULL
                AND CompletedAt >= @StartDate
                AND CompletedAt <= @EndDate
            GROUP BY
                DATEPART(YEAR, CompletedAt),
                DATEPART(WEEK, CompletedAt)
            ORDER BY
                [Year],
                [WeekNumber]";

        var parameters = new DynamicParameters();
        parameters.Add("@StartDate", startDate);
        parameters.Add("@EndDate", endDate);

        // Execute query using Dapper
        await using var connection = new SqlConnection(_connectionString);
        var command = new CommandDefinition(
            sql,
            parameters,
            cancellationToken: cancellationToken);

        var result = await connection.QueryAsync<CompletedTasksPerWeekReportDto>(command);

        return result.ToList();
    }
}

