using ApiConcilacionFr.Infrastructure.Database;

using FluentValidation;
using Microsoft.Extensions.FileProviders;
using Serilog;
using ApiConcilacionFr.Common;
using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Core.Services;
using ApiConcilacionFr.Infrastructure.Auth;
using ApiConcilacionFr.Infrastructure.Repositories;
using ApiConcilacionFr.Infrastructure.Services;
using Audit.Core;
using Audit.MySql;
using Audit.WebApi;
using ApiConcilacionFr.Infrastructure.Hubs;



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
    // Requerido por ExcelDataReader para soportar codificaciones antiguas si el Excel las usa
    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

    var builder = WebApplication.CreateBuilder(args);
// Registrar el helper
builder.Services.AddSingleton<IAuditHelper, AuditHelper>();

    // Configurar Audit.NET para MySQL
    // Program.cs
    var auditConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    // Forzamos que la conexión de auditoría apunte al esquema 'autentificacion' donde existe la tabla de logs
    if (auditConnectionString != null && !auditConnectionString.Contains("Database=autentificacion"))
    {
        auditConnectionString = System.Text.RegularExpressions.Regex
            .Replace(auditConnectionString, @"Database=[^;]+", "Database=autentificacion");
    }

    Audit.Core.Configuration.Setup()
        .UseMySql(config => config
            .ConnectionString(auditConnectionString)
            .TableName("auditoriaregistros")
            .IdColumnName("Id")
            .JsonColumnName("Data")
            .CustomColumn("Entidad", ev => ev.CustomFields["Entidad"])
            .CustomColumn("EntidadId", ev => ev.CustomFields["EntidadId"])
            .CustomColumn("Operacion", ev => ev.EventType)
            .CustomColumn("UsuarioResponsable", ev => ev.CustomFields["Usuario"]));

    builder.Host.UseSerilog();

    // ── Servicios ─────────────────────────────────────────────────────────────
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddSignalR();
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
    builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
    builder.Services.AddScoped<IGrupoAsignadoRepository, GrupoAsignadoRepository>();
    builder.Services.AddScoped<IGrupoCobranzaRepository, GrupoCobranzaRepository>();
    builder.Services.AddScoped<IGrupoCobranzaCreditoRepository, GrupoCobranzaCreditoRepository>();
    builder.Services.AddScoped<IReporteRepository, ReporteRepository>();
    builder.Services.AddScoped<IBitacoraRepository, BitacoraRepository>();
    builder.Services.AddScoped<IBitacoraArchivoRepository, BitacoraArchivoRepository>();
    builder.Services.AddScoped<IBitacoraBajasRepository, BitacoraBajasRepository>();
    builder.Services.AddScoped<ISocioRepository, SocioRepository>();
    builder.Services.AddScoped<IRolesRepository, RolesRepository>();
    builder.Services.AddScoped<ISolicitudBajaRepository, SolicitudBajaRepository>();
    builder.Services.AddScoped<IProvisionRepository, ProvisionRepository>();

    // Servicios de negocio
    // builder.Services.AddScoped<IProductoService, ProductoService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IBitacoraService, BitacoraService>();
    builder.Services.AddScoped<IBitacoraBajasService, BitacoraBajasService>();
    builder.Services.AddScoped<ISocioService, SocioService>();
    builder.Services.AddSingleton<IFileStorageService, LocalFileStorageService>();
    builder.Services.AddScoped<INotificationService, SignalRNotificationService>();
    builder.Services.AddScoped<IRolesService, RolesService>();
    builder.Services.AddScoped<ISolicitudBajaService, SolicitudBajaService>();
    builder.Services.AddScoped<IProvisionService, ProvisionService>();
    
    // PDF Reportes y Http Client
    builder.Services.AddScoped<IReportePdfSimpleService, ReportePdfSimpleService>();
    builder.Services.AddScoped<IReportePdfCompletoService, ReportePdfCompletoService>();
    builder.Services.AddHttpClient();
    QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

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
    var carpetaStorage = builder.Configuration.GetSection("FileStorage")["RutaBase"] ?? "uploads";
    var uploadsPath = Path.IsPathRooted(carpetaStorage)
        ? carpetaStorage
        : Path.Combine(builder.Environment.ContentRootPath, carpetaStorage);
    Directory.CreateDirectory(uploadsPath);
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uploadsPath),
        RequestPath = "/uploads"
    });
    app.UseSerilogRequestLogging();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health");
    app.MapHub<NotificationHub>("/hubs/notifications");

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