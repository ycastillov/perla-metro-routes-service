using PerlaMetro_RouteService.Src.Infrastructure.Db;

var builder = WebApplication.CreateBuilder(args);

// Agregamos el contexto de Neo4j como Singleton
builder.Services.AddSingleton<ApplicationDbContext>();

// Servicios de la API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// builder.Services.AddSwaggerGen();

// Neo4j
builder.Services.AddSingleton<ApplicationDbContext>();

// builder.Services.AddScoped<RouteRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // app.UseSwagger();
    // app.UseSwaggerUI();
}

// Middleware global de errores
// app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Seeder inicial (crear constraints en Neo4j)
// using (var scope = app.Services.CreateScope())
// {
//     var neo4j = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//     using var session = neo4j.GetSession();
//     await session.RunAsync("CREATE CONSTRAINT IF NOT EXISTS FOR (r:Route) REQUIRE r.Id IS UNIQUE");
// }

app.Run();
