using Dapper;
using Microsoft.Data.SqlClient;
using Reporting.Application.DTOs;
using Reporting.Application.Interfaces;
using System.Data;

namespace Reporting.Infrastructure.Queries;

/// <summary>
/// Stored Procedure implementation of ITasksPerUserQuery.
/// This implementation uses a SQL Server stored procedure for maximum performance and query plan caching.
/// 
/// Performance characteristics:
/// - Pre-compiled SQL with cached execution plans
/// - No SQL parsing/compilation overhead at runtime
/// - Database optimizer can create optimal plan once and reuse it
/// - Typically fastest for frequently executed queries with stable parameters
/// </summary>
public sealed class StoredProcedureTasksPerUserQuery : ITasksPerUserQuery
{
    private readonly string _connectionString;

    public StoredProcedureTasksPerUserQuery(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<List<TasksPerUserReportDto>> ExecuteAsync(
        string? status = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        // Prepare parameters for stored procedure
        var parameters = new DynamicParameters();
        parameters.Add("@Status", status, dbType: DbType.String);
        parameters.Add("@From", from, dbType: DbType.DateTime2);
        parameters.Add("@To", to, dbType: DbType.DateTime2);

        // Execute stored procedure using Dapper
        await using var connection = new SqlConnection(_connectionString);
        var command = new CommandDefinition(
            "sp_GetTasksPerUser",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var result = await connection.QueryAsync<TasksPerUserReportDto>(command);

        return result.ToList();
    }
}

