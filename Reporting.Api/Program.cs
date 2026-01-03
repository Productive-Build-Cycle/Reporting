using Microsoft.EntityFrameworkCore;
using Reporting.Infrastructure;
using Reporting.Infrastructure.Db;
using Reporting.Application.Interfaces;
using Reporting.Application.Services;
using Reporting.Infrastructure.Repositories;
using OfficeOpenXml;

var builder = WebApplication.CreateBuilder(args);

ExcelPackage.License.SetNonCommercialPersonal("Reporting");

// Controllers
builder.Services.AddControllers();

// Application Services
builder.Services.AddApplicationServices();

// DbContext
builder.Services.AddDbContext<ReportingDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});

// Application Services
builder.Services.AddApplicationServices();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ReportingDbContext>();
    db.Database.Migrate();
    SeedData.Initialize(db);
   // Create stored procedures (required for benchmarking)
    try
    {
        Reporting.Infrastructure.Queries.CreateStoredProcedures.CreateAll(db);
    }
    catch (Exception ex)
    {
        // Log but don't fail startup if stored procedures can't be created
        // They can be created manually if needed
        Console.WriteLine($"Warning: Could not create stored procedures automatically: {ex.Message}");
        Console.WriteLine("You can create them manually by running the SQL scripts in Reporting.Infrastructure/Queries/");
    }


}

app.Run();
