using Dapper;
using Microsoft.Data.SqlClient;
using Reporting.Application.DTOs;
using Reporting.Application.Interfaces;
using System.Data;

namespace Reporting.Infrastructure.Queries;

/// <summary>
/// Stored Procedure implementation of ITeamPerformanceQuery.
/// This implementation uses a SQL Server stored procedure for maximum performance and query plan caching.
/// </summary>
public sealed class StoredProcedureTeamPerformanceQuery : ITeamPerformanceQuery
{
    private readonly string _connectionString;

    public StoredProcedureTeamPerformanceQuery(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<List<TeamPerformanceSummaryResponseDto>> ExecuteAsync(
        TeamPerformanceSummaryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@StartDate", request.StartDate, dbType: System.Data.DbType.DateTime2);
        parameters.Add("@EndDate", request.EndDate, dbType: System.Data.DbType.DateTime2);
        parameters.Add("@TeamId", request.TeamId, dbType: System.Data.DbType.Int32);

        await using var connection = new SqlConnection(_connectionString);
        var command = new CommandDefinition(
            "sp_GetTeamPerformanceSummary",
            parameters,
            commandType: System.Data.CommandType.StoredProcedure,
            cancellationToken: cancellationToken);
        
        var result = await connection.QueryAsync<TeamPerformanceSummaryResponseDto>(command);

        return result.ToList();
    }
}

