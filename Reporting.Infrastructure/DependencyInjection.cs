using Microsoft.Extensions.DependencyInjection;
using Reporting.Application.Interfaces;
using Reporting.Application.Services;
using Reporting.Infrastructure.Repositories;

namespace Reporting.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IRepository, ReportRepository>();
        services.AddScoped<IReportService, ReportService>();
        
        services.AddScoped<IPerformanceRepository, PerformanceReportRepository>();
        services.AddScoped<IPerformanceReportService, PerformanceReportService>();

        return services;
    }
}
