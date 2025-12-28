using Microsoft.Extensions.DependencyInjection;
using Reporting.Application.Interfaces;
using Reporting.Application.Services;
using Reporting.Infrastructure.Repositories;

namespace Reporting.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        //Repositories
        services.AddScoped<IReportRepository,ReportRepository>();

        //Services
        services.AddScoped<IReportService, ReportService>();

        return services;
    }
}
