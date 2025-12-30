using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reporting.Application.DTOs;
using Reporting.Application.Interfaces;
using Reporting.Infrastructure.Db;
using Reporting.Infrastructure.Queries;

namespace Reporting.Benchmarks;

/// <summary>
/// Benchmark class for comparing different implementations of ICompletedTasksPerWeekQuery.
/// Measures execution time and memory allocation for EF Core, Dapper, and Stored Procedure approaches.
/// 
/// This benchmark helps determine which data access method performs best for:
/// - Weekly aggregation of completed tasks
/// - Date range filtering
/// - Grouping by year and week number
/// </summary>
[MemoryDiagnoser]
[WarmupCount(2)]
[IterationCount(5)]
[InvocationCount(10)]
public class CompletedTasksPerWeekQueryBenchmark
{
    // Use concrete type so we can dispose the service provider in cleanup
    private ServiceProvider _serviceProvider = null!;
    private IServiceScope _scope = null!;
    private ICompletedTasksPerWeekQuery _efQuery = null!;
    private ICompletedTasksPerWeekQuery _dapperQuery = null!;
    private ICompletedTasksPerWeekQuery _storedProcedureQuery = null!;
    private DateTime _startDate;
    private DateTime _endDate;

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
        services.AddScoped<EfCompletedTasksPerWeekQuery>();
        services.AddScoped<DapperCompletedTasksPerWeekQuery>(sp =>
            new DapperCompletedTasksPerWeekQuery(connectionString));
        services.AddScoped<StoredProcedureCompletedTasksPerWeekQuery>(sp =>
            new StoredProcedureCompletedTasksPerWeekQuery(connectionString));

        _serviceProvider = services.BuildServiceProvider();

        // Resolve implementations - keep scope alive until cleanup
        _scope = _serviceProvider.CreateScope();
        var dbContext = _scope.ServiceProvider.GetRequiredService<ReportingDbContext>();
        _efQuery = new EfCompletedTasksPerWeekQuery(dbContext);
        _dapperQuery = new DapperCompletedTasksPerWeekQuery(connectionString);
        _storedProcedureQuery = new StoredProcedureCompletedTasksPerWeekQuery(connectionString);

        // Set up test date range (last 6 months)
        _startDate = DateTime.UtcNow.AddMonths(-6);
        _endDate = DateTime.UtcNow;
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
    public List<CompletedTasksPerWeekReportDto> EfCore_Linq()
    {
        return _efQuery.ExecuteAsync(_startDate, _endDate).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Benchmark: Dapper raw SQL implementation.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Query")]
    public List<CompletedTasksPerWeekReportDto> Dapper_RawSql()
    {
        return _dapperQuery.ExecuteAsync(_startDate, _endDate).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Benchmark: Stored Procedure implementation.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Query")]
    public List<CompletedTasksPerWeekReportDto> StoredProcedure()
    {
        return _storedProcedureQuery.ExecuteAsync(_startDate, _endDate).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Benchmark: EF Core with shorter date range (last month).
    /// Tests performance when filtering by a shorter date range.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Filtered")]
    public List<CompletedTasksPerWeekReportDto> EfCore_WithShorterDateRange()
    {
        var from = DateTime.UtcNow.AddMonths(-1);
        var to = DateTime.UtcNow;
        return _efQuery.ExecuteAsync(from, to).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Benchmark: Dapper with shorter date range.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Filtered")]
    public List<CompletedTasksPerWeekReportDto> Dapper_WithShorterDateRange()
    {
        var from = DateTime.UtcNow.AddMonths(-1);
        var to = DateTime.UtcNow;
        return _dapperQuery.ExecuteAsync(from, to).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Benchmark: Stored Procedure with shorter date range.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Filtered")]
    public List<CompletedTasksPerWeekReportDto> StoredProcedure_WithShorterDateRange()
    {
        var from = DateTime.UtcNow.AddMonths(-1);
        var to = DateTime.UtcNow;
        return _storedProcedureQuery.ExecuteAsync(from, to).GetAwaiter().GetResult();
    }
}
