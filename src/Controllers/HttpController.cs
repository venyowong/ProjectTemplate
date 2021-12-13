using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ProjectTemplate.Extensions;
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
        private RedisService redis;

        public HttpController(GitHubService gitHubService, DbConnectionFactory dbConnectionFactory, 
            IDatabase database, RedisService redis)
        {
            this.gitHubService = gitHubService;
            this.dbConnectionFactory = dbConnectionFactory;
            this.database = database;
            this.redis = redis;
        }

        [HttpGet("Get")]
        public async Task<object> Get()
        {
            Log.Information("/Http/Get");
            Log.Information(Guid.NewGuid().ToString());
            var dbConnection = await this.dbConnectionFactory.CreateDbConnection("MySql", "resader");
            if (dbConnection == null)
            {
                return null;
            }

            using (dbConnection)
            {
                return await dbConnection.QueryWithPolly<Feed>("SELECT * FROM resader.feed limit 1;");
            }
        }

        [HttpGet("Github/Emojis")]
        public async Task<object> GetGithubEmojis()
        {
            return await this.gitHubService.GetEmojis();
        }

        [HttpGet("redis")]
        public async Task<object> GetRedisRandomValue()
        {
            var key = Guid.NewGuid().ToString();
            var value = Guid.NewGuid().ToString();
            await this.database.StringSetWithPolly(key, value);
            return await this.database.StringGetWithPolly(key);
        }

        [HttpGet("redisservice")]
        public async Task<object> GetRedisRandomValue2()
        {
            var key = Guid.NewGuid().ToString();
            var value = Guid.NewGuid().ToString();
            await this.redis.SetAsync(key, value);
            var (val, e) = await this.redis.GetAsync<string>(key);
            if (e != null)
            {
                return "get an exception when get string from redis service";
            }

            return val;
        }

        [HttpGet("sqlserver")]
        public async Task<object> QuerySqlServer()
        {
            var dbConnection = await this.dbConnectionFactory.CreateDbConnection("SqlServer", "resader");
            if (dbConnection == null)
            {
                return null;
            }

            using (dbConnection)
            {
                return await dbConnection.QueryWithPolly<dynamic>("SELECT TOP 1 * FROM [master].[dbo].[OrleansQuery]");
            }
        }
    }
}