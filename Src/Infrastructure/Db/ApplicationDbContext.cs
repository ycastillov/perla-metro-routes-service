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
            var uri = configuration["Neo4j:Uri"];
            var user = configuration["Neo4j:User"];
            var password = configuration["Neo4j:Password"];
            _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
        }

        public IAsyncSession GetSession()
        {
            _driver.AsyncSession();
            return _driver.AsyncSession();
        }

        public async ValueTask DisposeAsync()
        {
            await _driver.DisposeAsync();
        }
    }
}
