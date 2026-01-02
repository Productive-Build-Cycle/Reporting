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
/// Benchmark class for comparing different implementations of ITeamPerformanceQuery.
/// Measures execution time and memory allocation for EF Core, Dapper, and Stored Procedure approaches.
/// </summary>
[MemoryDiagnoser]
[WarmupCount(2)]
[IterationCount(5)]
[InvocationCount(10)]
public class TeamPerformanceQueryBenchmark
{
    // Use concrete type so we can dispose the service provider in cleanup
    private ServiceProvider _serviceProvider = null!;
    private IServiceScope _scope = null!;
    private ITeamPerformanceQuery _efQuery = null!;
    private ITeamPerformanceQuery _dapperQuery = null!;
    private ITeamPerformanceQuery _storedProcedureQuery = null!;
    private TeamPerformanceSummaryRequestDto _testRequest = null!;

    [GlobalSetup]
    public void Setup()
    {
        var configPath = FindConfigFile();
        
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.GetDirectoryName(configPath)!)
            .AddJsonFile(Path.GetFileName(configPath), optional: false, reloadOnChange: false)
            .Build();

        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' not found.");

        // Setup DI container
        var services = new ServiceCollection();

        // Register DbContext
        services.AddDbContext<ReportingDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Register query implementations
        services.AddScoped<EfTeamPerformanceQuery>();
        services.AddScoped<DapperTeamPerformanceQuery>(sp =>
            new DapperTeamPerformanceQuery(connectionString));
        services.AddScoped<StoredProcedureTeamPerformanceQuery>(sp =>
            new StoredProcedureTeamPerformanceQuery(connectionString));

        _serviceProvider = services.BuildServiceProvider();

        // Resolve implementations - keep scope alive until cleanup
        _scope = _serviceProvider.CreateScope();
        var dbContext = _scope.ServiceProvider.GetRequiredService<ReportingDbContext>();
        _efQuery = new EfTeamPerformanceQuery(dbContext);
        _dapperQuery = new DapperTeamPerformanceQuery(connectionString);
        _storedProcedureQuery = new StoredProcedureTeamPerformanceQuery(connectionString);

        // Create test request with typical parameters
        _testRequest = new TeamPerformanceSummaryRequestDto
        {
            StartDate = DateTime.UtcNow.AddMonths(-6),
            EndDate = DateTime.UtcNow,
            TeamId = null // Test with all teams
        };
    }

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
    /// Uses TeamPerformanceSummaryRequestDto matching the API contract.
    /// This is marked as baseline so other implementations are compared relative to it.
    /// </summary>
    [Benchmark(Baseline = true)]
    public List<TeamPerformanceSummaryResponseDto> EfCore_Linq()
    {
        return _efQuery.ExecuteAsync(_testRequest).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Benchmark: Dapper raw SQL implementation.
    /// Uses TeamPerformanceSummaryRequestDto matching the API contract.
    /// </summary>
    [Benchmark]
    public List<TeamPerformanceSummaryResponseDto> Dapper_RawSql()
    {
        return _dapperQuery.ExecuteAsync(_testRequest).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Benchmark: Stored Procedure implementation.
    /// Uses TeamPerformanceSummaryRequestDto matching the API contract.
    /// </summary>
    [Benchmark]
    public List<TeamPerformanceSummaryResponseDto> StoredProcedure()
    {
        return _storedProcedureQuery.ExecuteAsync(_testRequest).GetAwaiter().GetResult();
    }
}

