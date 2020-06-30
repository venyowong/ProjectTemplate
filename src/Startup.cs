using System;
using System.Data;
using System.Threading.Tasks;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using Polly;
using ProjectTemplate.Factories;
using ProjectTemplate.Middlewares;
using ProjectTemplate.Services;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using StackExchange.Redis;

namespace ProjectTemplate
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddSwaggerDocument();
            this.AddRateLimit(services);
            services.Configure<Configuration>(this.Configuration);

            services.AddHttpClient("Default", client =>
                {
                    client.Timeout = new TimeSpan(0, 0, 30);
                })
                .AddPolicyHandler(PollyPolicies.HttpFallBackPolicy)
                .AddPolicyHandler(PollyPolicies.HttpRetryPolicy)
                .AddPolicyHandler(PollyPolicies.HttpCircuitBreakerPolicy)
                .AddPolicyHandler(PollyPolicies.GetHttpTimeoutPolicy(new TimeSpan(0, 0, 30)));

            services.AddHttpClient<GitHubService>()
                .AddPolicyHandler(PollyPolicies.HttpFallBackPolicy)
                .AddPolicyHandler(PollyPolicies.HttpRetryPolicy)
                .AddPolicyHandler(PollyPolicies.HttpCircuitBreakerPolicy)
                .AddPolicyHandler(PollyPolicies.GetHttpTimeoutPolicy(new TimeSpan(0, 1, 0)));
            
            services.AddSingleton<DbConnectionFactory>();
            services.AddSingleton<ConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(this.Configuration["Redis:ConnectionString"]));
            services.AddTransient<IDatabase>(serviceProvider =>
            {
                int.TryParse(this.Configuration["Redis:DefaultDb"], out int db);
                return serviceProvider.GetService<ConnectionMultiplexer>().GetDatabase(db);
            });

            services.AddCors(o => o.AddPolicy("Default", builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            }));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<Configuration> configuration)
        {
            var logSwitch = new LoggingLevelSwitch();
            if (env.IsDevelopment())
            {
                logSwitch.MinimumLevel = LogEventLevel.Information;
                app.UseDeveloperExceptionPage();
                app.UseOpenApi();
                app.UseSwaggerUi3();
            }
            else
            {
                logSwitch.MinimumLevel = LogEventLevel.Warning;
            }
            app.UseStaticFiles();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(logSwitch)
                .ReadFrom.Configuration(this.Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();
            app.UseMiddleware<LogMiddleware>();

            app.UseIpRateLimiting();

            app.UseCors("Default");
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            Utility.MakeDapperMapping("ProjectTemplate.Models");
        }

        private void AddRateLimit(IServiceCollection services)
        {
            // needed to load configuration from appsettings.json
            services.AddOptions();

            // needed to store rate limit counters and ip rules
            services.AddMemoryCache();

            //load general configuration from appsettings.json
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));

            //load ip rules from appsettings.json
            services.Configure<IpRateLimitPolicies>(Configuration.GetSection("IpRateLimitPolicies"));

            // inject counter and rules stores
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();

            // https://github.com/aspnet/Hosting/issues/793
            // the IHttpContextAccessor service is not registered by default.
            // the clientId/clientIp resolvers use it.
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // configuration (resolvers, counter key builders)
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        }
    }
}
