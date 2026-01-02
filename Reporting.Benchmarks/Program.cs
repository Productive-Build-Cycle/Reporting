using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using Reporting.Benchmarks;

namespace Reporting.Benchmarks;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("==========================================");
        Console.WriteLine(" Reporting Service Performance Benchmarks ");
        Console.WriteLine("==========================================");
        Console.WriteLine();
        Console.WriteLine("Comparing:");
        Console.WriteLine("  • Entity Framework Core (LINQ)");
        Console.WriteLine("  • Dapper (Raw SQL)");
        Console.WriteLine("  • Stored Procedures");
        Console.WriteLine();
        Console.WriteLine("Running all benchmark suites...");
        Console.WriteLine("This may take a few minutes.\n");

        // Get the benchmark project directory (where this assembly's .csproj file is located)
        // This ensures artifacts are always created in the benchmark project folder, not the solution root
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation) ?? Directory.GetCurrentDirectory();
        
        // Navigate up from bin/Debug/netX.X or bin/Release/netX.X to find the project directory
        // Look for Reporting.Benchmarks.csproj file
        var projectDirectory = assemblyDirectory;
        var maxDepth = 5;
        for (int i = 0; i < maxDepth; i++)
        {
            var csprojPath = Path.Combine(projectDirectory, "Reporting.Benchmarks.csproj");
            if (File.Exists(csprojPath))
                break;
            
            var parent = Directory.GetParent(projectDirectory);
            if (parent == null)
                break;
            projectDirectory = parent.FullName;
        }
        
        // Set artifacts path to the benchmark project directory
        var artifactsPath = Path.Combine(projectDirectory, "BenchmarkDotNet.Artifacts");

        // Use a clean config:
        // - No .log files (reduces noisy artifacts)
        // - Keep default exporters (CSV/HTML/Markdown) for detailed analysis
        // - Set ArtifactsPath to benchmark project directory (prevents creation at solution root)
        var config = ManualConfig
            .Create(DefaultConfig.Instance)
            .WithOptions(ConfigOptions.DisableLogFile);
        
        // Set the artifacts path to the benchmark project directory
        config.ArtifactsPath = artifactsPath;

        var summaries = new List<Summary>
        {
            BenchmarkRunner.Run<TeamPerformanceQueryBenchmark>(config),
            BenchmarkRunner.Run<TasksPerUserQueryBenchmark>(config),
            BenchmarkRunner.Run<CompletedTasksPerWeekQueryBenchmark>(config)
        };

        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("ALL BENCHMARKS COMPLETED");
        Console.WriteLine(new string('=', 80));

        PrintConsolidatedSummary(summaries);
        PrintResultsLocation(summaries);

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    private static void PrintConsolidatedSummary(List<Summary> summaries)
    {
        Console.WriteLine("\nCONSOLIDATED BENCHMARK SUMMARY");
        Console.WriteLine("==============================");
        Console.WriteLine("All ratios are relative to the EF Core version of each query (baseline = 1.00)");
        Console.WriteLine("Lower time = better performance\n");

        var queryGroups = new[]
        {
            ("Team Performance Summary Query",       typeof(TeamPerformanceQueryBenchmark)),
            ("Tasks Per User Query",                typeof(TasksPerUserQueryBenchmark)),
            ("Completed Tasks Per Week Query",      typeof(CompletedTasksPerWeekQueryBenchmark))
        };

        bool anyResults = false;

        foreach (var (displayName, benchmarkType) in queryGroups)
        {
            var summary = summaries.FirstOrDefault(s => 
                s.BenchmarksCases.Any(c => c.Descriptor.Type == benchmarkType));

            if (summary == null) continue;

            var successfulReports = summary.Reports
                .Where(r => r.Success && r.ResultStatistics != null)
                .ToList();

            if (!successfulReports.Any())
            {
                Console.WriteLine($"📊 {displayName}");
                Console.WriteLine("   ⚠️ No successful results\n");
                continue;
            }

            anyResults = true;

            Console.WriteLine($"📊 {displayName}");
            Console.WriteLine(new string('-', 80));

            // Reliable baseline selection:
            // 1. Prefer method marked with [Benchmark(Baseline = true)]
            // 2. Then any method containing "EfCore" or "Linq"
            // 3. Fallback: slowest method (most conservative)
            var baselineReport = successfulReports
                .FirstOrDefault(r =>
                {
                    var benchmarkAttr = r.BenchmarkCase.Descriptor.WorkloadMethod
                        .GetCustomAttributes(typeof(BenchmarkDotNet.Attributes.BenchmarkAttribute), false)
                        .FirstOrDefault() as BenchmarkDotNet.Attributes.BenchmarkAttribute;
                    return benchmarkAttr?.Baseline == true;
                })
                ?? successfulReports
                    .FirstOrDefault(r => r.BenchmarkCase.Descriptor.WorkloadMethod.Name.Contains("EfCore") ||
                                        r.BenchmarkCase.Descriptor.WorkloadMethod.Name.Contains("Linq"))
                ?? successfulReports.OrderByDescending(r => r.ResultStatistics!.Mean).First();

            var baselineMeanNs = baselineReport.ResultStatistics!.Mean;

            // Header
            Console.WriteLine($"  {"Method",-40} {"Mean Time",12} {"Ratio",8} {"Status",12}");
            Console.WriteLine(new string('-', 80));

            foreach (var report in successfulReports.OrderBy(r => r.ResultStatistics!.Mean))
            {
                var fullName = report.BenchmarkCase.Descriptor.WorkloadMethod.Name;
                var shortName = CleanMethodName(fullName, benchmarkType.Name);

                var meanNs = report.ResultStatistics!.Mean;
                var ratio = meanNs / baselineMeanNs;
                var ratioText = ReferenceEquals(report, baselineReport) ? "1.00 (base)" : $"{ratio:F2}x";
                var status = ReferenceEquals(report, baselineReport)
                    ? "Baseline"
                    : ratio < 1.0 ? "Faster ✓" : "Slower";

                var timeText = FormatNanoseconds(meanNs);

                Console.WriteLine($"  {shortName,-40} {timeText,12} {ratioText,8} {status,12}");
            }

            Console.WriteLine();
        }

        if (anyResults)
        {
            Console.WriteLine("Legend:");
            Console.WriteLine("  • Baseline  : EF Core implementation for that query");
            Console.WriteLine("  • Ratio     : Performance vs baseline (lower = faster)");
            Console.WriteLine("  • Faster ✓  : Outperforms EF Core");
            Console.WriteLine();
        }

        PrintWarnings(summaries);
    }

    private static string CleanMethodName(string fullName, string benchmarkClassName)
    {
        return fullName
            .Replace(benchmarkClassName + ".", "")
            .Replace("EfCore", "EF Core")
            .Replace("Linq", "(LINQ)")
            .Replace("Dapper", "Dapper")
            .Replace("RawSql", "(Raw SQL)")
            .Replace("StoredProcedure", "Stored Proc")
            .Replace("_", " ")
            .Trim();
    }

    private static string FormatNanoseconds(double ns)
    {
        return ns switch
        {
            < 999.5 => $"{ns:F0} ns",
            < 999_999.5 => $"{ns / 1_000:F1} μs",
            < 999_999_999.5 => $"{ns / 1_000_000:F2} ms",
            _ => $"{ns / 1_000_000_000:F2} s"
        };
    }

    private static void PrintResultsLocation(List<Summary> summaries)
    {
        Console.WriteLine("==========================================");
        Console.WriteLine("Detailed Results");
        Console.WriteLine("==========================================");
        Console.WriteLine();
        Console.WriteLine("Full reports (interactive charts, CSV, Markdown):");
        Console.WriteLine("  📊 HTML  → BenchmarkDotNet.Artifacts/results/*.html");
        Console.WriteLine("  📈 CSV   → Import into Excel/Google Sheets");
        Console.WriteLine("  📝 MD    → For documentation/pull requests");
        Console.WriteLine();

        var path = summaries.FirstOrDefault(s => !string.IsNullOrEmpty(s.ResultsDirectoryPath))?.ResultsDirectoryPath;
        if (!string.IsNullOrEmpty(path))
        {
            Console.WriteLine($"Results folder: {path}");
        }
    }

    private static void PrintWarnings(List<Summary> summaries)
    {
        var failed = summaries
            .SelectMany(s => s.Reports.Where(r => !r.Success))
            .ToList();

        if (!failed.Any()) return;

        Console.WriteLine("⚠️ WARNINGS & FAILED BENCHMARKS");
        Console.WriteLine(new string('-', 80));

        var distinctNames = failed
            .Select(r => CleanMethodName(r.BenchmarkCase.Descriptor.WorkloadMethod.Name, ""))
            .Distinct()
            .ToList();

        Console.WriteLine($"\nFailed benchmarks ({failed.Count}):");
        foreach (var name in distinctNames.Take(10))
        {
            Console.WriteLine($"  • {name}");
        }
        if (distinctNames.Count > 10)
            Console.WriteLine($"  ... and {distinctNames.Count - 10} more");

        Console.WriteLine("\nMost likely causes & fixes:");
        Console.WriteLine("1. Missing Stored Procedures");
        Console.WriteLine("   → Run these scripts on your database:");
        Console.WriteLine("       • sp_GetTeamPerformanceSummary.sql");
        Console.WriteLine("       • sp_GetTasksPerUser.sql");
        Console.WriteLine("       • sp_GetCompletedTasksPerWeek.sql");
        Console.WriteLine();
        Console.WriteLine("2. Database Connection Issues");
        Console.WriteLine("   → Check appsettings.json connection string");
        Console.WriteLine("   → Ensure DB is running and has test data");
        Console.WriteLine();
        Console.WriteLine("3. For more stable results:");
        Console.WriteLine("   → Increase InvocationCount to 50–100 so each iteration takes ~100–500 ms");
        Console.WriteLine("   → Current iterations are too short (<20 ms) → noisy results");
    }
}