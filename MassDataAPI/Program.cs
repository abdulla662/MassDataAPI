using Microsoft.EntityFrameworkCore;
using MassDataAPI.Data;
using MassDataAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title       = "MassDataAPI",
        Version     = "v1",
        Description = "Query 1M+ records with filtering, sorting and pagination — responses in milliseconds.",
    });
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});

var dbPath = builder.Configuration.GetValue<string>("DatabasePath") ?? "massdata.db";
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite($"Data Source={dbPath}")
       .EnableSensitiveDataLogging(builder.Environment.IsDevelopment()));

builder.Services.AddScoped<DataSeeder>();

var app = builder.Build();

// ── Auto-migrate & seed on startup ───────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db     = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    await db.Database.MigrateAsync();

    var seedCount = app.Configuration.GetValue<int>("SeedCount", 1_000_000);
    logger.LogInformation("Startup seed target: {Count:N0} records", seedCount);
    await seeder.SeedAsync(seedCount);
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MassDataAPI v1");
    c.RoutePrefix = string.Empty;
});

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", time = DateTime.UtcNow }));

app.Run();
