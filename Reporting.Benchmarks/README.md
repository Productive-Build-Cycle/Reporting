# Reporting Service Performance Benchmarks

This project contains performance benchmarks comparing different data access implementations for the Reporting Service queries. The benchmarks help determine which data access method (EF Core, Dapper, or Stored Procedures) performs best for each query type.

## Benchmarked Queries

The project benchmarks three query types that match the API endpoints:

1. **Team Performance Summary Query** - Performance metrics for teams with optional date range and team filters
2. **Tasks Per User Query** - Task counts per user with optional status and date range filters
3. **Completed Tasks Per Week Query** - Weekly aggregation of completed tasks with date range filters

## Implementations Compared

For each query, three implementations are benchmarked:

1. **EF Core LINQ** - Entity Framework Core with LINQ-to-SQL (baseline)
2. **Dapper Raw SQL** - Dapper with raw SQL queries
3. **Stored Procedure** - SQL Server stored procedures

## Prerequisites

Before running the benchmarks, ensure the following:

1. **Database Setup**
   - SQL Server database is set up and accessible
   - Database schema is created (run migrations)
   - Database is seeded with test data

2. **Stored Procedures**
   - Execute the following stored procedure scripts in your database:
     - `Reporting.Infrastructure/Scripts/sp_GetTeamPerformanceSummary.sql`
     - `Reporting.Infrastructure/Scripts/sp_GetTasksPerUser.sql`
     - `Reporting.Infrastructure/Scripts/sp_GetCompletedTasksPerWeek.sql`

3. **Configuration**
   - Update `appsettings.json` with your database connection string:
     ```json
     {
       "ConnectionStrings": {
         "Default": "Server=.;Database=ReportingDb;Trusted_Connection=True;TrustServerCertificate=True"
       }
     }
     ```

## Building the Project

### From Command Line

Navigate to the solution root directory and build the benchmark project:

```bash
# From solution root
dotnet build Reporting.Benchmarks/Reporting.Benchmarks.csproj -c Release
```

Or navigate to the benchmark project directory:

```bash
cd Reporting.Benchmarks
dotnet build -c Release
```

### From Visual Studio

1. Open the solution in Visual Studio
2. Right-click on `Reporting.Benchmarks` project
3. Select **Build** (or press `Ctrl+Shift+B`)
4. Ensure you're building in **Release** configuration (benchmarks require Release builds for accurate results)

## Running Benchmarks

### From Command Line

**Option 1: From Solution Root**

```bash
dotnet run --project Reporting.Benchmarks/Reporting.Benchmarks.csproj -c Release
```

**Option 2: From Project Directory**

```bash
cd Reporting.Benchmarks
dotnet run -c Release
```

**Note:** Always run benchmarks in **Release** configuration for accurate performance measurements. Debug builds include debug symbols and optimizations are disabled, which will produce incorrect results.

### From Visual Studio

1. Set `Reporting.Benchmarks` as the startup project:
   - Right-click on `Reporting.Benchmarks` project
   - Select **Set as Startup Project**

2. Change build configuration to **Release**:
   - Use the dropdown in the toolbar (usually shows "Debug" by default)
   - Select "Release"

3. Run the project:
   - Press `F5` (Start Debugging) or `Ctrl+F5` (Start Without Debugging)
   - Or use **Debug** → **Start Debugging** / **Start Without Debugging**

4. Wait for benchmarks to complete (this may take a few minutes)

## Benchmark Configuration

- **Warmup Count**: 2 iterations (JIT compilation and warmup)
- **Iteration Count**: 5 iterations per benchmark
- **Invocation Count**: 10 invocations per iteration
- **Memory Diagnostics**: Enabled (measures memory allocation)
- **Artifacts Path**: Results are saved to `BenchmarkDotNet.Artifacts/` in the benchmark project directory

## Understanding Results

The benchmark output includes:

### Console Output

- **Consolidated Summary**: Comparison of all three implementations for each query
- **Mean Time**: Average execution time per operation
- **Ratio**: Performance relative to EF Core baseline (1.00 = baseline)
- **Status**: Indicates if implementation is faster or slower than baseline

### Detailed Reports

After benchmarks complete, detailed reports are generated in:
```
Reporting.Benchmarks/BenchmarkDotNet.Artifacts/results/
```

Available formats:
- **HTML** (`.html`) - Interactive charts and detailed analysis (recommended)
- **CSV** (`.csv`) - Import into Excel/Google Sheets for further analysis
- **Markdown** (`.md`) - For documentation or pull requests

### Metrics Explained

- **Mean**: Average execution time across all iterations
- **Error**: Standard error of the mean
- **StdDev**: Standard deviation (variability in results)
- **Gen0/Gen1/Gen2**: Garbage collection generation counts
- **Allocated**: Memory allocated per operation (in bytes)
- **Ratio**: Performance compared to EF Core baseline (lower is better)

## Output Location

All benchmark artifacts are saved to:
```
Reporting.Benchmarks/BenchmarkDotNet.Artifacts/
```

This includes:
- `results/` - Detailed benchmark reports (HTML, CSV, Markdown)
- Compiled benchmark assemblies and temporary files

**Note:** The artifacts folder is automatically created in the benchmark project directory, not at the solution root.

## Notes

- ✅ **Always run in Release mode** - Debug builds produce inaccurate results
- ✅ **Ensure sufficient test data** - Database should have meaningful data for realistic benchmarks
- ✅ **Run on consistent hardware** - Results vary based on CPU, memory, and SQL Server configuration
- ✅ **Stored procedures must exist** - All three stored procedures must be created before running
- ✅ **Allow time for completion** - Benchmarks may take several minutes to complete
- ⚠️ **Results may vary** - Performance depends on database size, hardware, SQL Server configuration, and system load

## Troubleshooting

### "Connection string 'Default' not found"
- Ensure `appsettings.json` exists in the `Reporting.Benchmarks` directory
- Verify the connection string is correctly configured

### "Stored procedure not found" errors
- Execute all three stored procedure scripts in your database
- Verify stored procedures exist: `sp_GetTeamPerformanceSummary`, `sp_GetTasksPerUser`, `sp_GetCompletedTasksPerWeek`

### Benchmarks run but produce no results
- Check that the database has data (run seed scripts if needed)
- Verify database connection is working
- Check console output for error messages
