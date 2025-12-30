using Dapper;
using Microsoft.Data.SqlClient;
using Reporting.Application.DTOs;
using Reporting.Application.Interfaces;
using System.Data;

namespace Reporting.Infrastructure.Queries;

/// <summary>
/// Stored Procedure implementation of ICompletedTasksPerWeekQuery.
/// This implementation uses a SQL Server stored procedure for maximum performance and query plan caching.
/// 
/// Performance characteristics:
/// - Pre-compiled SQL with cached execution plans
/// - Database optimizer creates optimal plan once and reuses it
/// - No SQL parsing/compilation overhead at runtime
/// - Typically fastest for frequently executed aggregation queries
/// </summary>
public sealed class StoredProcedureCompletedTasksPerWeekQuery : ICompletedTasksPerWeekQuery
{
    private readonly string _connectionString;

    public StoredProcedureCompletedTasksPerWeekQuery(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<List<CompletedTasksPerWeekReportDto>> ExecuteAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        // Prepare parameters for stored procedure
        var parameters = new DynamicParameters();
        parameters.Add("@StartDate", startDate, dbType: DbType.DateTime2);
        parameters.Add("@EndDate", endDate, dbType: DbType.DateTime2);

        // Execute stored procedure using Dapper
        await using var connection = new SqlConnection(_connectionString);
        var command = new CommandDefinition(
            "sp_GetCompletedTasksPerWeek",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var result = await connection.QueryAsync<CompletedTasksPerWeekReportDto>(command);

        return result.ToList();
    }
}

