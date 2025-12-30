using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reporting.Application.Interfaces;
using Reporting.Infrastructure.Db;
using Reporting.Infrastructure.Queries;

namespace Reporting.Benchmarks;

/// <summary>
/// Benchmark class for comparing different implementations of ITasksPerUserQuery.
/// Measures execution time and memory allocation for EF Core, Dapper, and Stored Procedure approaches.
/// 
/// This benchmark helps determine which data access method performs best for:
/// - Grouping tasks by user
/// - Filtering by status and date range
/// - Counting tasks per user
/// </summary>
[MemoryDiagnoser]
[WarmupCount(2)]
[IterationCount(5)]
[InvocationCount(10)]
public class TasksPerUserQueryBenchmark
{
    // Use concrete type so we can dispose the service provider in cleanup
    private ServiceProvider _serviceProvider = null!;
    private IServiceScope _scope = null!;
    private ITasksPerUserQuery _efQuery = null!;
    private ITasksPerUserQuery _dapperQuery = null!;
    private ITasksPerUserQuery _storedProcedureQuery = null!;

    /// <summary>
    /// Setup method called once before all benchmark iterations.
    /// Initializes database context, query implementations, and test data.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        var configPath = FindConfigFile();
        
        // Build configuration from appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.GetDirectoryName(configPath)!)
            .AddJsonFile(Path.GetFileName(configPath), optional: false, reloadOnChange: false)
            .Build();

        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' not found.");

        // Setup DI container for benchmark isolation
        var services = new ServiceCollection();

        // Register DbContext (required for EF Core implementation)
        services.AddDbContext<ReportingDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Register query implementations
        services.AddScoped<EfTasksPerUserQuery>();
        services.AddScoped<DapperTasksPerUserQuery>(sp =>
            new DapperTasksPerUserQuery(connectionString));
        services.AddScoped<StoredProcedureTasksPerUserQuery>(sp =>
            new StoredProcedureTasksPerUserQuery(connectionString));

        _serviceProvider = services.BuildServiceProvider();

        // Resolve implementations - keep scope alive until cleanup
        _scope = _serviceProvider.CreateScope();
        var dbContext = _scope.ServiceProvider.GetRequiredService<ReportingDbContext>();
        _efQuery = new EfTasksPerUserQuery(dbContext);
        _dapperQuery = new DapperTasksPerUserQuery(connectionString);
        _storedProcedureQuery = new StoredProcedureTasksPerUserQuery(connectionString);
    }

    /// <summary>
    /// Cleanup method called once after all benchmark iterations.
    /// Disposes resources to prevent memory leaks.
    /// </summary>
    [GlobalCleanup]
    public void Cleanup()
    {
        _scope?.Dispose();
        _serviceProvider?.Dispose();
    }

    /// <summary>
    /// Finds appsettings.json file in various possible locations.
    /// </summary>
    private static string FindConfigFile()
    {
        var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation) ?? Directory.GetCurrentDirectory();
        
        // Try current directory first
        var configPath = Path.Combine(assemblyDirectory, "appsettings.json");
        if (File.Exists(configPath))
            return configPath;
        
        // Try parent directories (for BenchmarkDotNet's generated projects)
        var currentDir = new DirectoryInfo(assemblyDirectory);
        for (int i = 0; i < 5 && currentDir != null; i++)
        {
            configPath = Path.Combine(currentDir.FullName, "appsettings.json");
            if (File.Exists(configPath))
                return configPath;
            currentDir = currentDir.Parent;
        }
        
        throw new FileNotFoundException(
            $"appsettings.json not found. Searched starting from: {assemblyDirectory}");
    }

    /// <summary>
    /// Baseline benchmark: EF Core LINQ implementation.
    /// This is marked as baseline so other implementations are compared relative to it.
    /// </summary>
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Query")]
    public object EfCore_Linq_NoFilters()
    {
        // Query all tasks per user without filters
        return _efQuery.ExecuteAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Benchmark: Dapper raw SQL implementation without filters.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Query")]
    public object Dapper_RawSql_NoFilters()
    {
        return _dapperQuery.ExecuteAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Benchmark: Stored Procedure implementation without filters.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Query")]
    public object StoredProcedure_NoFilters()
    {
        return _storedProcedureQuery.ExecuteAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Benchmark: EF Core with status filter (only completed tasks).
    /// Tests performance when filtering by status.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Filtered")]
    public object EfCore_WithStatusFilter()
    {
        return _efQuery.ExecuteAsync(status: "Completed").GetAwaiter().GetResult();
    }

    /// <summary>
    /// Benchmark: Dapper with status filter.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Filtered")]
    public object Dapper_WithStatusFilter()
    {
        return _dapperQuery.ExecuteAsync(status: "Completed").GetAwaiter().GetResult();
    }

    /// <summary>
    /// Benchmark: Stored Procedure with status filter.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Filtered")]
    public object StoredProcedure_WithStatusFilter()
    {
        return _storedProcedureQuery.ExecuteAsync(status: "Completed").GetAwaiter().GetResult();
    }

    /// <summary>
    /// Benchmark: EF Core with date range filter (last 6 months).
    /// Tests performance when filtering by date range.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Filtered")]
    public object EfCore_WithDateRange()
    {
        var from = DateTime.UtcNow.AddMonths(-6);
        var to = DateTime.UtcNow;
        return _efQuery.ExecuteAsync(from: from, to: to).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Benchmark: Dapper with date range filter.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Filtered")]
    public object Dapper_WithDateRange()
    {
        var from = DateTime.UtcNow.AddMonths(-6);
        var to = DateTime.UtcNow;
        return _dapperQuery.ExecuteAsync(from: from, to: to).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Benchmark: Stored Procedure with date range filter.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Filtered")]
    public object StoredProcedure_WithDateRange()
    {
        var from = DateTime.UtcNow.AddMonths(-6);
        var to = DateTime.UtcNow;
        return _storedProcedureQuery.ExecuteAsync(from: from, to: to).GetAwaiter().GetResult();
    }
}

