using ApiConcilacionFr.Infrastructure.Database;

using FluentValidation;
using Serilog;
using ApiConcilacionFr.Common;
using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Core.Services;
using ApiConcilacionFr.Infrastructure.Auth;
using ApiConcilacionFr.Infrastructure.Repositories;

// ── Serilog ──────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddEnvironmentVariables()
        .Build())
    .WriteTo.Console()
    .WriteTo.File("logs/api-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();






try
{
    var builder = WebApplication.CreateBuilder(args);


    builder.Host.UseSerilog();

    // ── Servicios ─────────────────────────────────────────────────────────────
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    // Connection Factory — MySQL
    builder.Services.AddSingleton<IDbConnectionFactory, MySqlConnectionFactory>();

    // Health Checks
    builder.Services.AddHealthChecks()
        .AddCheck<DatabaseHealthCheck>("mysql", tags: new[] { "db", "ready" });

    // Repositorios
    // builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
    builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
    builder.Services.AddScoped<IGrupoAsignadoRepository, GrupoAsignadoRepository>();
    builder.Services.AddScoped<IReporteRepository, ReporteRepository>();

    // Servicios de negocio
    // builder.Services.AddScoped<IProductoService, ProductoService>();
    builder.Services.AddScoped<IAuthService, AuthService>();

    // FluentValidation — auto-registro de todos los validators del ensamblado
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    // JWT (ver módulo 10)
    builder.Services.AddJwtAuthentication(builder.Configuration);

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
    });

    var app = builder.Build();

    // ── Pipeline ──────────────────────────────────────────────────────────────
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Middleware de errores SIEMPRE al inicio del pipeline
    app.UseMiddleware<ErrorHandlingMiddleware>();

    app.UseHttpsRedirection();
    app.UseCors("AllowAll");
    app.UseSerilogRequestLogging();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación falló al iniciar");
}
finally
{
    Log.CloseAndFlush();
}