using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reporting.Application.Interfaces;
using Reporting.Application.Services;
using Reporting.Infrastructure.Queries;
using Reporting.Infrastructure.Repositories;

namespace Reporting.Infrastructure;

/// <summary>
/// Dependency Injection configuration for the Infrastructure layer.
/// This class registers all services, repositories, and query implementations.
/// 
/// Architecture notes:
/// - Query interfaces are registered with EF Core as default (can be changed for production)
/// - All implementations are registered for benchmarking/testing purposes
/// - Connection string is injected from configuration
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds all application services, repositories, and query implementations to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    /// <param name="configuration">Configuration instance to read connection strings</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default") 
            ?? throw new InvalidOperationException("Connection string 'Default' not found.");

        // ========== Repositories ==========
        // Legacy repository pattern - kept for backward compatibility
        services.AddScoped<IReportRepository, ReportRepository>();

        // ========== Query Implementations ==========
        // These follow Clean Architecture: Application layer defines interfaces,
        // Infrastructure layer provides implementations (EF Core, Dapper, Stored Procedures)
        
        // Tasks Per User Query - EF Core as default
        services.AddScoped<ITasksPerUserQuery, EfTasksPerUserQuery>();
        // Register all implementations for benchmarking/testing
        services.AddScoped<EfTasksPerUserQuery>();
        services.AddScoped<DapperTasksPerUserQuery>(sp => 
            new DapperTasksPerUserQuery(connectionString));
        services.AddScoped<StoredProcedureTasksPerUserQuery>(sp => 
            new StoredProcedureTasksPerUserQuery(connectionString));

        // Completed Tasks Per Week Query - EF Core as default
        services.AddScoped<ICompletedTasksPerWeekQuery, EfCompletedTasksPerWeekQuery>();
        // Register all implementations for benchmarking/testing
        services.AddScoped<EfCompletedTasksPerWeekQuery>();
        services.AddScoped<DapperCompletedTasksPerWeekQuery>(sp => 
            new DapperCompletedTasksPerWeekQuery(connectionString));
        services.AddScoped<StoredProcedureCompletedTasksPerWeekQuery>(sp => 
            new StoredProcedureCompletedTasksPerWeekQuery(connectionString));

        // Team Performance Query - EF Core as default
        services.AddScoped<ITeamPerformanceQuery, EfTeamPerformanceQuery>();
        // Register all implementations for benchmarking/testing
        services.AddScoped<EfTeamPerformanceQuery>();
        services.AddScoped<DapperTeamPerformanceQuery>(sp => 
            new DapperTeamPerformanceQuery(connectionString));
        services.AddScoped<StoredProcedureTeamPerformanceQuery>(sp => 
            new StoredProcedureTeamPerformanceQuery(connectionString));

        // ========== Application Services ==========
        // Services depend on query interfaces, not implementations (Dependency Inversion Principle)
        services.AddScoped<IReportService, ReportService>();

        return services;
    }
}
