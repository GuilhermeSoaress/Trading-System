using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TradingSystem.Infrastructure;
using TradingSystem.Infrastructure.Persistence;
using TradingSystem.Middlewares;

// ── Serilog bootstrap ──────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .WriteTo.Console());

    // CORS — allow frontend origins
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins(
                    "http://localhost:5173",  // Vite dev
                    "http://localhost:3000")  // Docker/nginx
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    // Controllers
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // Swagger
    builder.Services.AddSwaggerGen();

    // FluentValidation — auto-registers all validators in the assembly
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    // Infrastructure (DbContext, Repositories, Services)
    builder.Services.AddInfrastructure(builder.Configuration);

    var app = builder.Build();

    // ── Apply pending migrations on startup ────────────────────────
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    // ── Middleware pipeline ────────────────────────────────────────
    app.UseMiddleware<GlobalExceptionMiddleware>();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Polaris Exchange API v1");
        options.RoutePrefix = "swagger";
    });

    app.UseSerilogRequestLogging();
    app.UseCors();
    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    Log.Information("Polaris Exchange API starting...");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
