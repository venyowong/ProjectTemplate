using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ProjectTemplate.Factories;
using ProjectTemplate.Models;
using ProjectTemplate.Services;
using Serilog;
using StackExchange.Redis;

namespace ProjectTemplate.Controllers
{
    [ApiController]
    [Route("/Http")]
    public class HttpController : Controller
    {
        private GitHubService gitHubService;

        private DbConnectionFactory dbConnectionFactory;

        private IDatabase database;

        public HttpController(GitHubService gitHubService, DbConnectionFactory dbConnectionFactory, 
            IDatabase database)
        {
            this.gitHubService = gitHubService;
            this.dbConnectionFactory = dbConnectionFactory;
            this.database = database;
        }

        [HttpGet("Get")]
        public async Task<object> Get()
        {
            Log.Information("/Http/Get");
            Log.Information(Guid.NewGuid().ToString());
            return await PollyPolicies.GetDbCommandPolicy<IEnumerable<Feed>>().ExecuteAsync(async () =>
            {
                var dbConnection = this.dbConnectionFactory.CreateDbConnection("MySql", "resader");
                if (dbConnection == null)
                {
                    return null;
                }

                using (dbConnection)
                {
                    return await dbConnection.QueryAsync<Feed>("SELECT * FROM resader.feed;");
                }
            });
        }

        [HttpGet("Github/Emojis")]
        public async Task<object> GetGithubEmojis()
        {
            return await this.gitHubService.GetEmojis();
        }

        [HttpGet("redis")]
        public object GetRedisRandomValue()
        {
            var key = Guid.NewGuid().ToString();
            var value = Guid.NewGuid().ToString();
            this.database.StringSet(key, value);
            return this.database.StringGet(key).ToString();
        }

        [HttpGet("sqlserver")]
        public object QuerySqlServer()
        {
            var dbConnection = this.dbConnectionFactory.CreateDbConnection("SqlServer", "resader");
            if (dbConnection == null)
            {
                return null;
            }

            using (dbConnection)
            {
                return dbConnection.Query("SELECT TOP 1 * FROM [master].[dbo].[OrleansQuery]");
            }
        }
    }
}