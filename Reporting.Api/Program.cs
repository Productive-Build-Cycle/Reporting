using Microsoft.EntityFrameworkCore;
using Reporting.Infrastructure;
using Reporting.Infrastructure.Db;
using Reporting.Application.Interfaces;
using Reporting.Application.Services;
using Reporting.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Application Services
builder.Services.AddApplicationServices(builder.Configuration);

// DbContext
builder.Services.AddDbContext<ReportingDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});

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

// Database initialization and seeding
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ReportingDbContext>();
    
    // Apply pending migrations
    db.Database.Migrate();
    
    // Create stored procedures (required for benchmarking)
    try
    {
        Reporting.Infrastructure.Scripts.CreateStoredProcedures.CreateAll(db);
    }
    catch (Exception ex)
    {
        // Log but don't fail startup if stored procedures can't be created
        // They can be created manually if needed
        Console.WriteLine($"Warning: Could not create stored procedures automatically: {ex.Message}");
        Console.WriteLine("You can create them manually by running the SQL scripts in Reporting.Infrastructure/Scripts/");
    }
    
    // Seed initial data (teams, users, projects, tasks)
    SeedData.Initialize(db);
    
    // Seed heavy data for performance benchmarking
    // This creates 50,000+ tasks for realistic performance testing
    // Note: This will take several minutes to complete on first run
    try
    {
        Reporting.Infrastructure.Scripts.SeedHeavyData.Seed(db, targetTaskCount: 50_000);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Warning: Could not seed heavy data: {ex.Message}");
        Console.WriteLine("Basic seed data is still available. Heavy data can be seeded manually if needed.");
    }
}

app.Run();
