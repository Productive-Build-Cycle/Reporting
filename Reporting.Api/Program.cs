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
    
    // Seed initial data (teams, users, projects, tasks)
    SeedData.Initialize(db);
    
    // Optional: Seed heavy data for performance benchmarking
    // Uncomment the line below to add 50,000+ tasks for realistic performance testing
    // Note: This will take several minutes to complete
    // Reporting.Infrastructure.Scripts.SeedHeavyData.Seed(db, targetTaskCount: 50_000);
}

app.Run();
