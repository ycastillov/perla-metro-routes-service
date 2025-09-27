using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace PerlaMetro_RouteService.Src.Infrastructure.Db
{
    public class ApplicationDbContext : IAsyncDisposable
    {
        private readonly IDriver _driver;
        private readonly ILogger<ApplicationDbContext> _logger;

        public ApplicationDbContext(
            IConfiguration configuration,
            ILogger<ApplicationDbContext> logger
        )
        {
            _logger = logger;

            var uri =
                Environment.GetEnvironmentVariable("NEO4J_URI")
                ?? configuration.GetConnectionString("Neo4jConnection");
            var user =
                Environment.GetEnvironmentVariable("NEO4J_USER") ?? configuration["Neo4j:Username"];
            var password =
                Environment.GetEnvironmentVariable("NEO4J_PASSWORD")
                ?? configuration["Neo4j:Password"];

            // Validación de variables de entorno
            if (
                string.IsNullOrEmpty(uri)
                || string.IsNullOrEmpty(user)
                || string.IsNullOrEmpty(password)
            )
            {
                var missingVars = new List<string>();
                if (string.IsNullOrEmpty(uri))
                    missingVars.Add("NEO4J_URI");
                if (string.IsNullOrEmpty(user))
                    missingVars.Add("NEO4J_USER");
                if (string.IsNullOrEmpty(password))
                    missingVars.Add("NEO4J_PASSWORD");

                throw new InvalidOperationException(
                    $"Neo4j environment variables are missing: {string.Join(", ", missingVars)}"
                );
            }

            try
            {
                _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
                _logger.LogInformation("Successfully connected to Neo4j database at {Uri}", uri);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to Neo4j database at {Uri}", uri);
                throw;
            }
        }

        public IAsyncSession GetSession()
        {
            return _driver.AsyncSession();
        }

        // Método para verificar la conexión
        public async Task<bool> VerifyConnectivityAsync()
        {
            try
            {
                await _driver.VerifyConnectivityAsync();
                _logger.LogInformation("Neo4j connectivity verified successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to verify Neo4j connectivity");
                return false;
            }
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                await _driver.DisposeAsync();
                _logger.LogInformation("Neo4j driver disposed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing Neo4j driver");
            }
        }
    }
}
