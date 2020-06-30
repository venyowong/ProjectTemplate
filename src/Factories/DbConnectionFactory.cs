using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Serilog;

namespace ProjectTemplate.Factories
{
    public class DbConnectionFactory
    {
        private IConfiguration config;

        public DbConnectionFactory(IConfiguration config)
        {
            this.config = config;
        }

        public IDbConnection CreateDbConnection(string clientType, string connectionName)
        {
            var key = $"{clientType}:ConnectionStrings:{connectionName}";
            var connectionString = this.config[key];
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Log.Warning($"无法获取到 {clientType} {connectionName} 对应的连接字符串，key: {key}");
                return null;
            }

            switch(clientType.ToLower())
            {
                case "mysql":
                    return this.CreateMySqlConnection(connectionString);
                case "sqlserver":
                    return this.CreateSqlServerConnection(connectionString);
            }
            
            Log.Warning($"无法识别的数据库类型: {clientType}");
            return null;
        }

        public IDbConnection CreateMySqlConnection(string connectionString)
        {
            var connectionPolicy = PollyPolicies.GetDbConnectionPolicy<MySqlConnection>();
            return connectionPolicy.ExecuteAsync(async () =>
            {
                var conn = new MySqlConnection(connectionString);
                if (conn.State != ConnectionState.Open)
                {
                    await conn.OpenAsync();
                }
                return conn;
            }).Result;
        }

        public IDbConnection CreateSqlServerConnection(string connectionString)
        {
            var connectionPolicy = PollyPolicies.GetDbConnectionPolicy<SqlConnection>();
            return connectionPolicy.ExecuteAsync(async () =>
            {
                var conn = new SqlConnection(connectionString);
                if (conn.State != ConnectionState.Open)
                {
                    await conn.OpenAsync();
                }
                return conn;
            }).Result;
        }
    }
}