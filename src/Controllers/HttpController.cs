using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Mvc;
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
            using var dbConnection = await this.dbConnectionFactory.CreateDbConnection("MySql", "resader");
            #if (Polly)
            return await dbConnection.QueryWithPolly<Feed>("SELECT * FROM resader.feed limit 1;");
            #else
            return await dbConnection.QueryAsync<Feed>("SELECT * FROM resader.feed limit 1;");
            #endif
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
            #if (Polly)
            await this.database.StringSetWithPolly(key, value);
            return await this.database.StringGetWithPolly(key);
            #else
            await this.database.StringSetAsync(key, value);
            return await this.database.StringGetAsync(key);
            #endif
        }

        [HttpGet("redisservice")]
        public async Task<object> GetRedisRandomValue2()
        {
            var key = Guid.NewGuid().ToString();
            var list = new List<string> { Guid.NewGuid().ToString() };
            await this.redis.SetAsync(key, list);
            var (val, e) = await this.redis.GetAsync<List<string>>(key);
            if (e != null)
            {
                return "get an exception when get object from redis service";
            }

            return val;
        }

        [HttpGet("sqlserver")]
        public async Task<object> QuerySqlServer()
        {
            using var dbConnection = await this.dbConnectionFactory.CreateDbConnection("SqlServer", "resader");
            #if (Polly)
            return await dbConnection.QueryWithPolly<dynamic>("SELECT TOP 1 * FROM [master].[dbo].[OrleansQuery]");
            #else
            return await dbConnection.QueryAsync<dynamic>("SELECT TOP 1 * FROM [master].[dbo].[OrleansQuery]");
            #endif
        }
    }
}