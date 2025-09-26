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

        public ApplicationDbContext(IConfiguration configuration)
        {
            var uri = Environment.GetEnvironmentVariable("NEO4J_URI");
            var user = Environment.GetEnvironmentVariable("NEO4J_USER");
            var password = Environment.GetEnvironmentVariable("NEO4J_PASSWORD");

            if (
                string.IsNullOrEmpty(uri)
                || string.IsNullOrEmpty(user)
                || string.IsNullOrEmpty(password)
            )
                throw new Exception("Neo4j environment variables are missing.");

            _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
        }

        public IAsyncSession GetSession()
        {
            return _driver.AsyncSession();
        }

        public async ValueTask DisposeAsync()
        {
            await _driver.DisposeAsync();
        }
    }
}
