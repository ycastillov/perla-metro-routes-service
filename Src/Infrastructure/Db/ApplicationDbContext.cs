using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace PerlaMetro_RouteService.Src.Infrastructure.Db
{
    /// <summary>
    /// Contexto de la base de datos para la aplicación.
    /// </summary>
    public class ApplicationDbContext : IAsyncDisposable
    {
        private readonly IDriver _driver;
        private readonly ILogger<ApplicationDbContext> _logger;

        /// <summary>
        /// Constructor para el contexto de la base de datos.
        /// </summary>
        /// <param name="configuration">Configuración de la aplicación.</param>
        /// <param name="logger">Logger para registrar información.</param>
        public ApplicationDbContext(
            IConfiguration configuration,
            ILogger<ApplicationDbContext> logger
        )
        {
            _logger = logger;

            // Obtener variables de entorno o configuración
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

            // Configurar el driver de Neo4j
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

        /// <summary>
        /// Obtiene una nueva sesión asíncrona para interactuar con la base de datos.
        /// </summary>
        public IAsyncSession GetSession()
        {
            return _driver.AsyncSession();
        }

        /// <summary>
        /// Verifica la conectividad con la base de datos Neo4j.
        /// </summary>
        /// <returns>True si la conectividad es exitosa, de lo contrario false.</returns>
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

        /// <summary>
        /// Libera los recursos del driver de Neo4j de forma asíncrona.
        /// </summary>
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
