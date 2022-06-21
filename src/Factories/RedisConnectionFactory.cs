using Microsoft.Extensions.Configuration;
using ProjectTemplate.Services;
using StackExchange.Redis;
using System.Collections.Concurrent;

namespace ProjectTemplate.Factories
{
    public class RedisConnectionFactory
    {
        private ConcurrentDictionary<string, ConnectionMultiplexer> connections = new ConcurrentDictionary<string, ConnectionMultiplexer>();
        private IConfiguration configuration;

        public RedisConnectionFactory(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IDatabase Create(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                return null;
            }

            if (!this.connections.TryGetValue(type, out ConnectionMultiplexer connection))
            {
                connection = ConnectionMultiplexer.Connect(this.configuration[$"Redis:{type}:ConnectionString"]);
                this.connections.TryAdd(type, connection);
            }

            int.TryParse(this.configuration[$"Redis:{type}:DefaultDb"], out int db);
            return connection.GetDatabase(db);
        }

        public RedisService CreateService(string type) => new RedisService(this.Create(type));
    }
}