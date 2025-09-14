using DotNetEnv;
using PerlaMetro_RouteService.Src.Infrastructure.Db;
using PerlaMetro_RouteService.Src.Interfaces;
using PerlaMetro_RouteService.Src.Mappings;
using PerlaMetro_RouteService.Src.Repositories;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Neo4j connection
builder.Services.AddSingleton<ApplicationDbContext>();

builder.Services.AddScoped<IRouteRepository, RouteRepository>();

builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(RouteMappingProfile).Assembly);
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware global de errores
// app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Seeder inicial (crear constraints en Neo4j)
using (var scope = app.Services.CreateScope())
{
    var neo4j = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    using var session = neo4j.GetSession();
    await session.RunAsync("CREATE CONSTRAINT IF NOT EXISTS FOR (r:Route) REQUIRE r.Id IS UNIQUE");
}

app.Run();
