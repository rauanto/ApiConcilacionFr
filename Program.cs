using ApiConcilacionFr.Infrastructure.Database;

using FluentValidation;
using Serilog;
using ApiConcilacionFr.Common;
using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Core.Services;
using ApiConcilacionFr.Infrastructure.Auth;
using ApiConcilacionFr.Infrastructure.Repositories;
using Audit.Core;
using Audit.MySql;
using Audit.WebApi;



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
// Registrar el helper
builder.Services.AddSingleton<IAuditHelper, AuditHelper>();

    // Configurar Audit.NET para MySQL
    // Program.cs
    var auditConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    // Forzamos que la conexión de auditoría use el esquema correcto ignorando el default
    if (!auditConnectionString.Contains("Database=autentificacion"))
    {
        // Reemplazamos el database original por el de autentificacion solo para los logs
        auditConnectionString = auditConnectionString.Replace("creditos_fincrece_conciliacion", "autentificacion");
    }

    Audit.Core.Configuration.Setup()
        .UseMySql(config => config
            .ConnectionString(auditConnectionString)
            .TableName("auditoriaregistros") // Ya no necesitas poner el punto ni el esquema aquí
            .IdColumnName("Id")
            .JsonColumnName("Data")
            .CustomColumn("Entidad", ev => ev.CustomFields["Entidad"])
            .CustomColumn("Operacion", ev => ev.EventType)
            .CustomColumn("UsuarioResponsable", ev => ev.CustomFields["Usuario"]));

    builder.Host.UseSerilog();

    // ── Servicios ─────────────────────────────────────────────────────────────
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    // Connection Factory — MySQL
    builder.Services.AddSingleton<IDbConnectionFactory, MySqlConnectionFactory>();
    // Habilitar IHttpContextAccessor
    builder.Services.AddHttpContextAccessor(); // Habilita el acceso al HttpContext
    builder.Services.AddScoped<IAuditHelper, AuditHelper>(); // Cambiado a Scoped para usar el contexto del usuario
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