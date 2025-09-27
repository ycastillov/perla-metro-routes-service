using DotNetEnv;
using PerlaMetro_RouteService.Src.Infrastructure.Db;
using PerlaMetro_RouteService.Src.Interfaces;
using PerlaMetro_RouteService.Src.Mappings;
using PerlaMetro_RouteService.Src.Repositories;

// Cargar variables de entorno desde .env (solo en desarrollo)
if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Production")
{
    Env.Load();
}

var builder = WebApplication.CreateBuilder(args);

// Configuración de CORS para permitir comunicación con API Main
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        }
    );
});

// Neo4j connection como Singleton
builder.Services.AddSingleton<ApplicationDbContext>();

// Dependency Injection
builder.Services.AddScoped<IRouteRepository, RouteRepository>();

// Controllers y AutoMapper
builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(RouteMappingProfile).Assembly);

// API Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc(
        "v1",
        new()
        {
            Title = "Perla Metro Routes Service API",
            Version = "v1",
            Description = "API para gestión de rutas del sistema de transporte Perla Metro",
        }
    );
});

// Logging configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Routes Service API v1");
        c.RoutePrefix = "swagger"; // Accesible en /swagger
    });
}

// Middleware pipeline
app.UseCors("AllowAll");

app.UseAuthorization();
app.MapControllers();

app.Run();
